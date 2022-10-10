namespace PerformanceCounter.Internal
{
    public class MultiBufferedList<T>
    {
        public int BufferCount => _buffers.Length;
        public int Capacity => _buffers[0].Length;
        public int Length => _itemOffset;
        public T[] LastSwappedBuffer => _buffers[_lastBufferIndex];
        public T[] CurrentBuffer => _buffers[_bufferIndex];

        public T this[int index]
        {
            get => _buffers[_bufferIndex][index];
            set => _buffers[_bufferIndex][index] = value;
        }

        public MultiBufferedList(int capacity, int bufferCount)
        {
            _buffers = new T[bufferCount][];
            Resize(capacity);
        }

        public void Resize(int capacity)
        {
            for (var i = 0; i < _buffers.Length; ++i)
            {
                _buffers[i] = new T[capacity];
            }
        }

        public void Add(T item)
        {
            this[_itemOffset] = item;
            ++_itemOffset;
        }

        public bool TryAdd(T item)
        {
            if (_itemOffset >= Capacity)
            {
                return false;
            }
            Add(item);
            return true;
        }

        public void Clear()
        {
            _itemOffset = 0;
        }

        public T[] Swap()
        {
            _lastBufferIndex = _bufferIndex;
            ++_bufferIndex;
            if (_bufferIndex >= _buffers.Length)
            {
                _bufferIndex = 0;
            }
            Clear();
            return _buffers[_lastBufferIndex];
        }

        private T[][] _buffers;
        private int _bufferIndex;
        private int _lastBufferIndex;
        private int _itemOffset;
    }
}
