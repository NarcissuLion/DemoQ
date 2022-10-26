using System;

namespace Framework.Buffer
{
    public partial class LoopBuffer : IBuffer
    {
        public byte[] InnerBuffer
        {
            get
            {
                return _Buffer;
            }
        }

        public int Capacity
        {
            get
            {
                return _Buffer.Length - 1;
            }
        }

        // Current write cursor.
        public int UnwrittenPosition
        {
            get
            {
                return _WrittenLimit;
            }
        }

        // Total size can be written on the buffer.
        public int UnwrittenCount
        {
            get
            {
                return Capacity - WrittenCount;
            }
        }

        // Size already written on the buffer.
        public int WrittenCount
        {
            get
            {
                return (_WrittenLimit + _Buffer.Length - WrittenPosition) % _Buffer.Length;
            }
        }

        // Start position to be read.
        public int WrittenPosition
        {
            get
            {
                return _WrittenPosition;
            }
        }

        // Current size that already read.
        public int ReadCount
        {
            get
            {
                return (_UnreadPosition + _Buffer.Length - _WrittenPosition) % _Buffer.Length;
            }
        }

        // Current read cursor.
        public int UnreadPosition
        {
            get
            {
                return _UnreadPosition;
            }
        }

        // Remain size unread on the buffer.
        public int UnreadCount
        {
            get
            {
                return WrittenCount - ReadCount;
            }
        }

        private volatile int _WrittenPosition;
        private volatile int _UnreadPosition;
        private volatile int _WrittenLimit;
        private byte[] _Buffer;


        public LoopBuffer(int capacity)
        {
            _Buffer = new byte[capacity + 1];
            Reset();
        }


        public void Reset()
        {
            _WrittenPosition = 0;
            _WrittenLimit = 0;
            _UnreadPosition = 0;
        }

        public virtual void IncreaseWritten(int count)
        {
            if (count < 0)
            {
                throw new ArgumentException("count can't be negative.");
            }

            if (count > UnwrittenCount)
            {
                throw new BufferOutOfRangeException();
            }

            _WrittenLimit = (_WrittenLimit + count) % _Buffer.Length;
        }

        public void IncreaseRead(int count)
        {
            if (count < 0)
            {
                throw new ArgumentException("count can't be negative.");
            }
            if (count > UnreadCount)
            {
                throw new BufferOutOfRangeException();
            }

            _UnreadPosition = (_UnreadPosition + count) % _Buffer.Length;
        }

        public void ClearRead()
        {
            _WrittenPosition = _UnreadPosition;
        }

        protected virtual IBufferReadOperator CreateBufferReadOperator()
        {
            LoopBufferReadOperator bufferReadOperator = new LoopBufferReadOperator();
            bufferReadOperator.SetBuffer(this);
            return bufferReadOperator;
        }

        protected virtual IBufferWriteOperator CreateBufferWriteOperator()
        {
            LoopBufferWriteOperator bufferWriteOperator = new LoopBufferWriteOperator();
            bufferWriteOperator.SetBuffer(this);
            return bufferWriteOperator;
        }

    }
}
