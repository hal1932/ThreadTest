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

        public void SetLogger(ILogWriter logger)
        {
            _logger = logger;
        }

        public void Alloc(int capacity)
        {
            _recorders = new SampleRecorder[_targets.Length];
            for (var i = 0; i < _targets.Length; ++i)
            {
                _recorders[i] = new SampleRecorder(_targets[i], capacity);
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

            var count = _recorders.Max(rec => rec.CurrentLength);
            WriteLog(count, rec => rec.CurrentSamples);

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


            if (isFull)
            {
                foreach (var recorder in _recorders)
                {
                    recorder.Swap();
                }

                var count = _recorders.Max(rec => rec.Capacity);
                WriteLog(count, rec => rec.LastSamples);
            }
        }

        private void WriteLog(int valueCount, Func<SampleRecorder, SampleValue[]> valuesSelector)
        {
            _logger.BeginWrite(valueCount);
            for (var i = 0; i < _targets.Length; ++i)
            {
                _logger.Write(_targets[i], valuesSelector(_recorders[i]));
            }
            _logger.EndWrite();
        }

        private SamplingTarget[] _targets;
        private SampleRecorder[] _recorders;
        private ILogWriter _logger;
    }
}
