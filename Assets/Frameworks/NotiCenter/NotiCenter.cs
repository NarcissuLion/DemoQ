using System;
using System.Collections.Generic;

namespace Framework.Noti
{
    public partial class NotiCenter
    {
        private static Dictionary<object, List<Action<INotification>>> m_static_notiDict = new Dictionary<object, List<Action<INotification>>>();

        private Dictionary<object, List<Action<INotification>>> m_notiDict = new Dictionary<object, List<Action<INotification>>>();

        public void AddListener(string notiName, Action<INotification> action)
        {
            List<Action<INotification>> actions;
            if (!m_notiDict.TryGetValue(notiName, out actions))
            {
                actions = new List<Action<INotification>>();
                m_notiDict.Add(notiName, actions);
            }
            if (!actions.Contains(action))
            {
                actions.Insert(0, action);
            }
        }
        public static void AddStaticListener(string notiName, Action<INotification> action)
        {
            List<Action<INotification>> actions;
            if (!m_static_notiDict.TryGetValue(notiName, out actions))
            {
                actions = new List<Action<INotification>>();
                m_static_notiDict.Add(notiName, actions);
            }
            if (!actions.Contains(action))
            {
                actions.Insert(0, action);
            }
        }

        public void RemoveListener(object notiName, Action<INotification> action)
        {
            List<Action<INotification>> actions;
            if (!m_notiDict.TryGetValue(notiName, out actions))
            {
                return;
            }
            if (actions.Contains(action))
            {
                actions.Remove(action);
            }
        }
        public static void RemoveStaticListener(object notiName, Action<INotification> action)
        {
            List<Action<INotification>> actions;
            if (!m_static_notiDict.TryGetValue(notiName, out actions))
            {
                return;
            }
            if (actions.Contains(action))
            {
                actions.Remove(action);
            }
        }

        public void Dispatch(INotification noti)
        {
            List<Action<INotification>> actions;
            if (m_notiDict.TryGetValue(noti.name, out actions))
            {
                for (int i = actions.Count - 1; i >= 0; i--)
                {
                    actions[i](noti);
                }
            }

            noti.Release();
        }
        public static void DispatchStatic(INotification noti)
        {
            List<Action<INotification>> actions;
            if (m_static_notiDict.TryGetValue(noti.name, out actions))
            {
                for (int i = actions.Count - 1; i >= 0; i--)
                {
                    actions[i](noti);
                }
            }

            noti.Release();
        }

        public void Dispatch(string notiName, bool useCache = true)
        {
            Dispatch(Notification.Create(notiName, useCache));
        }
        public static void DispatchStatic(string notiName, bool useCache = true)
        {
            DispatchStatic(Notification.Create(notiName, useCache));
        }

        public void Dispatch<T>(string notiName, T notiData, bool useCache = true)
        {
            Dispatch(Notification<T>.Create(notiName, notiData, useCache));
        }
        public static void DispatchStatic<T>(string notiName, T notiData, bool useCache = true)
        {
            DispatchStatic(Notification<T>.Create(notiName, notiData, useCache));
        }

        public void Dispatch<T1, T2>(string notiName, T1 notiData1, T2 notiData2, bool useCache = true)
        {
            Dispatch(Notification<T1, T2>.Create(notiName, notiData1, notiData2, useCache));
        }
        public static void DispatchStatic<T1, T2>(string notiName, T1 notiData1, T2 notiData2, bool useCache = true)
        {
            DispatchStatic(Notification<T1, T2>.Create(notiName, notiData1, notiData2, useCache));
        }

        public void Dispatch<T1, T2, T3>(string notiName, T1 notiData1, T2 notiData2, T3 notiData3, bool useCache = true)
        {
            Dispatch(Notification<T1, T2, T3>.Create(notiName, notiData1, notiData2, notiData3, useCache));
        }
        public static void DispatchStatic<T1, T2, T3>(string notiName, T1 notiData1, T2 notiData2, T3 notiData3, bool useCache = true)
        {
            DispatchStatic(Notification<T1, T2, T3>.Create(notiName, notiData1, notiData2, notiData3, useCache));
        }

        public void Dispatch<T1, T2, T3, T4>(string notiName, T1 notiData1, T2 notiData2, T3 notiData3, T4 notiData4, bool useCache = true)
        {
            Dispatch(Notification<T1, T2, T3, T4>.Create(notiName, notiData1, notiData2, notiData3, notiData4, useCache));
        }
        public static void DispatchStatic<T1, T2, T3, T4>(string notiName, T1 notiData1, T2 notiData2, T3 notiData3, T4 notiData4, bool useCache = true)
        {
            DispatchStatic(Notification<T1, T2, T3, T4>.Create(notiName, notiData1, notiData2, notiData3, notiData4, useCache));
        }

        public void Dispatch<T1, T2, T3, T4, T5>(string notiName, T1 notiData1, T2 notiData2, T3 notiData3, T4 notiData4, T5 notiData5, bool useCache = true)
        {
            Dispatch(Notification<T1, T2, T3, T4, T5>.Create(notiName, notiData1, notiData2, notiData3, notiData4, notiData5, useCache));
        }
        public static void DispatchStatic<T1, T2, T3, T4, T5>(string notiName, T1 notiData1, T2 notiData2, T3 notiData3, T4 notiData4, T5 notiData5, bool useCache = true)
        {
            DispatchStatic(Notification<T1, T2, T3, T4, T5>.Create(notiName, notiData1, notiData2, notiData3, notiData4, notiData5, useCache));
        }

        public void Dispatch<T1, T2, T3, T4, T5, T6>(string notiName, T1 notiData1, T2 notiData2, T3 notiData3, T4 notiData4, T5 notiData5, T6 notiData6, bool useCache = true)
        {
            Dispatch(Notification<T1, T2, T3, T4, T5, T6>.Create(notiName, notiData1, notiData2, notiData3, notiData4, notiData5, notiData6, useCache));
        }
        public static void DispatchStatic<T1, T2, T3, T4, T5, T6>(string notiName, T1 notiData1, T2 notiData2, T3 notiData3, T4 notiData4, T5 notiData5, T6 notiData6, bool useCache = true)
        {
            DispatchStatic(Notification<T1, T2, T3, T4, T5, T6>.Create(notiName, notiData1, notiData2, notiData3, notiData4, notiData5, notiData6, useCache));
        }

        public void Dispatch<T1, T2, T3, T4, T5, T6, T7>(string notiName, T1 notiData1, T2 notiData2, T3 notiData3, T4 notiData4, T5 notiData5, T6 notiData6, T7 notiData7, bool useCache = true)
        {
            Dispatch(Notification<T1, T2, T3, T4, T5, T6, T7>.Create(notiName, notiData1, notiData2, notiData3, notiData4, notiData5, notiData6, notiData7, useCache));
        }
        public static void DispatchStatic<T1, T2, T3, T4, T5, T6, T7>(string notiName, T1 notiData1, T2 notiData2, T3 notiData3, T4 notiData4, T5 notiData5, T6 notiData6, T7 notiData7, bool useCache = true)
        {
            DispatchStatic(Notification<T1, T2, T3, T4, T5, T6, T7>.Create(notiName, notiData1, notiData2, notiData3, notiData4, notiData5, notiData6, notiData7, useCache));
        }
    }
}
