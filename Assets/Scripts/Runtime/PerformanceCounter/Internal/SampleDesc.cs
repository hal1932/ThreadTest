using System;
using System.Collections.Generic;
using Unity.Profiling;

namespace PerformanceCounter.Internal
{
    public struct SampleDesc
    {
        public ProfilerCategory Category;
        public string StatName;
        public int Capacity;
        public Func<ProfilerRecorder, SampleValue> SampleSelector;

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
                SamplingTarget.TotalUsedMemory,
                new SampleDesc
                {
                    Category = ProfilerCategory.Memory,
                    StatName = "Total Used Memory",
                    Capacity = 1,
                    SampleSelector = SampleSelectors.LongValue,
                }
            },
            {
                SamplingTarget.TotalReservedMemory,
                new SampleDesc
                {
                    Category = ProfilerCategory.Memory,
                    StatName = "GC Reserved Memory",
                    Capacity = 1,
                    SampleSelector = SampleSelectors.LongValue,
                }
            },
            {
                SamplingTarget.MainThreadTime,
                new SampleDesc
                {
                    Category = ProfilerCategory.Internal,
                    StatName = "Main Thread",
                    Capacity = 15,
                    SampleSelector = SampleSelectors.DoubleValue,
                }
            },
            {
                SamplingTarget.SetPassCallsCount,
                new SampleDesc
                {
                    Category = ProfilerCategory.Render,
                    StatName = "SetPass Calls Count",
                    Capacity = 1,
                    SampleSelector = SampleSelectors.LongValue,
                }
            },
            {
                SamplingTarget.DrawCallsCount,
                new SampleDesc
                {
                    Category = ProfilerCategory.Render,
                    StatName = "Draw Calls Count",
                    Capacity = 1,
                    SampleSelector = SampleSelectors.LongValue,
                }
            },
            {
                SamplingTarget.TotalBatchesCount,
                new SampleDesc
                {
                    Category = ProfilerCategory.Render,
                    StatName = "Total Batches Count",
                    Capacity = 1,
                    SampleSelector = SampleSelectors.LongValue,
                }
            },
            {
                SamplingTarget.VerticesCount,
                new SampleDesc
                {
                    Category = ProfilerCategory.Render,
                    StatName = "Vertices Count",
                    Capacity = 1,
                    SampleSelector = SampleSelectors.LongValue,
                }
            },
        };
    }
}
