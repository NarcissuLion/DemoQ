using Framework.Noti;
using Framework.FileSys;
using UnityEngine;

namespace Framework.Asset
{
    public interface IRequestFactory
    {
        void Dispose();

        IAssetRequest<T> GetBundleAssetRequest<T>(string assetPath, bool isAsync) where T : UnityEngine.Object;
        IBundleRequest GetBundleRequest(string bundlePath, bool isAsync);
        void FreeRequest(IRequest request);
    }
}

