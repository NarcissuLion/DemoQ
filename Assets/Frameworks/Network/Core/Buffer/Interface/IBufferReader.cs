namespace Framework.Buffer
{
    public interface IBufferReader : IBufferSeeker
    {
        byte[] ReadBytes(int count);
        byte ReadByte();
        int ReadInt();
        short ReadShort();
    }
}
