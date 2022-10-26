using Framework.Buffer;
namespace Framework.Network
{
    public class SimpleReceiver : Receiver<int, byte[]>
    {
        protected override bool OnReceive(int msgID, int msgBodyLength, IBufferReader bufferReader, bool bCompress)
        {
            byte[] bytes = bufferReader.ReadBytes(msgBodyLength);
            if (bCompress) bytes = CompressUtil.UncompressBuffer(msgID, bytes);
            Enqueue(msgID, bytes);
            return true;
        }
    }
}
