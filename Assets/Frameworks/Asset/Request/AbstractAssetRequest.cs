using System;
using System.Collections;
using UnityEngine;
using Object = UnityEngine.Object;
using Framework.Noti;

namespace Framework.Asset
{
    public abstract class AbstractAssetRequest<T> : IAssetRequest<T> where T : Object
    {
        protected string m_path;
        protected T m_asset;
        protected string m_error;
        protected int m_referenceCount;
        protected float m_progress;
        protected bool m_isDone;

        public string path { get { return m_path; } }
        public T asset { get { return m_asset; } }
        Object IRequest.asset { get { return m_asset; } }
        public bool isDone { get { return m_isDone; } }
        public bool hasError { get { return !string.IsNullOrEmpty(error); } }
        public virtual string error
        {
            get { return m_error; }
            protected set { m_error = value; }
        }
        public event Action<IRequest> onFinished;

        public abstract void Load(bool isAsync);
        public abstract void Unload();

        protected void _Internal_OnFinished()
        {
            m_progress = 1;
            m_isDone = true;
            if (onFinished != null)
            {
                onFinished(this);
                onFinished = null;
            }
        }

        #region Reference接口实现
        int IReference.referenceCount
        {
            get { return m_referenceCount; }
        }

        int IReference.Reference()
        {
            return ++m_referenceCount;
        }

        int IReference.UnReference()
        {
            return --m_referenceCount;
        }
        #endregion

        #region IEnumerator接口实现
        object IEnumerator.Current { get { return null; } }

        bool IEnumerator.MoveNext()
        {
            return !m_isDone;
        }

        void IEnumerator.Reset() { }
        #endregion

        void IRequest.Dispose()
        {
            onFinished = null;
            m_isDone = false;
            m_error = "Disposed";
            m_progress = 0;
            m_referenceCount = 0;
            m_asset = null;
        }
    }
}