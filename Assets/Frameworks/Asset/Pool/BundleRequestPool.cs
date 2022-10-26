using Framework.FileSys;
using System.Collections.Generic;

namespace Framework.Asset
{
    public class BundleRequestPool : AbstractRequestPool<IBundleRequest>, IBundleRequestPool
    {
        public IBundleRequest GetRequest(BundleFileInfo info, bool isAsync)
        {
            IBundleRequest request = FindRequest(info.path);
            if (request == null) request = new BundleRequest(info);
            request.Reference();
            LoadRequest(request, isAsync);
            return request;
        }
    }
}