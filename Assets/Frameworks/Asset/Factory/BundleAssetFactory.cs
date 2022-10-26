using Framework.Noti;
using Framework.FileSys;
using UnityEngine;

namespace Framework.Asset
{
    public class BundleAssetFactory : IRequestFactory
    {
        private IBundleRequestPool s_bundleRequestPool = new BundleRequestPool();
        private IBundleAssetRequestPool s_bundleAssetRequestPool = new BundleAssetRequestPool();

        public void Dispose()
        {
            s_bundleRequestPool.Dispose();
            s_bundleAssetRequestPool.Dispose();
        }

        public IAssetRequest<T> GetBundleAssetRequest<T>(string assetPath, bool isAsync) where T : UnityEngine.Object
        {
            AssetFileInfo info;
            if (!FileSystem.Instance.TryGetAssetFileInfo(assetPath, out info))
            {
                Debug.LogError("AssetRequestFactory->GetAssetRequest Can't find AssetInfo with : " + assetPath);
                return null;
            }
            if (info.belongBundle != null) return s_bundleAssetRequestPool.GetRequest<T>(info, isAsync) as IAssetRequest<T>;
            else Debug.LogError("AssetRequestFactory->GetAssetRequest Can't find the belong BundleInfo for : " + assetPath);
            return null;
        }

        public IBundleRequest GetBundleRequest(string bundlePath, bool isAsync)
        {
            BundleFileInfo info;
            if (!FileSystem.Instance.TryGetBundleFileInfo(bundlePath, out info))
            {
                Debug.LogError("AssetRequestFactory->GetBundleRequest Can't find BundleInfo with : " + bundlePath);
                return null;
            }
            return s_bundleRequestPool.GetRequest(info, isAsync);
        }

        public void FreeRequest(IRequest request)
        {
            if (request is IBundleAssetRequest) s_bundleAssetRequestPool.FreeRequest(request as IBundleAssetRequest);
            else s_bundleRequestPool.FreeRequest(request as IBundleRequest);
        }
    }
}

