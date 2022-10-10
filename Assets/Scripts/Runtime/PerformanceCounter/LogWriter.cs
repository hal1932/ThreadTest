using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PerformanceCounter.Internal;
using UnityEngine;

namespace PerformanceCounter
{
    public interface ILogWriter
    {
        void Start();
        void Stop();

        void BeginWrite(int valueCount);
        void Write(SamplingTarget target, SampleValue[] values);
        void EndWrite();
    }

    public class HttpLogWriter : ILogWriter
    {
        public void Start()
        {
            _client = new HttpClient();
            _sendEvent = new ManualResetEventSlim(false);
            _samplesLock = new ReaderWriterLockSlim();
            _tokenSource = new CancellationTokenSource();
            _task = Task.Factory.StartNew(SendThread, _tokenSource.Token);
        }

        public void Stop()
        {
            _tokenSource.Cancel();
            _task.Wait();

            _task.Dispose();
            _tokenSource.Dispose();
            _sendEvent.Dispose();
            _samplesLock.Dispose();
            _client.Dispose();
        }

        public void BeginWrite(int maxValueCount)
        {
            _samplesLock.EnterWriteLock();
            _maxValueCount = maxValueCount;
            if (_samples.Length < maxValueCount)
            {
                _samples = new PerformanceSample[maxValueCount];
            }
        }

        public void Write(SamplingTarget target, SampleValue[] values)
        {
            var setter = PerformanceSample.ValueSetters[target];
            for (var i = 0; i < _maxValueCount; ++i)
            {
                setter(ref _samples[i], values[i]);
            }
        }

        public void EndWrite()
        {
            _sendEvent.Set();
            _samplesLock.ExitWriteLock();
        }

        private void SendThread()
        {
            PerformanceSample[] samples;

            while (!_tokenSource.IsCancellationRequested)
            {
                try
                {
                    _sendEvent.Wait(_tokenSource.Token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                _sendEvent.Reset();

                _samplesLock.EnterWriteLock();
                {
                    samples = new PerformanceSample[_maxValueCount];
                    Array.Copy(_samples, samples, _maxValueCount);
                    _maxValueCount = 0;
                }
                _samplesLock.ExitWriteLock();

                SendSamples(samples);
            }

            if (_maxValueCount > 0)
            {
                samples = new PerformanceSample[_maxValueCount];
                Array.Copy(_samples, samples, _maxValueCount);
                SendSamples(samples);
            }
        }

        private void SendSamples(PerformanceSample[] samples)
        {
            var data = new SendData() { sequence = 0, samples = samples };
            var text = JsonUtility.ToJson(data);

            var content = new MultipartFormDataContent();
            content.Add(new StringContent(text, Encoding.UTF8, "application/json"), "logs");
            _client.PostAsync("http://127.0.0.1:5000", content).Wait();
            Debug.Log($"SEND {samples.Length}");
        }

        public class SendData
        {
            public int sequence;
            public PerformanceSample[] samples;
        }

        private PerformanceSample[] _samples = Array.Empty<PerformanceSample>();
        private int _maxValueCount;
        private ReaderWriterLockSlim _samplesLock;

        private Task _task;
        private CancellationTokenSource _tokenSource;
        private ManualResetEventSlim _sendEvent;

        private HttpClient _client;
    }

    [Serializable]
    public struct PerformanceSample
    {
        public float time;
        public float mem;
        public float cpu;
        public long setpass;
        public long draw;
        public long batch;
        public long vertex;

        public delegate void LoadValue(ref PerformanceSample sample, SampleValue value);

        public static readonly Dictionary<SamplingTarget, LoadValue> ValueSetters = new Dictionary<SamplingTarget, LoadValue>()
        {
            { SamplingTarget.TimeFromStartup, SetTime },
            { SamplingTarget.TotalUsedMemory, SetMemory },
            { SamplingTarget.MainThreadTime, SetCpuTime },
            { SamplingTarget.SetPassCallsCount, SetSetPassCalls },
            { SamplingTarget.DrawCallsCount, SetDrawCalls },
            { SamplingTarget.BatchesCount, SetBatches },
            { SamplingTarget.VerticesCount, SetVertices },
        };

        public static void SetTime(ref PerformanceSample sample, SampleValue value)
            => sample.time = (float)value.DoubleValue;

        public static void SetMemory(ref PerformanceSample sample, SampleValue value)
            => sample.mem = (float)value.DoubleValue;

        public static void SetCpuTime(ref PerformanceSample sample, SampleValue value)
            => sample.cpu = (float)value.DoubleValue;

        public static void SetSetPassCalls(ref PerformanceSample sample, SampleValue value)
            => sample.setpass = value.LongValue;

        public static void SetDrawCalls(ref PerformanceSample sample, SampleValue value)
            => sample.draw = value.LongValue;

        public static void SetBatches(ref PerformanceSample sample, SampleValue value)
            => sample.batch = value.LongValue;

        public static void SetVertices(ref PerformanceSample sample, SampleValue value)
            => sample.vertex = value.LongValue;
    }
}
