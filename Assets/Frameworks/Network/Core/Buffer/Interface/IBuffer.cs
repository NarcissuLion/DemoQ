using System;


namespace Framework.Buffer
{
    public interface IBuffer
    {
        byte[] InnerBuffer { get; }
        int Capacity { get; }

        int UnwrittenPosition { get; }
        int UnwrittenCount { get; }

        int WrittenPosition { get; }
        int WrittenCount { get; }

        int ReadCount { get; }
        
        int UnreadPosition { get; }
        int UnreadCount { get; }

        void Reset();

        void IncreaseWritten(int count);
        void IncreaseRead(int count);
        void ClearRead();
    }
}
