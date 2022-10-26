namespace Framework.Buffer
{
    public interface IBufferOperator : IBufferSeeker
    {
        void SetArea(int position, int size);
    }
}
