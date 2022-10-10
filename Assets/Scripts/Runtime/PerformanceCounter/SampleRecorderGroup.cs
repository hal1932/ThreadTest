using System;
using System.Collections.Generic;

namespace PerformanceCounter
{
    public class SampleRecorderGroup
    {
        public event EventHandler OnRecordRequested;

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

            _logger.BeginWrite(_recorders[0].CurrentLength);
            for (var i = 0; i < _targets.Length; ++i)
            {
                _logger.Write(_targets[i], _recorders[i].CurrentSamples);
            }
            _logger.EndWrite();

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
                //OnRecordRequested?.Invoke(this, EventArgs.Empty);

                _logger.BeginWrite(_recorders[0].Capacity);
                for (var i = 0; i < _targets.Length; ++i)
                {
                    _logger.Write(_targets[i], _recorders[i].LastSamples);
                }
                _logger.EndWrite();
            }
        }

        private SamplingTarget[] _targets;
        private SampleRecorder[] _recorders;
        private ILogWriter _logger;
    }
}
