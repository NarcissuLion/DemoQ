using Framework.FileSys;
using UnityEngine;

namespace Framework.Asset
{
    public class BundleAssetRequestPool : AbstractRequestPool<IBundleAssetRequest>, IBundleAssetRequestPool
    {
        public IBundleAssetRequest GetRequest<T>(AssetFileInfo info, bool isAsync) where T : UnityEngine.Object
        {
            IBundleAssetRequest request = FindRequest(info.path);
            if (request == null) request = new BundleAssetRequest<T>(info);
            request.Reference();
            LoadRequest(request, isAsync);
            return request;
        }
    }
}