using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace Framework.FileSys
{
    public interface IFileSystem        
    {
        IEnumerator PreUpdate(Action<string, string> errorCallback);
        IEnumerator DoUpdate(Action<string, string, float, float, int, int, long, long> progressCallback, Action<string, string> errorCallback);
        int GetNeedDownloadFileCount();
        long GetNeedDownloadBytes();

        bool BundleExists(string bundlePath);

        bool AssetExists(string assetPath);

        bool TryGetAssetFileInfo(string assetPath, out AssetFileInfo assetFileInfo);

        bool TryGetBundleFileInfo(string bundlePath, out BundleFileInfo bundleFileInfo);
        List<BundleFileInfo> GetBundleDependencies(string bundlePath);

        bool IsDLCReady(string dlcName);
        void DownloadDLC(string dlcName);
        bool MarkDLC(string dlcName);
        void SetCarrierDataNetworkPremitted(bool isPremitted);
    }
}