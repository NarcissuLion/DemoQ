using System;

namespace Framework.Buffer
{
    public interface IBufferWriter : IBufferSeeker
    {
        void Write(short data);
        void Write(int data);
        void Write(byte[] data);
        void Write(byte data);
        void Write(IntPtr source, int count);
        void WriteBlank(int count);
    }
}
