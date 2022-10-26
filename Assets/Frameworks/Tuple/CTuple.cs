using System.Collections.Generic;
using System;

namespace Framework.Utils
{
    public interface ITuple : IDisposable
    {
        int length { get; }
        bool isValid { get; }
        object this[int index] { get; }
        void Close();
    }

    public abstract class CTupleBase : ITuple
    {
        int ITuple.length { get { return 0; } }
        object ITuple.this[int index] { get { return null; } }

        public bool isValid { get; protected set; }

        protected virtual void InternalDispose(bool destruction) { }

        ~CTupleBase()
        {
            InternalDispose(true);
        }

        void IDisposable.Dispose()
        {
            InternalDispose(false);
            GC.SuppressFinalize(this);
        }

        public void Close()
        {
            InternalDispose(false);
            GC.SuppressFinalize(this);
        }
    }

    public sealed class CTuple<T1> : CTupleBase, ITuple
    {
        int ITuple.length { get { return 1; } }
        public T1 t1 { get; private set; }
        private static Queue<CTuple<T1>> pool = new Queue<CTuple<T1>>();

        object ITuple.this[int index]
        {
            get
            {
                if (index != 0)
                    throw new IndexOutOfRangeException();

                return t1;
            }
        }

        private CTuple()
        {
            isValid = false;
        }

        public static CTuple<T1> Create(T1 t1)
        {
            CTuple<T1> tuple;
            lock (pool)
            {
                tuple = pool.Count > 0 ? pool.Dequeue() : new CTuple<T1>();
                tuple.t1 = t1;
                tuple.isValid = true;
            }
            return tuple;
        }

        protected override void InternalDispose(bool destruction)
        {
            if (!isValid || destruction)
                return;

            isValid = false;
            t1 = default(T1);
            lock (pool)
            {
                if (this != null) pool.Enqueue(this);
            }
        }
    }

    public sealed class CTuple<T1, T2> : CTupleBase, ITuple
    {
        int ITuple.length { get { return 2; } }
        public T1 t1 { get; private set; }
        public T2 t2 { get; private set; }
        private static Queue<CTuple<T1, T2>> pool = new Queue<CTuple<T1, T2>>();

        object ITuple.this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return t1;
                    case 1:
                        return t2;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
        }

        private CTuple()
        {
            isValid = false;
        }

        public static CTuple<T1, T2> Create(T1 t1, T2 t2)
        {
            CTuple<T1, T2> tuple;
            lock (pool)
            {
                tuple = pool.Count > 0 ? pool.Dequeue() : new CTuple<T1, T2>();
                tuple.t1 = t1;
                tuple.t2 = t2;
                tuple.isValid = true;
            }
            return tuple;
        }

        protected override void InternalDispose(bool destruction)
        {
            if (!isValid || destruction)
                return;

            isValid = false;
            t1 = default(T1);
            t2 = default(T2);
            lock (pool)
            {
                if (this != null) pool.Enqueue(this);
            }
        }
    }

    public sealed class CTuple<T1, T2, T3> : CTupleBase, ITuple
    {
        int ITuple.length { get { return 3; } }
        public T1 t1 { get; private set; }
        public T2 t2 { get; private set; }
        public T3 t3 { get; private set; }
        private static Queue<CTuple<T1, T2, T3>> pool = new Queue<CTuple<T1, T2, T3>>();

        object ITuple.this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return t1;
                    case 1:
                        return t2;
                    case 2:
                        return t3;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
        }

        private CTuple()
        {
            isValid = false;
        }

        public static CTuple<T1, T2, T3> Create(T1 t1, T2 t2, T3 t3)
        {
            CTuple<T1, T2, T3> tuple;
            lock (pool)
            {
                tuple = pool.Count > 0 ? pool.Dequeue() : new CTuple<T1, T2, T3>();
                tuple.t1 = t1;
                tuple.t2 = t2;
                tuple.t3 = t3;
                tuple.isValid = true;
            }

            return tuple;
        }

        protected override void InternalDispose(bool destruction)
        {
            if (!isValid || destruction)
                return;

            isValid = false;
            t1 = default(T1);
            t2 = default(T2);
            t3 = default(T3);
            lock (pool)
            {
                if (this != null) pool.Enqueue(this);
            }
        }
    }

    public sealed class CTuple<T1, T2, T3, T4> : CTupleBase, ITuple
    {
        int ITuple.length { get { return 4; } }
        public T1 t1 { get; private set; }
        public T2 t2 { get; private set; }
        public T3 t3 { get; private set; }
        public T4 t4 { get; private set; }
        private static Queue<CTuple<T1, T2, T3, T4>> pool = new Queue<CTuple<T1, T2, T3, T4>>();

        object ITuple.this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return t1;
                    case 1:
                        return t2;
                    case 2:
                        return t3;
                    case 3:
                        return t4;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
        }

        private CTuple()
        {
            isValid = false;
        }

        public static CTuple<T1, T2, T3, T4> Create(T1 t1, T2 t2, T3 t3, T4 t4)
        {
            CTuple<T1, T2, T3, T4> tuple;
            lock (pool)
            {
                tuple = pool.Count > 0 ? pool.Dequeue() : new CTuple<T1, T2, T3, T4>();
                tuple.t1 = t1;
                tuple.t2 = t2;
                tuple.t3 = t3;
                tuple.t4 = t4;
                tuple.isValid = true;
            }

            return tuple;
        }

        protected override void InternalDispose(bool destruction)
        {
            if (!isValid || destruction)
                return;

            isValid = false;
            t1 = default(T1);
            t2 = default(T2);
            t3 = default(T3);
            t4 = default(T4);
            lock (pool)
            {
                if (this != null) pool.Enqueue(this);
            }
        }
    }

    public sealed class CTuple<T1, T2, T3, T4, T5> : CTupleBase, ITuple
    {
        int ITuple.length { get { return 5; } }
        public T1 t1 { get; private set; }
        public T2 t2 { get; private set; }
        public T3 t3 { get; private set; }
        public T4 t4 { get; private set; }
        public T5 t5 { get; private set; }
        private static Queue<CTuple<T1, T2, T3, T4, T5>> pool = new Queue<CTuple<T1, T2, T3, T4, T5>>();

        object ITuple.this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return t1;
                    case 1:
                        return t2;
                    case 2:
                        return t3;
                    case 3:
                        return t4;
                    case 4:
                        return t5;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
        }

        private CTuple()
        {
            isValid = false;
        }

        public static CTuple<T1, T2, T3, T4, T5> Create(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5)
        {
            CTuple<T1, T2, T3, T4, T5> tuple;
            lock (pool)
            {
                tuple = pool.Count > 0 ? pool.Dequeue() : new CTuple<T1, T2, T3, T4, T5>();
                tuple.t1 = t1;
                tuple.t2 = t2;
                tuple.t3 = t3;
                tuple.t4 = t4;
                tuple.t5 = t5;
                tuple.isValid = true;
            }

            return tuple;
        }

        protected override void InternalDispose(bool destruction)
        {
            if (!isValid || destruction)
                return;

            isValid = false;
            t1 = default(T1);
            t2 = default(T2);
            t3 = default(T3);
            t4 = default(T4);
            t5 = default(T5);
            lock (pool)
            {
                if (this != null) pool.Enqueue(this);
            }
        }
    }

    public sealed class CTuple<T1, T2, T3, T4, T5, T6> : CTupleBase, ITuple
    {
        int ITuple.length { get { return 6; } }
        public T1 t1 { get; private set; }
        public T2 t2 { get; private set; }
        public T3 t3 { get; private set; }
        public T4 t4 { get; private set; }
        public T5 t5 { get; private set; }
        public T6 t6 { get; private set; }
        private static Queue<CTuple<T1, T2, T3, T4, T5, T6>> pool = new Queue<CTuple<T1, T2, T3, T4, T5, T6>>();

        object ITuple.this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return t1;
                    case 1:
                        return t2;
                    case 2:
                        return t3;
                    case 3:
                        return t4;
                    case 4:
                        return t5;
                    case 5:
                        return t6;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
        }

        private CTuple()
        {
            isValid = false;
        }

        public static CTuple<T1, T2, T3, T4, T5, T6> Create(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6)
        {   
            CTuple<T1, T2, T3, T4, T5, T6> tuple;
            lock (pool)
            {
                tuple = pool.Count > 0 ? pool.Dequeue() : new CTuple<T1, T2, T3, T4, T5, T6>();
                tuple.t1 = t1;
                tuple.t2 = t2;
                tuple.t3 = t3;
                tuple.t4 = t4;
                tuple.t5 = t5;
                tuple.t6 = t6;
                tuple.isValid = true;
            }

            return tuple;
        }

        protected override void InternalDispose(bool destruction)
        {
            if (!isValid || destruction)
                return;

            isValid = false;
            t1 = default(T1);
            t2 = default(T2);
            t3 = default(T3);
            t4 = default(T4);
            t5 = default(T5);
            t6 = default(T6);
            lock (pool)
            {
                if (this != null) pool.Enqueue(this);
            }
        }
    }

    public sealed class CTuple<T1, T2, T3, T4, T5, T6, T7> : CTupleBase, ITuple
    {
        int ITuple.length { get { return 6; } }
        public T1 t1 { get; private set; }
        public T2 t2 { get; private set; }
        public T3 t3 { get; private set; }
        public T4 t4 { get; private set; }
        public T5 t5 { get; private set; }
        public T6 t6 { get; private set; }
        public T7 t7 { get; private set; }
        private static Queue<CTuple<T1, T2, T3, T4, T5, T6, T7>> pool = new Queue<CTuple<T1, T2, T3, T4, T5, T6, T7>>();

        object ITuple.this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return t1;
                    case 1:
                        return t2;
                    case 2:
                        return t3;
                    case 3:
                        return t4;
                    case 4:
                        return t5;
                    case 5:
                        return t6;
                    case 6:
                        return t7;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
        }

        private CTuple()
        {
            isValid = false;
        }

        public static CTuple<T1, T2, T3, T4, T5, T6, T7> Create(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7)
        {
            CTuple<T1, T2, T3, T4, T5, T6, T7> tuple;
            lock (pool)
            {
                tuple = pool.Count > 0 ? pool.Dequeue() : new CTuple<T1, T2, T3, T4, T5, T6, T7>();
                tuple.t1 = t1;
                tuple.t2 = t2;
                tuple.t3 = t3;
                tuple.t4 = t4;
                tuple.t5 = t5;
                tuple.t6 = t6;
                tuple.t7 = t7;
                tuple.isValid = true;
            }

            return tuple;
        }

        private void Init(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7)
        {
        }

        protected override void InternalDispose(bool destruction)
        {
            if (!isValid || destruction)
                return;

            isValid = false;
            t1 = default(T1);
            t2 = default(T2);
            t3 = default(T3);
            t4 = default(T4);
            t5 = default(T5);
            t6 = default(T6);
            t7 = default(T7);
            lock (pool)
            {
                if (this != null) pool.Enqueue(this);
            }
        }
    }

    public sealed class CTuple<T1, T2, T3, T4, T5, T6, T7, T8> : CTupleBase, ITuple
    {
        int ITuple.length { get { return 6; } }
        public T1 t1 { get; private set; }
        public T2 t2 { get; private set; }
        public T3 t3 { get; private set; }
        public T4 t4 { get; private set; }
        public T5 t5 { get; private set; }
        public T6 t6 { get; private set; }
        public T7 t7 { get; private set; }
        public T8 t8 { get; private set; }
        private static Queue<CTuple<T1, T2, T3, T4, T5, T6, T7, T8>> pool = new Queue<CTuple<T1, T2, T3, T4, T5, T6, T7, T8>>();

        object ITuple.this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return t1;
                    case 1:
                        return t2;
                    case 2:
                        return t3;
                    case 3:
                        return t4;
                    case 4:
                        return t5;
                    case 5:
                        return t6;
                    case 6:
                        return t7;
                    case 7:
                        return t8;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
        }

        private CTuple()
        {
            isValid = false;
        }

        public static CTuple<T1, T2, T3, T4, T5, T6, T7, T8> Create(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8)
        {
            CTuple<T1, T2, T3, T4, T5, T6, T7, T8> tuple;
            lock (pool)
            {
                tuple = pool.Count > 0 ? pool.Dequeue() : new CTuple<T1, T2, T3, T4, T5, T6, T7, T8>();
                tuple.t1 = t1;
                tuple.t2 = t2;
                tuple.t3 = t3;
                tuple.t4 = t4;
                tuple.t5 = t5;
                tuple.t6 = t6;
                tuple.t7 = t7;
                tuple.t8 = t8;
                tuple.isValid = true;
            }

            return tuple;
        }

        protected override void InternalDispose(bool destruction)
        {
            if (!isValid || destruction)
                return;

            isValid = false;
            t1 = default(T1);
            t2 = default(T2);
            t3 = default(T3);
            t4 = default(T4);
            t5 = default(T5);
            t6 = default(T6);
            t7 = default(T7);
            t8 = default(T8);
            lock (pool)
            {
                if (this != null) pool.Enqueue(this);
            }
        }
    }
}
