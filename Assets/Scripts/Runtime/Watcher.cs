using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PerformanceCounter;
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

                    var logger = _loggers[recorder.Target];
                    var log = new StringBuilder();
                    foreach (var sample in samples)
                    {
                        log.Append($"[{recorder.Target}] ");
                        logger(sample, log);
                    }

                    var text = log.ToString();
                    //Debug.Log(text);

                    var content = new MultipartFormDataContent();
                    content.Add(new StringContent(text, Encoding.UTF8, "text/plain"), "logs");
                    _logSendingHttpClient.PostAsync("http://127.0.0.1:5000", content)
                        .ContinueWith(res => Debug.Log(res.Result.StatusCode));
                    Debug.Log("SEND");
                }
            }
        }
    }

    private readonly SampleRecorder[] _recorders = new SampleRecorder[]
    {
        new SampleRecorder(SamplingTarget.TotalUsedMemory, 100),
        new SampleRecorder(SamplingTarget.ReservedMemory, 100),
        new SampleRecorder(SamplingTarget.MainThreadTime, 100),
    };

    private readonly Dictionary<SamplingTarget, Action<SampleValue, StringBuilder>> _loggers = new Dictionary<SamplingTarget, Action<SampleValue, StringBuilder>>()
    {
        { SamplingTarget.TotalUsedMemory, (sample, builder) => builder.AppendLine($"{sample.LongValue / (1024 * 1024):F3} MB") },
        { SamplingTarget.ReservedMemory, (sample, builder) => builder.AppendLine($"{sample.LongValue / (1024 * 1024):F3} MB") },
        { SamplingTarget.MainThreadTime, (sample, builder) => builder.AppendLine($"{sample.DoubleValue / (1000 * 1000):F3} ms") },
    };

    private struct LogSource
    {
        public SampleRecorder Recorder;
        public SampleValue[] Samples;
    }
    private LogSource[] _logSources;
    private object _logSourcesLock = new object();
    private HttpClient _logSendingHttpClient = new HttpClient();

    private Task _loggingThread;
    private CancellationTokenSource _loggingCancelTokenSource;
    private ManualResetEventSlim _startLogging = new ManualResetEventSlim(false);
}
