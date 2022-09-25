using System.Runtime.InteropServices;
using PerformanceCounter.Internal;
using Unity.Profiling;

namespace PerformanceCounter
{
    [StructLayout(LayoutKind.Explicit)]
    public struct SampleValue
    {
        [FieldOffset(0)] public long LongValue;
        [FieldOffset(0)] public double DoubleValue;
    }

    public enum SamplingTarget
    {
        TotalUsedMemory,
        TotalReservedMemory,
        MainThreadTime,
        SetPassCallsCount,
        DrawCallsCount,
        TotalBatchesCount,
        VerticesCount,
    }

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
            =>  _desc.SampleSelector(_recorder);

        private SampleDesc _desc;
        private ProfilerRecorder _recorder;
    }
}
