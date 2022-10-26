using System;
using Framework.Buffer;

namespace Framework.Network
{
    internal class ReceiveBuffer: LoopBuffer
    {
        public IBufferReadOperator BufferReadOperator
        {
            get
            {
                return _BufferReadOperator;
            }
        }

        public int UnwrittenLineCount
        {
            get
            {
                return Math.Min(base.UnwrittenCount, InnerBuffer.Length - UnwrittenPosition);
            }
        }


        private IBufferReadOperator _BufferReadOperator;


        public ReceiveBuffer(int capacity)
            : base(capacity)
        {
            _BufferReadOperator = CreateBufferReadOperator();
        }
    }
}
