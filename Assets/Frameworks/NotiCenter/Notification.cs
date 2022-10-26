using System.Collections.Generic;
using System.Text;

namespace Framework.Noti
{
    public interface INotification
    {
        string name { get; }
        void Release();
    }

    public class Notification : INotification
    {
        protected static Queue<Notification> m_cache = new Queue<Notification>();
        protected static StringBuilder m_stringBuilder = new StringBuilder();

        protected string m_name;
        public string name { get { return m_name; } set { m_name = value; } }

        protected bool m_useCache;

        public Notification(bool useCache) { this.m_useCache = useCache; }

        public static Notification Create(string name, bool useCache)
        {
            Notification noti;
            lock (m_cache)
            {
                if (!useCache || m_cache.Count <= 0) noti = new Notification(useCache);
                else noti = m_cache.Dequeue();

                noti.name = name;
            }

            return noti;
        }

        public virtual void Release()
        {
            if (!this.m_useCache) return;

            lock (m_cache)
            {
                this.name = string.Empty;
                m_cache.Enqueue(this);
            }
        }
    }

    public class Notification<T> : Notification
    {
        new protected static Queue<Notification<T>> m_cache = new Queue<Notification<T>>();

        public T data;

        public Notification(bool useCache) : base(useCache)
        {
        }

        public static Notification<T> Create(string name, T data, bool useCache)
        {
            Notification<T> noti;
            lock (m_cache)
            {
                if (!useCache || m_cache.Count <= 0) noti = new Notification<T>(useCache);
                else noti = m_cache.Dequeue();

                noti.name = name;
                noti.data = data;
            }

            return noti;
        }
        public override void Release()
        {
            if (!this.m_useCache) return;

            lock (m_cache)
            {
                this.name = string.Empty;
                this.data = default(T);
                m_cache.Enqueue(this);
            }
        }
    }

    public class Notification<T1, T2> : Notification
    {        
        new protected static Queue<Notification<T1, T2>> m_cache = new Queue<Notification<T1, T2>>();

        public T1 data1;
        public T2 data2;

        public Notification(bool useCache) : base(useCache)
        {
        }

        public static Notification<T1, T2> Create(string name, T1 data1, T2 data2, bool useCache)
        {
            Notification<T1, T2> noti;
            lock (m_cache)
            {
                if (!useCache || m_cache.Count <= 0) noti = new Notification<T1, T2>(useCache);
                else noti = m_cache.Dequeue();

                noti.name = name;
                noti.data1 = data1;
                noti.data2 = data2;
            }

            return noti;
        }
        public override void Release()
        {
            if (!this.m_useCache) return;

            lock (m_cache)
            {
                this.name = string.Empty;
                this.data1 = default(T1);
                this.data2 = default(T2);
                m_cache.Enqueue(this);
            }
        }
    }

    public class Notification<T1, T2, T3> : Notification
    {
        new protected static Queue<Notification<T1, T2, T3>> m_cache = new Queue<Notification<T1, T2, T3>>();

        public T1 data1;
        public T2 data2;
        public T3 data3;

        public Notification(bool useCache) : base(useCache)
        {
        }

        public static Notification<T1, T2, T3> Create(string name, T1 data1, T2 data2, T3 data3, bool useCache)
        {
            Notification<T1, T2, T3> noti;
            lock (m_cache)
            {
                if (!useCache || m_cache.Count <= 0) noti = new Notification<T1, T2, T3>(useCache);
                else noti = m_cache.Dequeue();

                noti.name = name;
                noti.data1 = data1;
                noti.data2 = data2;
                noti.data3 = data3;
            }

            return noti;
        }
        public override void Release()
        {
            if (!this.m_useCache) return;

            lock (m_cache)
            {
                this.name = string.Empty;
                this.data1 = default(T1);
                this.data2 = default(T2);
                this.data3 = default(T3);
                m_cache.Enqueue(this);
            }
        }
    }

    public class Notification<T1, T2, T3, T4> : Notification
    {
        new protected static Queue<Notification<T1, T2, T3, T4>> m_cache = new Queue<Notification<T1, T2, T3, T4>>();

        public T1 data1;
        public T2 data2;
        public T3 data3;
        public T4 data4;

        public Notification(bool useCache) : base(useCache)
        {
        }

        public static Notification<T1, T2, T3, T4> Create(string name, T1 data1, T2 data2, T3 data3, T4 data4, bool useCache)
        {
            Notification<T1, T2, T3, T4> noti;
            lock (m_cache)
            {
                if (!useCache || m_cache.Count <= 0) noti = new Notification<T1, T2, T3, T4>(useCache);
                else noti = m_cache.Dequeue();

                noti.name = name;
                noti.data1 = data1;
                noti.data2 = data2;
                noti.data3 = data3;
                noti.data4 = data4;
            }

            return noti;
        }
        public override void Release()
        {
            if (!this.m_useCache) return;

            lock (m_cache)
            {
                this.name = string.Empty;
                this.data1 = default(T1);
                this.data2 = default(T2);
                this.data3 = default(T3);
                this.data4 = default(T4);
                m_cache.Enqueue(this);
            }
        }
    }

    public class Notification<T1, T2, T3, T4, T5> : Notification
    {
        new protected static Queue<Notification<T1, T2, T3, T4, T5>> m_cache = new Queue<Notification<T1, T2, T3, T4, T5>>();

        public T1 data1;
        public T2 data2;
        public T3 data3;
        public T4 data4;
        public T5 data5;

        public Notification(bool useCache) : base(useCache)
        {
        }

        public static Notification<T1, T2, T3, T4, T5> Create(string name, T1 data1, T2 data2, T3 data3, T4 data4, T5 data5, bool useCache)
        {
            Notification<T1, T2, T3, T4, T5> noti;
            lock (m_cache)
            {
                if (!useCache || m_cache.Count <= 0) noti = new Notification<T1, T2, T3, T4, T5>(useCache);
                else noti = m_cache.Dequeue();

                noti.name = name;
                noti.data1 = data1;
                noti.data2 = data2;
                noti.data3 = data3;
                noti.data4 = data4;
                noti.data5 = data5;
            }

            return noti;
        }
        public override void Release()
        {
            if (!this.m_useCache) return;

            lock (m_cache)
            {
                this.name = string.Empty;
                this.data1 = default(T1);
                this.data2 = default(T2);
                this.data3 = default(T3);
                this.data4 = default(T4);
                this.data5 = default(T5);
                m_cache.Enqueue(this);
            }
        }
    }

    public class Notification<T1, T2, T3, T4, T5, T6> : Notification
    {
        new protected static Queue<Notification<T1, T2, T3, T4, T5, T6>> m_cache = new Queue<Notification<T1, T2, T3, T4, T5, T6>>();

        public T1 data1;
        public T2 data2;
        public T3 data3;
        public T4 data4;
        public T5 data5;
        public T6 data6;

        public Notification(bool useCache) : base(useCache)
        {
        }

        public static Notification<T1, T2, T3, T4, T5, T6> Create(string name, T1 data1, T2 data2, T3 data3, T4 data4, T5 data5, T6 data6, bool useCache)
        {
            Notification<T1, T2, T3, T4, T5, T6> noti;
            lock (m_cache)
            {
                if (!useCache || m_cache.Count <= 0) noti = new Notification<T1, T2, T3, T4, T5, T6>(useCache);
                else noti = m_cache.Dequeue();

                noti.name = name;
                noti.data1 = data1;
                noti.data2 = data2;
                noti.data3 = data3;
                noti.data4 = data4;
                noti.data5 = data5;
                noti.data6 = data6;
            }

            return noti;
        }
        public override void Release()
        {
            if (!this.m_useCache) return;

            lock (m_cache)
            {
                this.name = string.Empty;
                this.data1 = default(T1);
                this.data2 = default(T2);
                this.data3 = default(T3);
                this.data4 = default(T4);
                this.data5 = default(T5);
                this.data6 = default(T6);
                m_cache.Enqueue(this);
            }
        }
    }

    public class Notification<T1, T2, T3, T4, T5, T6, T7> : Notification
    {
        new protected static Queue<Notification<T1, T2, T3, T4, T5, T6, T7>> m_cache = new Queue<Notification<T1, T2, T3, T4, T5, T6, T7>>();

        public T1 data1;
        public T2 data2;
        public T3 data3;
        public T4 data4;
        public T5 data5;
        public T6 data6;
        public T7 data7;

        public Notification(bool useCache) : base(useCache)
        {
        }

        public static Notification<T1, T2, T3, T4, T5, T6, T7> Create(string name, T1 data1, T2 data2, T3 data3, T4 data4, T5 data5, T6 data6, T7 data7, bool useCache)
        {
            Notification<T1, T2, T3, T4, T5, T6, T7> noti;
            lock (m_cache)
            {
                if (!useCache || m_cache.Count <= 0) noti = new Notification<T1, T2, T3, T4, T5, T6, T7>(useCache);
                else noti = m_cache.Dequeue();

                noti.name = name;
                noti.data1 = data1;
                noti.data2 = data2;
                noti.data3 = data3;
                noti.data4 = data4;
                noti.data5 = data5;
                noti.data6 = data6;
                noti.data7 = data7;
            }

            return noti;
        }
        public override void Release()
        {
            if (!this.m_useCache) return;

            lock (m_cache)
            {
                this.name = string.Empty;
                this.data1 = default(T1);
                this.data2 = default(T2);
                this.data3 = default(T3);
                this.data4 = default(T4);
                this.data5 = default(T5);
                this.data6 = default(T6);
                this.data7 = default(T7);
                m_cache.Enqueue(this);
            }
        }
    }

    public class Notification<T1, T2, T3, T4, T5, T6, T7, T8> : Notification
    {
        new protected static Queue<Notification<T1, T2, T3, T4, T5, T6, T7, T8>> m_cache = new Queue<Notification<T1, T2, T3, T4, T5, T6, T7, T8>>();

        public T1 data1;
        public T2 data2;
        public T3 data3;
        public T4 data4;
        public T5 data5;
        public T6 data6;
        public T7 data7;
        public T8 data8;

        public Notification(bool useCache) : base(useCache)
        {
        }

        public static Notification<T1, T2, T3, T4, T5, T6, T7, T8> Create(string name, T1 data1, T2 data2, T3 data3, T4 data4, T5 data5, T6 data6, T7 data7, T8 data8, bool useCache)
        {
            Notification<T1, T2, T3, T4, T5, T6, T7, T8> noti;
            lock (m_cache)
            {
                if (!useCache || m_cache.Count <= 0) noti = new Notification<T1, T2, T3, T4, T5, T6, T7, T8>(useCache);
                else noti = m_cache.Dequeue();

                noti.name = name;
                noti.data1 = data1;
                noti.data2 = data2;
                noti.data3 = data3;
                noti.data4 = data4;
                noti.data5 = data5;
                noti.data6 = data6;
                noti.data7 = data7;
                noti.data8 = data8;
            }

            return noti;
        }
        public override void Release()
        {
            if (!this.m_useCache) return;

            lock (m_cache)
            {
                this.name = string.Empty;
                this.data1 = default(T1);
                this.data2 = default(T2);
                this.data3 = default(T3);
                this.data4 = default(T4);
                this.data5 = default(T5);
                this.data6 = default(T6);
                this.data7 = default(T7);
                this.data8 = default(T8);
                m_cache.Enqueue(this);
            }
        }
    }
}
