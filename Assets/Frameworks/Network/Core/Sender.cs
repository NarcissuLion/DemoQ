using System;
using Framework.Buffer;

namespace Framework.Network
{
    public abstract class Sender
    {
        private SendBuffer _SendBuffer;
        private IBufferWriteOperator _BufferWriteOperator;
        private Session _Session;
        private Action<IBufferWriteOperator> _CustomDataProcessor;


        public void ProcessCustomData(Action<IBufferWriteOperator> f)
        {
            _CustomDataProcessor = f;
        }

        internal void Init(Session session, SendBuffer sendBuffer)
        {
            _Session = session;
            _SendBuffer = sendBuffer;
            _BufferWriteOperator = sendBuffer.BufferWriteOperator;
        }

        protected void WriteAndSend(int id)
        {
            if (!_Session.IsConnected)
            {
                throw new SessionDisconnectException();
            }

            try
            {
                _BufferWriteOperator.SetArea(_SendBuffer.UnwrittenPosition, _SendBuffer.UnwrittenCount);
                _BufferWriteOperator.Write(id);

                if (_CustomDataProcessor != null)
                {
                    _BufferWriteOperator.SetArea(_SendBuffer.UnwrittenPosition + Session.MSG_ID_LENGTH, Session.MSG_BODYLENGTH_LENGTH - 4);
                    _CustomDataProcessor(_BufferWriteOperator);
                }
                else
                {
                    _BufferWriteOperator.WriteBlank(3);
                    _BufferWriteOperator.SeekDelta(3);
                    _BufferWriteOperator.Write(_Session.GetLastMsgReceiveID());//extend2扩展为写session的最后ID
                    _BufferWriteOperator.WriteBlank(2);
                    _BufferWriteOperator.SeekDelta(2);
                    //_BufferWriteOperator.WriteBlank(Session.MSG_BODYLENGTH_LENGTH - 4);
                }

                _BufferWriteOperator.SetArea(_SendBuffer.UnwrittenPosition + Session.MSG_ID_LENGTH + Session.MSG_BODYLENGTH_LENGTH, _SendBuffer.UnwrittenCount - Session.MSG_ID_LENGTH - Session.MSG_BODYLENGTH_LENGTH);
                int bodyLength = OnWriteBody(_BufferWriteOperator);
                if (bodyLength < 0)
                {
                    throw new InvalidBodyException();
                }
                _BufferWriteOperator.SetArea(_SendBuffer.UnwrittenPosition, _SendBuffer.UnwrittenCount);

                _BufferWriteOperator.SeekDelta(Session.MSG_ID_LENGTH + 6);
                _BufferWriteOperator.Write(bodyLength);

                _SendBuffer.IncreaseWritten(Session.MSG_ID_LENGTH + Session.MSG_BODYLENGTH_LENGTH + bodyLength);

                //_SendBuffer.DebugPrint();

                _Session.Send();
            }
            catch (Exception e)
            {
                _Session.Disconnect();
                throw e;
            }
        }

        protected abstract int OnWriteBody(IBufferWriteOperator bufferWriter);
    }

    public abstract class Sender<T0> : Sender
    {
        public abstract void Send(T0 param0);
    }

    public abstract class Sender<T0, T1> : Sender
    {
        public abstract void Send(T0 param0, T1 param1);
    }

    public abstract class Sender<T0, T1, T2> : Sender
    {
        public abstract void Send(T0 param0, T1 param1, T2 param2);
    }
}
