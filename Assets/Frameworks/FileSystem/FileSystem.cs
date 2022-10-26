using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using Framework.Log;
using Framework.Utils;

namespace Framework.FileSys
{
    public partial class FileSystem : Singleton<FileSystem>
    {
        private Dictionary<string, BundleFileInfo> s_bundleInfoMap = new Dictionary<string, BundleFileInfo>();
        private Dictionary<string, AssetFileInfo> s_assetInfoMap = new Dictionary<string, AssetFileInfo>();

        public bool BundleExists(string bundlePath)
        {
            return s_bundleInfoMap.ContainsKey(bundlePath);
        }

        public bool AssetExists(string assetPath)
        {
            #if UNITY_EDITOR
            if (System.IO.File.Exists(assetPath)) return true;
            #endif
            return s_assetInfoMap.ContainsKey(assetPath);
        }

        public bool TryGetAssetFileInfo(string assetPath, out AssetFileInfo assetFileInfo)
        {
            return s_assetInfoMap.TryGetValue(assetPath, out assetFileInfo);
        }
        public bool TryGetBundleFileInfo(string bundlePath, out BundleFileInfo bundleFileInfo)
        {
            return s_bundleInfoMap.TryGetValue(bundlePath, out bundleFileInfo);
        }
        public List<BundleFileInfo> GetBundleDependencies(string bundlePath)
        {
            BundleFileInfo bundleFileInfo;
            if (!TryGetBundleFileInfo(bundlePath, out bundleFileInfo))
            {
                #if UNITY_EDITOR
                Logger.LogWarning("FileSystem->GetBundleDependencies Can't find the BundleInfo with : " + bundlePath);
                #else
                Logger.LogError("FileSystem->GetBundleDependencies Can't find the BundleInfo with : " + bundlePath);
                #endif
                return null;
            }
            return bundleFileInfo.dependencies;
        }
    }
}
