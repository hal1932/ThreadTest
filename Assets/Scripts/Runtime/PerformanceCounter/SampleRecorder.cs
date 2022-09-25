using System;
using PerformanceCounter.Internal;

namespace PerformanceCounter
{
    public class SampleRecorder
    {
        public event EventHandler<SampleValue[]> OnRecordRequested;

        public const int SampleSize = 8;

        public bool CopyBufferOnRecordRequestd { get; set; }

        public SamplingTarget Target => _sampler.Target;
        public bool IsFull => _samples.Length == _samples.Capacity;

        public SampleRecorder(SamplingTarget target, int capacity)
            : this(new Sampler(target), capacity)
        { }

        public SampleRecorder(Sampler sampler, int capacity)
        {
            _sampler = sampler;
            _samples = new MultiBufferedList<SampleValue>(capacity, 2);
        }

        public void Start() => _sampler.Start();
        public void Stop() => _sampler.Stop();

        public void Record()
        {
            _samples.Add(_sampler.Sample());

            if (OnRecordRequested != default && IsFull)
            {
                var values = Swap();
                if (CopyBufferOnRecordRequestd)
                {
                    var valuesClone = new SampleValue[values.Length];
                    Buffer.BlockCopy(values, 0, valuesClone, 0, values.Length * SampleSize);
                    values = valuesClone;
                }
                OnRecordRequested.Invoke(this, values);
            }
        }

        public SampleValue[] Swap()
            => _samples.Swap();

        private Sampler _sampler;
        private MultiBufferedList<SampleValue> _samples;
    }
}
