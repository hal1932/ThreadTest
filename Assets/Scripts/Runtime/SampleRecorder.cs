using System;

public class SampleRecorder
{
    public const int SampleSize = 8;

    public int Capacity => _bufferCapacity;
    public int Count => _bufferOffset;
    public int Category => _sampler.Category;
    public string Label => _sampler.Label;

    public bool IsFull => Count >= Capacity;

    public SampleRecorder(Sampler sampler, int bufferCount = 2)
    {
        _sampler = sampler;

        _buffers = new SampledValue[bufferCount][];
        for (var i = 0; i < _buffers.Length; ++i)
        {
            _buffers[i] = Array.Empty<SampledValue>();
        }

        _bufferOffset = 0;
        _bufferCapacity = 0;
        _bufferIndex = 0;
    }

    public void Alloc(int capacity)
    {
        for (var i = 0; i < _buffers.Length; ++i)
        {
            _buffers[i] = new SampledValue[capacity];
        }
        _bufferCapacity = capacity;
    }

    public void Start() => _sampler.Start();
    public void Stop() => _sampler.Stop();

    public void Record()
    {
        _buffers[_bufferIndex][_bufferOffset] = _sampler.Sample();
        ++_bufferOffset;
    }

    public SampledValue[] Swap()
    {
        _bufferOffset = 0;

        var lastIndex = _bufferIndex;
        ++_bufferIndex;
        if (_bufferIndex >= _buffers.Length)
        {
            _bufferIndex = 0;
        }

        return _buffers[lastIndex];
    }

    private Sampler _sampler;
    private SampledValue[][] _buffers;
    private int _bufferOffset;
    private int _bufferCapacity;
    private int _bufferIndex;
}

