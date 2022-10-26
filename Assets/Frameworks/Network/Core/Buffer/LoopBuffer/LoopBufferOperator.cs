using System;


namespace Framework.Buffer
{
    public partial class LoopBuffer : IBuffer
    {

        protected abstract class LoopBufferOperator : IBufferOperator
        {
            public int Position
            {
                get
                {
                    return m_Position;
                }
            }

            public int LeftCount
            {
                get
                {
                    return m_Size - m_Position;
                }
            }

            public int Size
            {
                get
                {
                    return m_Size;
                }
            }

            protected int AbsolutePosition
            {
                get
                {
                    return (m_LeftLimitPosition + m_Position) % m_Buffer.InnerBuffer.Length;
                }
            }

            protected IBuffer m_Buffer;
            protected int m_LeftLimitPosition; // 左边界。
            protected int m_Position; // 在可操作区间内的指针位置，范围[0, m_Size]。
            protected int m_Size; // 可操作大小


            public void SetBuffer(IBuffer buffer)
            {
                if (buffer == null)
                {
                    throw new ArgumentNullException("buffer");
                }

                m_Buffer = buffer;
                m_Position = 0;
                m_LeftLimitPosition = 0;
                m_Size = 0;
            }

            public void SetBuffer(IBuffer buffer, int position, int size)
            {
                if (buffer == null)
                {
                    throw new ArgumentNullException("buffer");
                }

                position %= buffer.InnerBuffer.Length;
                if (position < 0 || position > buffer.Capacity)
                {
                    throw new ArgumentOutOfRangeException("position");
                }

                if (size < 0 || size > buffer.Capacity)
                {
                    throw new ArgumentOutOfRangeException("size");
                }

                m_Buffer = buffer;
                m_Position = 0;
                m_LeftLimitPosition = position;
                m_Size = size;
            }

            public void SetArea(int position, int size)
            {
                if (m_Buffer == null)
                {
                    throw new NoBufferException();
                }

                position %= m_Buffer.InnerBuffer.Length;
                if (position < 0 || position > m_Buffer.Capacity)
                {
                    throw new ArgumentOutOfRangeException("position");
                }

                if (size < 0 || size > m_Buffer.Capacity)
                {
                    throw new ArgumentOutOfRangeException("size");
                }

                m_Position = 0;
                m_LeftLimitPosition = position;
                m_Size = size;
            }

            public void Seek(int position)
            {
                if (position < 0 || position > m_Size)
                {
                    throw new ArgumentOutOfRangeException("position");
                }

                m_Position = position;
            }

            public void SeekDelta(int delta)
            {
                int newPos = m_Position + delta;
                if (newPos < 0 || newPos > m_Size)
                {
                    throw new ArgumentOutOfRangeException("delta");
                }

                m_Position = newPos;
            }

        }

    }
}
