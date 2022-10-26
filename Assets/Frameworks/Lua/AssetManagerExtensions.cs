using Framework.Asset;
using UnityEngine;

namespace Framework.Lua
{
    public static class AssetManagerExtensions
    {
        public static IAssetRequest<GameObject> LoadPrefab(this AssetManager assetManager, string assetPath)
        {
            return assetManager.LoadAsset<GameObject>(assetPath);
        }

        public static IAssetRequest<GameObject> LoadPrefabAsync(this AssetManager assetManager, string assetPath)
        {
            return assetManager.LoadAssetAsync<GameObject>(assetPath);
        }
    }
}
