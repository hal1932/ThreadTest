using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class Watcher : MonoBehaviour
{
    public void OnEnable()
    {
        _loggingCancelTokenSource = new CancellationTokenSource();
        _loggingThread = Task.Factory.StartNew(LoggingThread, _loggingCancelTokenSource.Token);
        _logSources = new LogSource[_recorders.Length];

        foreach (var recorder in _recorders)
        {
            recorder.Alloc(100);
            recorder.Start();
        }
    }

    public void OnDisable()
    {
        foreach (var recorder in _recorders)
        {
            recorder.Stop();
        }

        _loggingCancelTokenSource.Cancel();
        _logSources = default;
        _startLogging.Set();
        _loggingThread.Wait();

        _loggingCancelTokenSource.Dispose();
    }

    public void LateUpdate()
    {
        foreach (var recorder in _recorders)
        {
            recorder.Record();
        }

        if (_recorders[0].IsFull)
        {
            lock (_logSourcesLock)
            {
                for (var i = 0; i < _recorders.Length; ++i)
                {
                    _logSources[i] = new LogSource()
                    {
                        Recorder = _recorders[i],
                        Samples = _recorders[i].Swap()
                    };
                }
            }
            _startLogging.Set();
        }
    }

    private void LoggingThread()
    {
        while (!_loggingCancelTokenSource.IsCancellationRequested)
        {
            _startLogging.Wait();
            _startLogging.Reset();

            if (_logSources == default)
            {
                continue;
            }

            lock (_logSourcesLock)
            {
                foreach (var source in _logSources)
                {
                    var recorder = source.Recorder;
                    var samples = source.Samples;

                    var logger = _loggers[recorder.Category];
                    var log = new StringBuilder();
                    foreach (var sample in samples)
                    {
                        log.Append($"[{recorder.Label}] ");
                        logger(sample, log);
                    }

                    var text = log.ToString();
                    //Debug.Log(text);

                    var content = new MultipartFormDataContent();
                    content.Add(new StringContent(text, Encoding.UTF8, "text/plain"), "logs");
                    var client = new HttpClient();
                    client.PostAsync("http://127.0.0.1:5000", content)
                        .ContinueWith(res => Debug.Log(res.Result.StatusCode));
                    Debug.Log("SEND");
                }
            }
        }
    }

    private readonly SampleRecorder[] _recorders = new SampleRecorder[]
    {
        new SampleRecorder(new UsedMemorySampler()),
        new SampleRecorder(new ReservedMemorySampler()),
        new SampleRecorder(new MainThreadTimeSampler()),
    };

    private readonly Dictionary<int, Action<SampledValue, StringBuilder>> _loggers = new Dictionary<int, Action<SampledValue, StringBuilder>>()
    {
        { SampleCategory.Memory, (sample, builder) => builder.AppendLine($"{sample.LongValue / (1024 * 1024):F3} MB") },
        { SampleCategory.Time, (sample, builder) => builder.AppendLine($"{sample.DoubleValue / (1000 * 1000):F3} ms") },
    };

    private struct LogSource
    {
        public SampleRecorder Recorder;
        public SampledValue[] Samples;
    }
    private LogSource[] _logSources;
    private object _logSourcesLock = new object();

    private Task _loggingThread;
    private CancellationTokenSource _loggingCancelTokenSource;
    private ManualResetEventSlim _startLogging = new ManualResetEventSlim(false);
}
