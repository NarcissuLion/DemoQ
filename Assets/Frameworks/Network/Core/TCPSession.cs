using System;
using System.Net.Sockets;

namespace Framework.Network
{
    public abstract class TCPSession : Session
    {
        private const int SO_SENDBUF = 65535;
        private const int SO_RECVBUF = 65535;
        protected override Socket CreateSocket(bool ipv6)
        {
            Socket ssc = new Socket(ipv6? AddressFamily.InterNetworkV6 : AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ssc.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            ssc.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, SO_SENDBUF);
            ssc.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, SO_RECVBUF);
            return ssc;
        }

        protected override void BeforeDisconnect()
        {
            m_Socket.Shutdown(SocketShutdown.Both);
        }
    }
}
