using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Framework.Asset
{
    public abstract class AbstractRequestPool<T> where T : IRequest
    {
        private Dictionary<string, T> m_loadedDict = new Dictionary<string, T>();
        private Dictionary<string, T> m_loadingDict = new Dictionary<string, T>();

        public bool IsLoading(string path)
        {
            return m_loadedDict.ContainsKey(path);
        }

        public bool IsLoaded(string path)
        {
            return m_loadedDict.ContainsKey(path);
        }

        public T FindRequest(string path)
        {
            if (m_loadedDict.ContainsKey(path)) return m_loadedDict[path];
            else if (m_loadingDict.ContainsKey(path)) return m_loadingDict[path];
            else return default(T);
        }

        public void LoadRequest(T request, bool isAsync)
        {
            //已经加载完毕
            if (request.isDone) return;
            //正在异步加载中
            if (m_loadingDict.ContainsKey(request.path))
            {
                //重复申请异步加载，直接忽略
                if (isAsync) return;
                //同步加载覆盖异步加载
                else request.Load(false);
            }
            else
            {
                //这里不管同步异步都先添加到loading列表中，不过同步加载会在当前帧立刻执行完毕并从loading列表移到loaded中
                m_loadingDict.Add(request.path, request);
                request.onFinished += RequestLoaded;
                request.Load(isAsync);
            }
        }

        protected void RequestLoaded(IRequest request)
        {
            if (m_loadingDict.ContainsKey(request.path)) m_loadingDict.Remove(request.path);
            if (!m_loadedDict.ContainsKey(request.path)) m_loadedDict.Add(request.path, (T)request);
        }

        public void FreeRequest(T request)
        {
            if (request == null) return;
            if (request.UnReference() <= 0)
            {
                if (m_loadingDict.ContainsKey(request.path)) m_loadingDict.Remove(request.path);
                if (m_loadedDict.ContainsKey(request.path)) m_loadedDict.Remove(request.path);
                UnloadRequest(request);
                request.Dispose();
            }
        }

        protected virtual void UnloadRequest(T request)
        {
            request.Unload();
        }

        public virtual void Dispose()
        {
            //理论上应该遍历并Unload所有Request，但实际中似乎不会在游戏过程中DisposePool，所以先这样吧
            m_loadedDict.Clear();
            m_loadingDict.Clear();
        }
    }
}
