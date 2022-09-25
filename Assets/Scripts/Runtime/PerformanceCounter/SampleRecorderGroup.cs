using System;
using System.Collections.Generic;

namespace PerformanceCounter
{
    public class SampleRecorderGroup
    {
        public event EventHandler OnRecordRequested;

        public int Length => _recorders.Count;

        public SampleRecorderGroup(SamplingTarget[] targets, int capacity)
        {
            _targets = new SamplingTarget[targets.Length];
            Array.Copy(targets, _targets, targets.Length);

            foreach (var target in targets)
            {
                _recorders[target] = new SampleRecorder(target, capacity);
            }
        }

        public void Start()
        {
            foreach (var recorder in _recorders.Values)
            {
                recorder.Start();
            }
        }

        public void Stop()
        {
            foreach (var recorder in _recorders.Values)
            {
                recorder.Stop();
            }
        }

        public void Record()
        {
            var isFull = false;
            foreach (var recorder in _recorders.Values)
            {
                recorder.Record();
                isFull = recorder.IsFull;
            }

            if (isFull)
            {
                foreach (var recorder in _recorders.Values)
                {
                    recorder.Swap();
                }
                OnRecordRequested?.Invoke(this, EventArgs.Empty);
            }
        }

        public SamplingTarget GetTarget(int index)
            => _targets[index];

        public SampleRecorder GetRecorder(SamplingTarget target)
            => _recorders[target];

        private SamplingTarget[] _targets;
        private Dictionary<SamplingTarget, SampleRecorder> _recorders = new Dictionary<SamplingTarget, SampleRecorder>();
    }
}
