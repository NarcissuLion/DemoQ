using Framework.FileSys;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#endif

namespace Framework.Asset
{
    public sealed class EditorAssetRequest<T> : AbstractAssetRequest<T>, IEditorAssetRequest where T : UnityEngine.Object
    {
        public EditorAssetRequest(AssetFileInfo info)
        {
            m_path = info.path;
        }

        public override void Load(bool isAsync)
        {
            _Internal_StartLoad();
        }

        public override void Unload()
        {
#if UNITY_EDITOR
            if (!(asset is GameObject) && !(asset is Component) && !(asset is Texture) && !(asset is Sprite))
            {
                Resources.UnloadAsset(asset);
            }
#endif
        }

        private void _Internal_StartLoad()
        {
#if !UNITY_EDITOR
            error = "Not in Editor. Can't load EditorAsset !!!";
            m_progress = 1;
            m_isDone = true;
            return;
#endif
            _Internal_SynsLoad();
        }

        private void _Internal_SynsLoad()
        {
#if UNITY_EDITOR
            m_asset = AssetDatabase.LoadAssetAtPath(m_path, typeof(T)) as T;
            if (m_asset == null)
            {
                error = string.Format("Fail load resources at {0} !!!", m_path);
                Debug.LogError(error);
            }
            _Internal_OnFinished();
#endif
        }
    }
}
