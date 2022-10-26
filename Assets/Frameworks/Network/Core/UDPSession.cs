using System.Net.Sockets;

namespace Framework.Network
{
    public abstract class UDPSession : Session
    {
        protected override Socket CreateSocket(bool ipv6)
        {
            return new Socket(ipv6? AddressFamily.InterNetworkV6:AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        }
    }
}
