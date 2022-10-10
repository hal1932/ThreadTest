using PerformanceCounter.Internal;
using Unity.Profiling;

namespace PerformanceCounter
{
    public class Sampler
    {
        public SamplingTarget Target { get; }

        public Sampler(SamplingTarget target)
        {
            Target = target;
            _desc = SampleDesc.Create(target);
            _recorder = default;
        }

        public void Start()
        {
            _recorder = ProfilerRecorder.StartNew(_desc.Category, _desc.StatName, _desc.Capacity);
        }

        public void Stop()
        {
            _recorder.Dispose();
        }

        public SampleValue Sample()
            =>  _desc.SelectSample(_recorder);

        private SampleDesc _desc;
        private ProfilerRecorder _recorder;
    }
}
