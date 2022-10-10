using PerformanceCounter.Internal;

namespace PerformanceCounter
{
    public class SampleRecorder
    {
        public SamplingTarget Target => _sampler.Target;
        public bool IsFull => _samples.Length == _samples.Capacity;
        public int Capacity => _samples.Capacity;
        public SampleValue[] LastSamples => _samples.LastSwappedBuffer;

        public int CurrentLength => _samples.Length;
        public SampleValue[] CurrentSamples => _samples.CurrentBuffer;

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
        }

        public SampleValue[] Swap()
            => _samples.Swap();

        private Sampler _sampler;
        private MultiBufferedList<SampleValue> _samples;
    }
}
