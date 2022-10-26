using System.Collections.Generic;
using UnityEngine;
using Framework.FileSys;

namespace Framework.Asset
{
    public sealed class BundleRequest : AbstractAssetRequest<AssetBundle>, IBundleRequest
    {
        private string m_realPath;
        public string realPath { get { return m_realPath; } }
        private Dictionary<string, IBundleRequest> m_allDependencies = new Dictionary<string, IBundleRequest>();
        private List<IBundleRequest> m_asyncLoadingDependencies = new List<IBundleRequest>();
        private AsyncOperation m_asyncOperation;

        public BundleRequest(BundleFileInfo info)
        {
            m_path = info.path;
            m_realPath = info.realPath;
        }

        public override void Load(bool isAsync)
        {
            _Internal_LoadDependentRequest(isAsync);
        }

        public override void Unload()
        {
            foreach (IBundleRequest depend in m_allDependencies.Values)
            {
                AssetManager.Instance.Unload(depend);
            }
            m_allDependencies.Clear();
            m_asyncLoadingDependencies.Clear();
            if (asset != null) asset.Unload(true);
        }

        private void _Internal_LoadDependentRequest(bool isAsync)
        {
            //  获取依赖 AssetBundles
            List<BundleFileInfo> dependBundleInfos = FileSystem.Instance.GetBundleDependencies(m_path);
            if (dependBundleInfos != null && dependBundleInfos.Count > 0)
            {
                foreach (BundleFileInfo dependBundleInfo in dependBundleInfos)
                {
                    var dependRequest = _Internal_GetDependRequest(dependBundleInfo.path, isAsync);
                    if (dependRequest.hasError)
                    {
                        error = dependRequest.error;
                        _Internal_OnFinished();
                        return;
                    }
                    if (!dependRequest.isDone)
                    {
                        //只有首次且异步加载才会走到这里，覆盖同步加载走不到这里
                        m_asyncLoadingDependencies.Add(dependRequest);
                    }
                }

                if (m_asyncLoadingDependencies.Count > 0)
                {
                    //只有首次且异步加载才会走到这里，覆盖同步加载走不到这里
                    for (int i = 0; i < m_asyncLoadingDependencies.Count; i++)
                    {
                        var dependRequest = m_asyncLoadingDependencies[i];
                        dependRequest.onFinished += _Internal_OnDependFinished;
                    }
                }
                else //依赖都好了,直接加载
                {
                    _Internal_StartLoad(isAsync);
                }
            }
            else  //没有依赖,直接加载
            {
                _Internal_StartLoad(isAsync); 
            }
        }

        //缓存所有依赖bundleRequest只要是为了每个bundleRequst只对所有依赖bundleRequest计一次引用，方便UnloadAsset时释放计数
        private IBundleRequest _Internal_GetDependRequest(string dependPath, bool isAsync)
        {
            IBundleRequest dependRequest;
            if (!m_allDependencies.TryGetValue(dependPath, out dependRequest))
            {
                //不存在说明是第一次加载，按照参数选择同步或异步
                dependRequest = isAsync ? AssetManager.Instance.LoadBundleAsync(dependPath) : AssetManager.Instance.LoadBundle(dependPath);
                m_allDependencies.Add(dependPath, dependRequest);
            }
            else
            {
                //如果存在且未完成，应该是加载出错(不考虑处理)或者正在异步加载中，用同步加载覆盖
                if (!dependRequest.isDone && !isAsync)
                {
                    dependRequest.Load(false);
                }
            }
            return dependRequest;
        }

        private void _Internal_OnDependFinished(IRequest request)
        {
            //已经被同步加载覆盖了
            if (isDone) return;
            // 检查依赖bundle错误
            if (request.hasError) error += string.Format("<{0}>", request.error);
            // 检查所有依赖bundle
            foreach (var dependentRequest in m_asyncLoadingDependencies)
            {
                if (!dependentRequest.isDone) return;
            }

            m_asyncLoadingDependencies.Clear();
            if (hasError)  _Internal_OnFinished();
            //先检查一下主体bundle是不是已经被同步加载覆盖过了，如果没有的话这里肯定是异步模式加载
            else _Internal_StartLoad(true);
        }

        private void _Internal_StartLoad(bool isAsync)
        {
            if (isAsync) _Internal_AsyncLoad();
            else _Internal_SyncLoad();
        }

        private void _Internal_SyncLoad()
        {
            // var offset = FileConfigUtility.CalcFixedRubbishLength(m_AssetPath);
            m_asset = AssetBundle.LoadFromFile(m_realPath, 0, 32);
            if (m_asset == null)
            {
                error = string.Format("Error -> _Internal_SyncLoad when load bundle: {0}", m_realPath);
            }
            _Internal_OnFinished();
        }

        private void _Internal_AsyncLoad()
        {
            // var offset = FileConfigUtility.CalcFixedRubbishLength(m_AssetPath);
            m_asyncOperation = AssetBundle.LoadFromFileAsync(m_realPath, 0, 32);
            m_asyncOperation.completed += _Internal_OnAsyncFinished;
        }

        private void _Internal_OnAsyncFinished(AsyncOperation operation)
        {
            var request = operation as AssetBundleCreateRequest;
            m_asset = request.assetBundle;
            if (m_asset == null)
            {
                error = string.Format("Error -> _Internal_AsyncLoad when load bundle: {0}", m_realPath);
            }
            _Internal_OnFinished();
        }
    }
}
