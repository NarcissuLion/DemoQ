using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Framework.Buffer;

//FIXME:去掉关于 volatile Exception _InnerException 的ref 或者out传参的警告
//interlocked确实可以是个例外，但仍需注意下
#pragma warning disable 420

namespace Framework.Network
{
    public abstract partial class Session : ISession
    {
        private enum State
        {
            Disconnected,
            Connecting,
            Connected,
        }

        private enum ReceiveReadState
        {
            Id,
            BodyLength,
            Body,
            Receiver,
        }


        public const int MSG_ID_LENGTH = 4;
        public const int MSG_BODYLENGTH_LENGTH = 10;


        private const int DEFAULT_SENDBUFFER_SIZE = 1024 * 1024;
        private const int DEFAULT_RECVBUFFER_SIZE = 1024 * 1024;


        public event Action OnConnected;
        public event Action OnDisconnected;
        public event Action OnTimeOut;


        public bool IsConnected
        {
            get
            {
                return _State == State.Connected;
            }
        }

        public bool IsDisconnected
        {
            get
            {
                return _State == State.Disconnected;
            }
        }

        public bool IsConnecting
        {
            get
            {
                return _State == State.Connecting;
            }
        }


        protected virtual int SendBufferSize
        {
            get
            {
                return DEFAULT_SENDBUFFER_SIZE;
            }
        }

        protected virtual int ReceiveBufferSize
        {
            get
            {
                return DEFAULT_RECVBUFFER_SIZE;
            }
        }


        protected volatile Socket m_Socket;

        // volatiles
        private volatile State _State;
        private volatile bool _NeedFireOnConnected;
        private volatile Exception _InnerException;

        private Dictionary<Type, Sender> _DictionarySender = new Dictionary<Type, Sender>();
        private Dictionary<Type, Receiver> _DictionaryReceiver = new Dictionary<Type, Receiver>();
        private List<Receiver> _ListReceiver = new List<Receiver>();

        private SendBuffer _SendBuffer;
        private ReceiveBuffer _ReceiveBuffer;
        private IBufferReadOperator _ReceiveBufferReadOperator;

        private Action _OnConnectedCallBack;

        private ReceiveReadState _ReceiveReadState;
        private volatile int _MsgID;
        private volatile int _MsgBodyLength;
        private volatile byte _LastMsgReceiveID = 0;//收到的extend2最后值
        private volatile byte _IsCompress = 0;//收到的extend1是否压缩 0压缩 1为压缩
        public byte GetLastMsgReceiveID()
        {
            return _LastMsgReceiveID;
        }

        private SocketAsyncEventArgs _ConnectEvtArgs = new SocketAsyncEventArgs();
        private SocketAsyncEventArgs _ReceiveEvtArgs = new SocketAsyncEventArgs();
        private SocketAsyncEventArgs _SendEvtArgs = new SocketAsyncEventArgs();


        protected Session()
        {
            //_ConnectEvtArgs.Completed += _OnConnectCompleted;

            _ReceiveBuffer = new ReceiveBuffer(ReceiveBufferSize);
            _ReceiveBufferReadOperator = _ReceiveBuffer.BufferReadOperator;
            _ReceiveEvtArgs.SetBuffer(_ReceiveBuffer.InnerBuffer, 0, 0);
            _ReceiveEvtArgs.Completed += _OnReceiveCompleted;

            _SendBuffer = new SendBuffer(SendBufferSize);
            _SendEvtArgs.SetBuffer(_SendBuffer.InnerBuffer, 0, 0);
            _SendEvtArgs.Completed += _OnSendCompleted;
        }

        public void Connect(string address, int port, Action onConnected = null)
        {
            if (address == null)
            {
                throw new ArgumentNullException("address");
            }

            if (_State != State.Disconnected)
            {
                throw new AlreadyConnectException();
            }


            if (_State == State.Connecting)
            {
                _Log("Connect Ignore:Under Connecting.");
                return;
            }

            _State = State.Connecting;

            _Log(string.Format("connecting ip: {1}, port: {2}.", GetType().Name, address, port));

            _ReceiveReadState = ReceiveReadState.Id;
            _MsgID = 0;
            _MsgBodyLength = 0;
            _IsCompress = 0;

            _SendBuffer.Reset();
            _ReceiveBuffer.Reset();
            //////连接前全部重新创建,尝试修复明明没有网络，但是还能重连成功
            _ConnectEvtArgs = new SocketAsyncEventArgs();
            _ReceiveEvtArgs = new SocketAsyncEventArgs();
            _SendEvtArgs = new SocketAsyncEventArgs();
            _ConnectEvtArgs.Completed += _OnConnectCompleted;

            _ReceiveEvtArgs.SetBuffer(_ReceiveBuffer.InnerBuffer, 0, 0);
            _ReceiveEvtArgs.Completed += _OnReceiveCompleted;

            _SendEvtArgs.SetBuffer(_SendBuffer.InnerBuffer, 0, 0);
            _SendEvtArgs.Completed += _OnSendCompleted;

            _NeedFireOnConnected = false;
            //////
            IPAddress ipAddress = IPAddress.Parse(address);
            _ConnectEvtArgs.RemoteEndPoint = new IPEndPoint(ipAddress, port);
            _OnConnectedCallBack = onConnected;
            //if (m_Socket == null)
            //{
            m_Socket = CreateSocket(ipAddress.AddressFamily==AddressFamily.InterNetworkV6);
            //}
            if (!m_Socket.ConnectAsync(_ConnectEvtArgs))
            {
                //同步的时候有可能出错
                if (_ConnectEvtArgs.SocketError != SocketError.Success)
                {
                    Interlocked.CompareExchange<Exception>(ref _InnerException, new SocketErrorException(_ConnectEvtArgs.SocketError), null);
                    return;
                }
                //同步情形也使用Try Catch
                try
                {
                    _ProcessConnected();
                }
                catch (Exception exception)
                {
                    Interlocked.CompareExchange<Exception>(ref _InnerException, exception, null);
                }
            }
        }

        public void Disconnect()
        {
            if (_State == State.Disconnected)
            {
                throw new AlreadyDisconnectException();
            }

            _State = State.Disconnected;

            try
            {
                BeforeDisconnect();
                m_Socket.Close();
            }
            catch (Exception)
            {

            }
            finally
            {
                m_Socket = null;
                _NeedFireOnConnected = false;
                _Log("Disconnected.");

                if (OnDisconnected != null)
                {
                    OnDisconnected();
                }
            }
        }


        public TSender GetSender<TSender>() 
            where TSender : Sender, new()
        {
            Sender sender;
            if (!_DictionarySender.TryGetValue(typeof(TSender), out sender))
            {
                throw new InexistenceException();
            }

            return (TSender)sender;
        }

        public TReceiver GetReceiver<TReceiver>()
            where TReceiver : Receiver, new()
        {
            Receiver receiver;
            if (!_DictionaryReceiver.TryGetValue(typeof(TReceiver), out receiver))
            {
                throw new InexistenceException();
            }

            return (TReceiver)receiver;
        }

        internal void Send()
        {
            if (_SendBuffer.ReadCount <= 0 && _SendBuffer.WrittenLineCount > 0)
            {
                _SendEvtArgs.SetBuffer(_SendBuffer.InnerBuffer, _SendBuffer.WrittenPosition, _SendBuffer.WrittenLineCount);
                _SendBuffer.IncreaseRead(_SendBuffer.WrittenLineCount);
                if (!m_Socket.SendAsync(_SendEvtArgs))
                {
                    _ProcessSended();
                }
            }
        }

        protected abstract Socket CreateSocket(bool ipv6);

        protected virtual void BeforeDisconnect() { }

        protected void RegisterSender<TSender, T0>() where TSender : Sender<T0>, new()
        {
            _RegisterSender<TSender>();
        }


        protected void RegisterSender<TSender, T0, T1>() where TSender : Sender<T0, T1>, new()
        {
            _RegisterSender<TSender>();
        }

        protected void RegisterSender<TSender, T0, T1, T2>() where TSender : Sender<T0, T1, T2>, new()
        {
            _RegisterSender<TSender>();
        }

        protected void RegisterReceiver<TReceiver, T0>() where TReceiver : Receiver<T0>, new()
        {
            _RegisterReceiver<TReceiver>();
        }

        protected void RegisterReceiver<TReceiver, T0, T1>() where TReceiver : Receiver<T0, T1>, new()
        {
            _RegisterReceiver<TReceiver>();
        }

        protected void RegisterReceiver<TReceiver, T0, T1, T2>() where TReceiver : Receiver<T0, T1, T2>, new()
        {
            _RegisterReceiver<TReceiver>();
        }

        public void OnNotReachability()
        {
            /*
            if (IsConnected)//手机不敏感，需要主动查询
            {
                UnityEngine.Debug.Log("NotReachable Disconnect");
                Disconnect();
            }
            */
        }

        internal void Update()
        {
            if (_InnerException != null)
            {// 内部错误，断开连接。
                Exception e = Interlocked.Exchange<Exception>(ref _InnerException, null);
                if (e != null)
                {
                    if (IsConnected)//FIXME:这块不对，如果是Timeout，实际没有Connect
                    {
                        UnityEngine.Debug.Log("_InnerException Disconnect:: " + e.Message);
                        Disconnect();
                    }

                    if (OnTimeOut != null)
                    {
                        if (e.Message == "TimedOut")
                            OnTimeOut();
                    }

                    throw e;
                }
            }

            if (_NeedFireOnConnected)//先处理连接上，防止反复重试
            {
                _Log("Connected.");
                _NeedFireOnConnected = false;
                _State = State.Connected;
                if (OnConnected != null)
                {
                    OnConnected();
                }
                if (_OnConnectedCallBack != null)
                {
                    _OnConnectedCallBack();
                    _OnConnectedCallBack = null;
                }
            }

            // Receiver在主线程的Update
            for (int i = 0; i < _ListReceiver.Count; ++i)
            {
                _ListReceiver[i].Update();
            }
        }

        internal void OnApplicationQuit()
        {
            if (m_Socket != null)
            {
                m_Socket.Close(0);
            }
        }


        private void _OnConnectCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (_State == State.Disconnected)
            {
                return;
            }

            if (e.SocketError != SocketError.Success)
            {
                Interlocked.CompareExchange<Exception>(ref _InnerException, new SocketErrorException(e.SocketError), null);
                return;
            }

            try
            { 
                _ProcessConnected();
            }
            catch (Exception exception)
            {
                Interlocked.CompareExchange<Exception>(ref _InnerException, exception, null);
            }
        }


        private void _OnSendCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (_State == State.Disconnected)
            {
                return;
            }

            if (e.SocketError != SocketError.Success)
            {
                Interlocked.CompareExchange<Exception>(ref _InnerException, new SocketErrorException(e.SocketError), null);
                return;
            }

            try
            { 
                _ProcessSended();
            }
            catch (Exception exception)
            {
                Interlocked.CompareExchange<Exception>(ref _InnerException, exception, null);
            }
        }


        private void _OnReceiveCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (_State == State.Disconnected)
            {
                return;
            }

            if (e.SocketError != SocketError.Success)
            {
                Interlocked.CompareExchange<Exception>(ref _InnerException, new SocketErrorException(e.SocketError), null);
                return;
            }

            try
            {
                _ProcessReceived();
            }
            catch (Exception exception)
            {
                Interlocked.CompareExchange<Exception>(ref _InnerException, exception, null);
            }
        }

        private void _ProcessConnected()
        {
            _NeedFireOnConnected = true;

            _ContinueReceive();
        }


        private void _ProcessSended()
        {
            _Log(string.Format("Send {0} bytes successful.", _SendBuffer.ReadCount));
            _SendBuffer.ClearRead();

            Send();
        }


        private void _ProcessReceived()
        {
            if (_ReceiveEvtArgs.BytesTransferred > 0)
            {
                _Log(string.Format("Receive {0} bytes successful.", _ReceiveEvtArgs.BytesTransferred));

                _ReceiveBuffer.IncreaseWritten(_ReceiveEvtArgs.BytesTransferred);

                while (true)
                {
                    if (_ReceiveReadState == ReceiveReadState.Id)
                    {
                        if (_ReceiveBuffer.WrittenCount >= MSG_ID_LENGTH)
                        {
                            _ReceiveBufferReadOperator.SetArea(_ReceiveBuffer.WrittenPosition, _ReceiveBuffer.WrittenCount);

                            _MsgID = _ReceiveBufferReadOperator.ReadInt();
                            _ReceiveReadState = ReceiveReadState.BodyLength;
                        }
                        else
                        {
                            // 数据不够，继续接收。
                            break;
                        }
                    }

                    if (_ReceiveReadState == ReceiveReadState.BodyLength)
                    {
                        if (_ReceiveBuffer.WrittenCount >= MSG_ID_LENGTH + MSG_BODYLENGTH_LENGTH)
                        {
                            _ReceiveBufferReadOperator.SetArea(_ReceiveBuffer.WrittenPosition, _ReceiveBuffer.WrittenCount);
                            /* 原始的头部格式
                            //messageId = in.readInt();
                            //encrypt   = in.readByte();
                            //compression = in.readByte();
                            //extend1 = in.readByte();
                            //extend2 = in.readByte();
                            //headLen = in.readShort();
                            */
                            //_ReceiveBufferReadOperator.SeekDelta(7);
                            _ReceiveBufferReadOperator.SeekDelta(5);
                            byte _extend1Arg = _ReceiveBufferReadOperator.ReadByte();
                            _IsCompress = _extend1Arg;
                            _ReceiveBufferReadOperator.SeekDelta(1);
                            byte _extend2Arg = _ReceiveBufferReadOperator.ReadByte();
                            _LastMsgReceiveID = _extend2Arg== 0? _LastMsgReceiveID: _extend2Arg;//extend2=0的话，skip
                            _ReceiveBufferReadOperator.SeekDelta(2);

                            _MsgBodyLength = _ReceiveBufferReadOperator.ReadInt();
                            _ReceiveReadState = ReceiveReadState.Body;
                        }
                        else
                        {
                            // 数据不够，继续接收。
                            break;
                        }
                    }

                    if (_ReceiveReadState == ReceiveReadState.Body)
                    {
                        if (_ReceiveBuffer.WrittenCount >= MSG_ID_LENGTH + MSG_BODYLENGTH_LENGTH + _MsgBodyLength)
                        {
                            _ReceiveReadState = ReceiveReadState.Receiver;
                        }
                        else
                        {
                            // 数据不够，继续接收。
                            break;
                        }
                    }

                    if (_ReceiveReadState == ReceiveReadState.Receiver)
                    {
                        // 限制只能读取Body
                        _ReceiveBufferReadOperator.SetArea(_ReceiveBuffer.WrittenPosition + MSG_ID_LENGTH + MSG_BODYLENGTH_LENGTH, _MsgBodyLength);

                        for (int i = 0; i < _ListReceiver.Count; ++i)
                        {
                            Receiver receiver = _ListReceiver[i];
                            if (receiver.Receive(_MsgID, _MsgBodyLength, _ReceiveBufferReadOperator,_IsCompress==1))
                            {
                                break;
                            }
                        }

                        _ReceiveBuffer.IncreaseRead(MSG_ID_LENGTH + MSG_BODYLENGTH_LENGTH + _MsgBodyLength);
                        _ReceiveBuffer.ClearRead();

                        _ReceiveReadState = ReceiveReadState.Id;
                    }
                }

            }

            _ContinueReceive();
        }

        private void _ContinueReceive()
        {
            if (_ReceiveBuffer.UnwrittenLineCount <= 0)
            {
                throw new SessionException("Receive buffer is not big enough.");
            }

            _ReceiveEvtArgs.SetBuffer(_ReceiveBuffer.UnwrittenPosition, _ReceiveBuffer.UnwrittenLineCount);

            //while (!m_Socket.ReceiveAsync(_ReceiveEvtArgs))
            if (!m_Socket.ReceiveAsync(_ReceiveEvtArgs))//避免接收多时造成 InvalidOperationException
            {
                if (_ReceiveEvtArgs.BytesTransferred > 0)
                {
                    _ProcessReceived();
                }
            }
        }

        private void _RegisterReceiver<TReceiver>()
            where TReceiver : Receiver, new()
        {
            Type t = typeof(TReceiver);
            if (_DictionaryReceiver.ContainsKey(t))
            {
                throw new RepetitiveRegisterException();
            }

            TReceiver receiver = new TReceiver();
            _DictionaryReceiver[t] = receiver;
            _ListReceiver.Add(receiver);
        }

        private void _RegisterSender<TSender>()
            where TSender : Sender, new()
        {
            Type t = typeof(TSender);
            if (_DictionarySender.ContainsKey(t))
            {
                throw new RepetitiveRegisterException();
            }

            TSender sender = new TSender();
            sender.Init(this, _SendBuffer);
            _DictionarySender[t] = sender;
        }


        [ConditionalAttribute("NetworkLog")]
        private void _Log(object msg)
        {
            UnityEngine.Debug.Log(string.Format("[Network] {0} {1}", GetType().Name, msg));
        }
    }

}
