using Framework.FileSys;
using UnityEngine;

namespace Framework.Asset
{
    public class EditorAssetRequestPool : AbstractRequestPool<IEditorAssetRequest>, IEditorAssetRequestPool
    {
        public IEditorAssetRequest GetRequest<T>(AssetFileInfo info) where T : UnityEngine.Object
        {
            IEditorAssetRequest request = FindRequest(info.path);
            if (request == null) request = new EditorAssetRequest<T>(info);
            request.Reference();
            LoadRequest(request, false);
            return request;
        }
    }
}