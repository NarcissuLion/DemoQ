using System;

namespace Framework.Buffer
{
    public interface IBufferSeeker
    {
        int Position { get; }
        int LeftCount { get; }
        int Size { get; }

        void Seek(int position);
        void SeekDelta(int delta);
    }
}
