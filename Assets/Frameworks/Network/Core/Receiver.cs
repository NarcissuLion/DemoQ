using System;
using System.Collections.Generic;
using Framework.Buffer;

namespace Framework.Network
{
    public abstract class Receiver
    {
        private ReceiveBuffer _ReceiveBuffer;


        public bool Receive(int msgID, int msgBodyLength, IBufferReader bufferReader, bool bCompress)
        {
            return OnReceive(msgID, msgBodyLength, bufferReader, bCompress);
        }

        public abstract void Update();

        protected abstract bool OnReceive(int msgID, int msgBodyLength, IBufferReader bufferReader,bool bCompress);
    }


    public abstract class Receiver<T0> : Receiver
    {
        private int _QueueCount
        {
            get
            {
                return _Queue0.Count;
            }
        }



        public event Action<T0> OnReceived
        {
            add
            {
                _ListAction.Add(value);
            }
            remove
            {
                _ListAction.Remove(value);
            }
        }

        private List<Action<T0>> _ListAction = new List<Action<T0>>();
        private object _QueueLock = new object();


        private Queue<T0> _Queue0 = new Queue<T0>();


        public sealed override void Update()
        {
            if (_ListAction.Count > 0)
            {
                while (_QueueCount > 0)
                {
                    T0 data0 = default(T0);
                    _Dequeue(out data0);

                    for (int i = 0; i < _ListAction.Count; ++i)
                    {
                        // try
                        // {
                            _ListAction[i](data0);
                        // }
                        // catch (Exception e)
                        // {
                        //     throw e;
                        // }
                    }
                }
            }
        }


        protected void Enqueue(T0 param0)
        {
            lock (_QueueLock)
            {
                _Queue0.Enqueue(param0);
            }
        }

        private void _Dequeue(out T0 param0)
        {
            lock (_QueueLock)
            {
                param0 = _Queue0.Dequeue();
            }
        }
    }


    public abstract class Receiver<T0, T1> : Receiver
    {
        private int _QueueCount
        {
            get
            {
                return _Queue0.Count;
            }
        }



        public event Action<T0, T1> OnReceived
        {
            add
            {
                _ListAction.Add(value);
            }
            remove
            {
                _ListAction.Remove(value);
            }
        }

        private List<Action<T0, T1>> _ListAction = new List<Action<T0, T1>>();


        private Queue<T0> _Queue0 = new Queue<T0>();
        private Queue<T1> _Queue1 = new Queue<T1>();
        private object _QueueLock = new object();


        public sealed override void Update()
        {
            if (_ListAction.Count > 0)
            {
                while (_QueueCount > 0)
                {
                    T0 data0 = default(T0);
                    T1 data1 = default(T1);
                    _Dequeue(out data0, out data1);

                    for (int i = 0; i < _ListAction.Count; ++i)
                    {
                        // try
                        // {
                            _ListAction[i](data0, data1);
                        // }
                        // catch (Exception e)
                        // {
                        //     throw e;
                        // }
                    }
                }
            }
        }


        protected void Enqueue(T0 param0, T1 param1)
        {
            lock (_QueueLock)
            {
                _Queue0.Enqueue(param0);
                _Queue1.Enqueue(param1);
            }
        }

        private void _Dequeue(out T0 param0, out T1 param1)
        {
            lock (_QueueLock)
            {
                param0 = _Queue0.Dequeue();
                param1 = _Queue1.Dequeue();
            }
        }
    }


    public abstract class Receiver<T0, T1, T2> : Receiver
    {
        private int _QueueCount
        {
            get
            {
                return _Queue0.Count;
            }
        }



        public event Action<T0, T1, T2> OnReceived
        {
            add
            {
                _ListAction.Add(value);
            }
            remove
            {
                _ListAction.Remove(value);
            }
        }

        private List<Action<T0, T1, T2>> _ListAction = new List<Action<T0, T1, T2>>();


        private Queue<T0> _Queue0 = new Queue<T0>();
        private Queue<T1> _Queue1 = new Queue<T1>();
        private Queue<T2> _Queue2 = new Queue<T2>();
        private object _QueueLock = new object();


        public sealed override void Update()
        {
            if (_ListAction.Count > 0)
            {
                while (_QueueCount > 0)
                {
                    T0 data0 = default(T0);
                    T1 data1 = default(T1);
                    T2 data2 = default(T2);
                    _Dequeue(out data0, out data1, out data2);

                    for (int i = 0; i < _ListAction.Count; ++i)
                    {
                        // try
                        // {
                            _ListAction[i](data0, data1, data2);
                        // }
                        // catch (Exception e)
                        // {
                        //     throw e;
                        // }
                    }
                }
            }
        }


        protected void Enqueue(T0 param0, T1 param1, T2 param2)
        {
            lock (_QueueLock)
            {
                _Queue0.Enqueue(param0);
                _Queue1.Enqueue(param1);
                _Queue2.Enqueue(param2);
            }
        }

        private void _Dequeue(out T0 param0, out T1 param1, out T2 param2)
        {
            lock (_QueueLock)
            {
                param0 = _Queue0.Dequeue();
                param1 = _Queue1.Dequeue();
                param2 = _Queue2.Dequeue();
            }
        }
    }
}
