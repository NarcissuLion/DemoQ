
namespace Framework.Network
{
    public class SimpleSession: TCPSession
    {
        public SimpleSender sender { get { return this.GetSender<SimpleSender>(); } }
        public SimpleReceiver receiver { get { return this.GetReceiver<SimpleReceiver>(); } }

        public SimpleSession()
        {
            RegisterReceiver<SimpleReceiver, int, byte[]>();
            RegisterSender<SimpleSender, int, byte[]>();
        }
    }
}
