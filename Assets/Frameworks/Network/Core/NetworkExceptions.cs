using System;
using System.Net.Sockets;

#pragma warning disable 414

namespace Framework.Network
{
    public class InvalidBodyException : ApplicationException { }

    public class SessionException : ApplicationException
    {
        public SessionException(string msg) : base(msg) { }
    }

    public class SocketErrorException : ApplicationException
    {
        private SocketError _SocketError;

        public SocketErrorException(SocketError socketError)
            : base(socketError.ToString())
        {
            _SocketError = socketError;
        }
    }

    public class SessionDisconnectException : ApplicationException { }

    public class RepetitiveRegisterException : ApplicationException { }

    public class InexistenceException : ApplicationException { }

    public class AlreadyConnectException : ApplicationException { }

    public class AlreadyDisconnectException : ApplicationException { }
}
