using Framework.Utils;
using System;
using Object = UnityEngine.Object;

namespace Framework.Asset
{
    public class AssetManager : Singleton<AssetManager>
    {
        public static bool ASSETDATABASE_MODE = true;       //开发期不使用bundle，资源直接从本地AssetDatabase加载，注意上层LoadBundle的地方要特殊处理

        public IRequestFactory m_factory;
        public AssetManager()
        {
#if UNITY_EDITOR
            if (ASSETDATABASE_MODE) m_factory = new AssetDatabaseFactory();
            else m_factory = new BundleAssetFactory();
#else
            m_factory = new BundleAssetFactory();
#endif
        }

        public override void Dispose()
        {
            m_factory.Dispose();
            base.Dispose();
        }

        /**
        *   同步加载游戏资源，注意与UnloadAsset配对使用，防止内存泄露
        */
        public IAssetRequest<T> LoadAsset<T>(string assetPath) where T : Object
        {
            IAssetRequest<T> request = m_factory.GetBundleAssetRequest<T>(assetPath, false);
            return request;
        }

        /**
        *   异步加载游戏资源，注意与UnloadAsset配对使用，防止内存泄露
        */
        public IAssetRequest<T> LoadAssetAsync<T>(string assetPath, Action<IRequest> callback = null) where T : Object
        {
            IAssetRequest<T> request = m_factory.GetBundleAssetRequest<T>(assetPath, true);
            if (request.isDone)
            {
                if (callback != null) callback(request);
            }
            else
            {
                if (callback != null) request.onFinished += callback;
            }
            return request;
        }

        /**
        *   同步加载资源bundle，注意与UnloadBundle配对使用，防止内存泄露
        */
        public IBundleRequest LoadBundle(string bundlePath)
        {
            IBundleRequest request = m_factory.GetBundleRequest(bundlePath, false);
            return request;
        }

        /**
        *   异步加载资源bundle，注意与UnloadBundle配对使用，防止内存泄露
        */
        public IBundleRequest LoadBundleAsync(string bundlePath, Action<IRequest> callback = null)
        {
            IBundleRequest request = m_factory.GetBundleRequest(bundlePath, true);
            if (request == null) return null;
            if (request.isDone)
            {
                if (callback != null) callback(request);
            }
            else
            {
                if (callback != null) request.onFinished += callback;
            }
            return request;
        }

        /**
        *   释放游戏资源
        */
        public void Unload(IRequest request)
        {
            m_factory.FreeRequest(request);
        }
    }
}
