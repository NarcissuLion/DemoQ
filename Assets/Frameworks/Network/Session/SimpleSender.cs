using Framework.Buffer;

namespace Framework.Network
{
    public class SimpleSender : Sender<int, byte[]>
    {
        private byte[] _Bytes;

        public override void Send(int msgID, byte[] msgBody)
        {
            _Bytes = msgBody;
            WriteAndSend(msgID);
        }

        protected override int OnWriteBody(IBufferWriteOperator bufferWriter)
        {
            bufferWriter.Write(_Bytes);
            return _Bytes.Length;
        }
    }
}
