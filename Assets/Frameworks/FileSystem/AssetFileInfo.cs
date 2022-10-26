using System.Collections.Generic;

namespace Framework.FileSys
{
    public class AssetFileInfo
    {
        public string path;                         //外部调用路径
        public string realPath;                     //独立文件的真实路径，bundle内部文件为null
        public BundleFileInfo belongBundle;         //bundle内部文件所属的bundle，独立文件为null
        public string md5;                          //bundle内部文件为null
        public long size;                           //bundle内部文件为0且无意义
    }

    public class BundleFileInfo : AssetFileInfo
    {
        public List<BundleFileInfo> dependencies;
    }

    public class DLCFilesInfo
    {
        public string dlcName;                                              //dlc名称
        public List<AssetFileInfo> dlcFiles = new List<AssetFileInfo>();    //dlc内全部资源信息
        public List<bool> dlcFilesReady = new List<bool>();                 //对应dlcFiles，标记每个资源是否已经就为

        public bool allReady
        {
            get
            {
                foreach (bool fileReady in dlcFilesReady)
                {
                    if (!fileReady) return false;
                }
                return true;
            }
        }

        private long _totalSize;
        public long totalSize
        {
            get
            {
                if (_totalSize > 0) return _totalSize;
                foreach (AssetFileInfo fileInfo in dlcFiles)
                {
                    _totalSize += fileInfo.size;
                }
                return _totalSize;
            }   
        }

        public long readySize
        {
            get
            {
                long sum = 0;
                for (int i = 0; i < dlcFiles.Count; ++i)
                {
                    if (!dlcFilesReady[i]) continue;
                    sum += dlcFiles[i].size;
                }
                return sum;
            }
        }
    }
}