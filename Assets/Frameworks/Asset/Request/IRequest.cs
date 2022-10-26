using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Asset
{
    public interface IReference
    {
        int referenceCount { get; }
        int Reference();
        int UnReference();
    }

    public interface IRequest : IEnumerator, IReference
    {
        Object asset { get; }
        string path { get; }
        bool isDone { get; }
        bool hasError { get; }
        string error { get; }

        void Load(bool isAsync);
        void Unload();

        event System.Action<IRequest> onFinished;

        void Dispose();
    }

    public interface IAssetRequest<T> : IRequest where T : Object
    {
        new T asset { get; }
    }

    public interface IEditorAssetRequest : IRequest {}
    public interface IBundleRequest : IAssetRequest<AssetBundle> {}
    public interface IBundleAssetRequest : IRequest {}
}
