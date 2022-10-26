using Framework.FileSys;
using System;

namespace Framework.Asset
{
    public interface IRequestPool<T> where T : IRequest
    {
        bool IsLoading(string path);
        bool IsLoaded(string path);
        T FindRequest(string path);
        void LoadRequest(T request, bool isAsync);
        void FreeRequest(T request);
    }

    public interface IEditorAssetRequestPool : IRequestPool<IEditorAssetRequest>, IDisposable
    {
        IEditorAssetRequest GetRequest<T>(AssetFileInfo info) where T : UnityEngine.Object;
    }

    public interface IBundleRequestPool :IRequestPool<IBundleRequest>, IDisposable
    {
        IBundleRequest GetRequest(BundleFileInfo info, bool isAsync);
    }

    public interface IBundleAssetRequestPool : IRequestPool<IBundleAssetRequest>, IDisposable
    {
        IBundleAssetRequest GetRequest<T>(AssetFileInfo info, bool isAsync) where T : UnityEngine.Object;
    }

}