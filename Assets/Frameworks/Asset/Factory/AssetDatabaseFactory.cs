using Framework.Noti;
using Framework.FileSys;
using UnityEngine;

namespace Framework.Asset
{
    public class AssetDatabaseFactory : IRequestFactory
    {
        private IEditorAssetRequestPool m_pool = new EditorAssetRequestPool();

        public void Dispose()
        {
            m_pool.Dispose();
        }

        public IAssetRequest<T> GetBundleAssetRequest<T>(string assetPath, bool isAsync) where T : UnityEngine.Object
        {
            AssetFileInfo info = new AssetFileInfo();
            info.path = assetPath;
            return m_pool.GetRequest<T>(info) as IAssetRequest<T>;
        }

        public IBundleRequest GetBundleRequest(string bundlePath, bool isAsync)
        {
            return null;
        }

        public void FreeRequest(IRequest request)
        {
            m_pool.FreeRequest(request as IEditorAssetRequest);
        }
    }
}

