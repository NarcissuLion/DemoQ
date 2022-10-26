using System;

namespace Framework.Buffer
{
    public partial class LoopBuffer : IBuffer
    {

        protected class LoopBufferReadOperator : LoopBufferOperator, IBufferReadOperator
        {
            public byte[] ReadBytes(int count)
            {
                if (LeftCount < count)
                {
                    throw new BufferOutOfRangeException();
                }

                byte[] bytes = new byte[count];
                int leftLineSize = m_Buffer.InnerBuffer.Length - AbsolutePosition;
                if (leftLineSize >= count)
                {
                    Array.Copy(m_Buffer.InnerBuffer, AbsolutePosition, bytes, 0, count);
                }
                else
                {
                    Array.Copy(m_Buffer.InnerBuffer, AbsolutePosition, bytes, 0, leftLineSize);
                    Array.Copy(m_Buffer.InnerBuffer, 0, bytes, leftLineSize, count - leftLineSize);
                }

                SeekDelta(count);
                return bytes;
            }
            public byte ReadByte()
            {
                if (LeftCount < 1)
                {
                    throw new BufferOutOfRangeException();
                }

                byte bytes = m_Buffer.InnerBuffer[AbsolutePosition];
                SeekDelta(1);
                return bytes;
            }

            public int ReadInt()
            {
                if (LeftCount < 4)
                {
                    throw new BufferOutOfRangeException();
                }

                int ret = 0;

                ret |= m_Buffer.InnerBuffer[AbsolutePosition] << 24;
                SeekDelta(1);
                ret |= m_Buffer.InnerBuffer[AbsolutePosition] << 16;
                SeekDelta(1);
                ret |= m_Buffer.InnerBuffer[AbsolutePosition] << 8;
                SeekDelta(1);
                ret |= m_Buffer.InnerBuffer[AbsolutePosition];
                SeekDelta(1);

                return ret;
            }

            public short ReadShort()
            {
                if (LeftCount < 2)
                {
                    throw new BufferOutOfRangeException();
                }

                short ret = 0;

                ret |= (short)(m_Buffer.InnerBuffer[AbsolutePosition] << 8);
                SeekDelta(1);
                ret |= (short)(m_Buffer.InnerBuffer[AbsolutePosition]);
                SeekDelta(1);

                return ret;
            }
        }

    }
}
