using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Profiling;

namespace PerformanceCounter.Internal
{
    [StructLayout(LayoutKind.Explicit)]
    public struct SampleValue
    {
        [FieldOffset(0)] public long LongValue;
        [FieldOffset(0)] public double DoubleValue;
    }

    public enum SamplingTarget
    {
        TimeFromStartup = 0,
        TotalUsedMemory,
        TotalReservedMemory,
        MainThreadTime,
        SetPassCallsCount,
        DrawCallsCount,
        BatchesCount,
        VerticesCount,
    }

    public struct SampleDesc
    {
        public delegate SampleValue OnSelectSample(ProfilerRecorder recorder);

        public ProfilerCategory Category;
        public string StatName;
        public int Capacity;
        public OnSelectSample SelectSample;

        public static SampleDesc Create(SamplingTarget target)
        {
            if (!_templates.TryGetValue(target, out var desc))
            {
                throw new ArgumentException();
            }
            return desc;
        }

        private static readonly Dictionary<SamplingTarget, SampleDesc> _templates = new Dictionary<SamplingTarget, SampleDesc>()
        {
            {
                SamplingTarget.TimeFromStartup,
                new SampleDesc
                {
                    Category = default,
                    StatName = "Time From Startup",
                    Capacity = 1,
                    SelectSample = SampleSelectors.TimeValue,
                }
            },
            {
                SamplingTarget.TotalUsedMemory,
                new SampleDesc
                {
                    Category = ProfilerCategory.Memory,
                    StatName = "Total Used Memory",
                    Capacity = 1,
                    SelectSample = SampleSelectors.LongValue,
                }
            },
            {
                SamplingTarget.MainThreadTime,
                new SampleDesc
                {
                    Category = ProfilerCategory.Internal,
                    StatName = "Main Thread",
                    Capacity = 15,
                    SelectSample = SampleSelectors.DoubleValue,
                }
            },
            {
                SamplingTarget.SetPassCallsCount,
                new SampleDesc
                {
                    Category = ProfilerCategory.Render,
                    StatName = "SetPass Calls Count",
                    Capacity = 1,
                    SelectSample = SampleSelectors.LongValue,
                }
            },
            {
                SamplingTarget.DrawCallsCount,
                new SampleDesc
                {
                    Category = ProfilerCategory.Render,
                    StatName = "Draw Calls Count",
                    Capacity = 1,
                    SelectSample = SampleSelectors.LongValue,
                }
            },
            {
                SamplingTarget.BatchesCount,
                new SampleDesc
                {
                    Category = ProfilerCategory.Render,
                    StatName = "Batches Count",
                    Capacity = 1,
                    SelectSample = SampleSelectors.LongValue,
                }
            },
            {
                SamplingTarget.VerticesCount,
                new SampleDesc
                {
                    Category = ProfilerCategory.Render,
                    StatName = "Vertices Count",
                    Capacity = 1,
                    SelectSample = SampleSelectors.LongValue,
                }
            },
        };
    }
}
