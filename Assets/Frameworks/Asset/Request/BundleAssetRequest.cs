using Framework.FileSys;
using UnityEngine;

namespace Framework.Asset
{
    public sealed class BundleAssetRequest<T> : AbstractAssetRequest<T>, IBundleAssetRequest where T:UnityEngine.Object
    {
        private IBundleRequest m_bundleRequest;
        private AsyncOperation m_asyncOperation;

        private string m_bundlePath;

        public BundleAssetRequest(AssetFileInfo info)
        {
            m_path = info.path;
            m_bundlePath = info.belongBundle.path;
        }

        public override void Load(bool isAsync)
        {
            _Internal_StartLoadBundle(isAsync);
        }

        public override void Unload()
        {
            if (!(asset is GameObject) && !(asset is Component))
            {
                Resources.UnloadAsset(asset);
            }
            if (m_bundleRequest != null)
            {
                AssetManager.Instance.Unload(m_bundleRequest);
                m_bundleRequest = null;
            }
        }

        private void _Internal_StartLoadBundle(bool isAsync)
        {
            m_bundleRequest = _Internal_GetDependRequest(m_bundlePath, isAsync);

            if (m_bundleRequest.hasError)
            {
                error = m_bundleRequest.error;
                _Internal_OnFinished();
                return;
            }

            if (m_bundleRequest.isDone)
            {
                _Internal_StartLoadAsset(isAsync);
            }
            else
            {
                //只有首次且异步加载才会走到这里，覆盖同步加载走不到这里
                m_bundleRequest.onFinished += _Internal_OnBundleFinished;
            }
        }

        //缓存依赖bundleRequest只要是为了每个assetRequst只对所有依赖bundleRequest计一次引用，方便UnloadAsset时释放计数
        private IBundleRequest _Internal_GetDependRequest(string dependPath, bool isAsync)
        {
            if (m_bundleRequest == null)
            {
                //不存在说明是第一次加载，按照参数选择同步或异步
                m_bundleRequest = isAsync ? AssetManager.Instance.LoadBundleAsync(dependPath) : AssetManager.Instance.LoadBundle(dependPath);
            }
            else
            {
                //如果存在且未完成，应该是加载出错(不考虑处理)或者正在异步加载中，用同步加载覆盖
                if (!m_bundleRequest.isDone && !isAsync)
                {
                    m_bundleRequest.Load(false);
                }
            }
            return m_bundleRequest;
        }

        private void _Internal_OnBundleFinished(IRequest request)
        {
            //已经被同步加载覆盖了
            if (isDone) return;
            if (request.hasError)
            {
                error = m_bundleRequest.error;
                _Internal_OnFinished();
            }
            else _Internal_StartLoadAsset(true);
        }

        private void _Internal_StartLoadAsset(bool isAsync)
        {
            if (isAsync) _Internal_AsyncLoadAsset(m_bundleRequest.asset);
            else _Internal_SyncLoadAsset(m_bundleRequest.asset);
        }

        private void _Internal_SyncLoadAsset(AssetBundle bundle)
        {
            m_asset = bundle.LoadAsset<T>(m_path);
            if (m_asset == null)
            {
                error = string.Format("Error -> _Internal_SyncLoadAsset when load {1}:{0}", m_path, m_bundleRequest.path);
            }
            _Internal_OnFinished();
        }

        private void _Internal_AsyncLoadAsset(AssetBundle bundle)
        {
            m_asyncOperation = bundle.LoadAssetAsync<T>(m_path);
            m_asyncOperation.completed += _Internal_OnCompleted;
        }

        private void _Internal_OnCompleted(AsyncOperation operation)
        {
            var request = operation as AssetBundleRequest;
            m_asset = request.asset as T;
            if (m_asset == null)
            {
                error = string.Format("Error -> _Internal_AsyncLoadAsset when load {1}:{0}", m_path, m_bundleRequest.path);
            }
            _Internal_OnFinished();
        }
    }
}
