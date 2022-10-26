using System;
using System.Runtime.InteropServices;

namespace Framework.Buffer
{
    public partial class LoopBuffer : IBuffer
    {

        protected class LoopBufferWriteOperator : LoopBufferOperator, IBufferWriteOperator
        {
            public void Write(short data)
            {
                if (LeftCount < 2)
                {
                    throw new BufferOutOfRangeException();
                }

                m_Buffer.InnerBuffer[AbsolutePosition] = (byte)(data >> 8);
                SeekDelta(1);
                m_Buffer.InnerBuffer[AbsolutePosition] = (byte)data;
                SeekDelta(1);
            }

            public void Write(byte[] data)
            {
                if (LeftCount < data.Length)
                {
                    throw new BufferOutOfRangeException();
                }

                int leftLineSize = m_Buffer.InnerBuffer.Length - AbsolutePosition;
                if (leftLineSize >= data.Length)
                {
                    Array.Copy(data, 0, m_Buffer.InnerBuffer, AbsolutePosition, data.Length);
                }
                else
                {
                    Array.Copy(data, 0, m_Buffer.InnerBuffer, AbsolutePosition, leftLineSize);
                    Array.Copy(data, leftLineSize, m_Buffer.InnerBuffer, AbsolutePosition + leftLineSize, data.Length - leftLineSize);
                }
            }

            public void Write(int data)
            {
                if (LeftCount < 4)
                {
                    throw new BufferOutOfRangeException();
                }

                m_Buffer.InnerBuffer[AbsolutePosition] = (byte)(data >> 24);
                SeekDelta(1);
                m_Buffer.InnerBuffer[AbsolutePosition] = (byte)(data >> 16);
                SeekDelta(1);
                m_Buffer.InnerBuffer[AbsolutePosition] = (byte)(data >> 8);
                SeekDelta(1);
                m_Buffer.InnerBuffer[AbsolutePosition] = (byte)data;
                SeekDelta(1);
            }

            public void Write(byte data)
            {
                if (LeftCount < 1)
                {
                    throw new BufferOutOfRangeException();
                }

                m_Buffer.InnerBuffer[AbsolutePosition] = data;
                SeekDelta(1);
            }

            public void Write(IntPtr source, int count)
            {
                if (LeftCount < count)
                {
                    throw new BufferOutOfRangeException();
                }

                int leftLineSize = m_Buffer.InnerBuffer.Length - AbsolutePosition;
                if (leftLineSize >= count)
                {
                    Marshal.Copy(source, m_Buffer.InnerBuffer, AbsolutePosition, count);
                }
                else
                {
                    Marshal.Copy(source, m_Buffer.InnerBuffer, AbsolutePosition, leftLineSize);
                    IntPtr newAddr = new IntPtr(source.ToInt32() + leftLineSize);
                    Marshal.Copy(newAddr, m_Buffer.InnerBuffer, 0, count - leftLineSize);
                }
            }

            public void WriteBlank(int count)
            {
                if (LeftCount < count)
                {
                    throw new BufferOutOfRangeException();
                }

                int leftLineSize = m_Buffer.InnerBuffer.Length - AbsolutePosition;
                if (leftLineSize >= count)
                {
                    Array.Clear(m_Buffer.InnerBuffer, AbsolutePosition, count);
                }
                else
                {
                    Array.Clear(m_Buffer.InnerBuffer, AbsolutePosition, leftLineSize);
                    Array.Clear(m_Buffer.InnerBuffer, 0, count - leftLineSize);
                }
            }
        }

    }
}
