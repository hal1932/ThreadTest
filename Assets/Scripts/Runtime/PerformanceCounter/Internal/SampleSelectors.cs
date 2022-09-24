using Unity.Profiling;

namespace PerformanceCounter.Internal
{
    public class SampleSelectors
    {
        public static SampleValue LongValue(ProfilerRecorder recorder)
        {
            var sampleCount = recorder.Capacity;

            if (sampleCount == 1)
            {
                return new SampleValue { LongValue = recorder.LastValue };
            }

            if (sampleCount == 0)
            {
                return default;
            }

            var value = SumSamples(recorder, sampleCount);
            return new SampleValue { LongValue = value /= sampleCount };
        }

        public static SampleValue DoubleValue(ProfilerRecorder recorder)
        {
            var sampleCount = recorder.Capacity;

            if (sampleCount == 1)
            {
                return new SampleValue { DoubleValue = recorder.LastValueAsDouble };
            }

            if (sampleCount == 0)
            {
                return default;
            }

            var value = SumSamples(recorder, sampleCount);
            return new SampleValue { DoubleValue = (double)value / sampleCount };
        }

        private static long SumSamples(ProfilerRecorder recorder, int sampleCount)
        {
            var value = 0L;
            unsafe
            {
                var samples = stackalloc ProfilerRecorderSample[sampleCount];
                recorder.CopyTo(samples, sampleCount);
                for (var i = 0; i < sampleCount; ++i)
                {
                    value += samples[i].Value;
                }
            }
            return value;
        }
    }
}
