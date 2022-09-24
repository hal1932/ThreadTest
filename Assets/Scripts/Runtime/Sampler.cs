using System.Runtime.InteropServices;
using Unity.Profiling;

[StructLayout(LayoutKind.Explicit)]
public struct SampledValue
{
    [FieldOffset(0)] public long LongValue;
    [FieldOffset(0)] public double DoubleValue;
}

public static class SampleCategory
{
    public static readonly int Memory = ProfilerCategory.Memory;
    public static readonly int Time = ProfilerCategory.Internal;
}

public abstract class Sampler
{
    public int Category => _category;
    public string Label => _label;

    protected ProfilerRecorder Recorder;

    protected Sampler(ProfilerCategory category, string label, int capacity = 1)
    {
        _category = category;
        _label = label;
        _capacity = capacity;
    }

    public void Start()
    {
        Recorder = ProfilerRecorder.StartNew(_category, _label, _capacity);
    }

    public void Stop()
    {
        Recorder.Dispose();
    }

    public abstract SampledValue Sample();

    private ProfilerCategory _category;
    private string _label;
    private int _capacity;
}

public class UsedMemorySampler : Sampler
{
    public UsedMemorySampler()
        : base(ProfilerCategory.Memory, "Total Used Memory")
    { }

    public override SampledValue Sample()
        => new SampledValue() { LongValue = Recorder.LastValue };
}

public class ReservedMemorySampler : Sampler
{
    public ReservedMemorySampler()
        : base(ProfilerCategory.Memory, "GC Reserved Memory")
    { }

    public override SampledValue Sample()
        => new SampledValue() { LongValue = Recorder.LastValue };
}

public class MainThreadTimeSampler : Sampler
{
    public MainThreadTimeSampler()
        : base(ProfilerCategory.Internal, "Main Thread", 15)
    { }

    public override SampledValue Sample()
    {
        var sampleCount = Recorder.Capacity;
        if (sampleCount == 0)
        {
            return default;
        }

        var result = 0.0;
        unsafe
        {
            var samples = stackalloc ProfilerRecorderSample[sampleCount];
            Recorder.CopyTo(samples, sampleCount);
            for (var i = 0; i < sampleCount; ++i)
            {
                result += samples[i].Value;
            }
            result /= sampleCount;
        }
        return new SampledValue() { DoubleValue = result };
    }
}
