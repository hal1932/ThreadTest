using System;
using System.Linq;
using PerformanceCounter.Internal;

namespace PerformanceCounter
{
    public class SampleRecorderGroup
    {
        public SampleRecorderGroup(params SamplingTarget[] targets)
        {
            _targets = new SamplingTarget[targets.Length];
            Array.Copy(targets, _targets, targets.Length);
        }

        public void SetLogger(ISampleLogger logger)
        {
            _logger = logger;
        }

        public void Alloc(int capacity)
        {
            _sampleCapacity = capacity;
            _recorders = new SampleRecorder[_targets.Length];
            for (var i = 0; i < _targets.Length; ++i)
            {
                _recorders[i] = new SampleRecorder(_targets[i], _sampleCapacity);
            }
        }

        public void Start()
        {
            _logger.Start();
            foreach (var recorder in _recorders)
            {
                recorder.Start();
            }
        }

        public void Stop()
        {
            foreach (var recorder in _recorders)
            {
                recorder.Stop();
            }

            WriteLog(_sampleCount, rec => rec.CurrentSamples);

            _logger.Stop();
        }

        public void Record()
        {
            var isFull = false;
            foreach (var recorder in _recorders)
            {
                recorder.Record();
                isFull = recorder.IsFull;
            }
            ++_sampleCount;


            if (isFull)
            {
                foreach (var recorder in _recorders)
                {
                    recorder.Swap();
                }
                _sampleCount = 0;

                WriteLog(_sampleCapacity, rec => rec.LastSamples);
            }
        }

        private void WriteLog(int valueCount, Func<SampleRecorder, SampleValue[]> samplesSelector)
        {
            using (var writer = _logger.CreateWriter(valueCount))
            {
                for (var i = 0; i < _targets.Length; ++i)
                {
                    writer.Write(_targets[i], samplesSelector(_recorders[i]));
                }
            }
        }

        private SamplingTarget[] _targets;
        private SampleRecorder[] _recorders;
        private ISampleLogger _logger;

        private int _sampleCapacity;
        private int _sampleCount;
    }
}
