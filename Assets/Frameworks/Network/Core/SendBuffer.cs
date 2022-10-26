using Framework.Buffer;
using System;

namespace Framework.Network
{

    internal class SendBuffer : LoopBuffer
    {
        public IBufferWriteOperator BufferWriteOperator
        {
            get
            {
                return _BufferWriteOperator;
            }
        }

        public int WrittenLineCount
        {
            get
            {
                return Math.Min(base.WrittenCount, InnerBuffer.Length - WrittenPosition);
            }
        }
        /*
        public void DebugPrint()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("[");
            for (int i= WrittenPosition; i<WrittenCount;++i)
            {
                sb.Append(InnerBuffer[i % InnerBuffer.Length]);
                sb.Append(",");
            }
            sb.Append("]");
            UnityEngine.Debug.LogWarning(sb.ToString());
        }
        */

        private IBufferWriteOperator _BufferWriteOperator;


        public SendBuffer(int capacity)
            : base(capacity)
        {
            _BufferWriteOperator = CreateBufferWriteOperator();
        }
    }

}
