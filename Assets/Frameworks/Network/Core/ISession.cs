using System;

namespace Framework.Network
{
    public interface ISession
    {
        event Action OnConnected;
        event Action OnDisconnected;

        bool IsConnected { get; }


        void Connect(string address, int port, Action onConnected = null);
        void Disconnect();

        TSender GetSender<TSender>() where TSender : Sender, new();
        TReceiver GetReceiver<TReceiver>() where TReceiver : Receiver, new();
    }
}
