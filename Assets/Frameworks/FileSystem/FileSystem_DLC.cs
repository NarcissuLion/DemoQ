using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using Framework.Noti;
using System.Threading;
using System.Net;
using Born2Code.Net;
using Framework.Utils;

namespace Framework.FileSys
{
    public partial class FileSystem : Singleton<FileSystem>
    {
        public static string NOTI_UPDATE_PROGRESS = "NOTI_UPDATE_PROGRESS";
        public static string NOTI_DLC_DOWNLOAD_STARTED = "NOTI_DLC_DOWNLOAD_STARTED";
        public static string NOTI_DLC_DOWNLOAD_COMPLETE = "NOTI_DLC_DOWNLOAD_COMPLETE";
        public static string NOTI_DLC_DOWNLOAD_PROGRESS = "NOTI_DLC_DOWNLOAD_PROGRESS";
        public static string NOTI_DLC_DOWNLOAD_FAILED = "NOTI_DLC_DOWNLOAD_FAILED";
        public static string NOTI_DLC_DOWNLOAD_INTERRUPTED = "NOTI_DLC_DOWNLOAD_INTERRUPTED";

        private Dictionary<string, DLCFilesInfo> s_dlcInfoMap = new Dictionary<string, DLCFilesInfo>();

        private DLCDownloaderManager m_dlc_downloader_mgr;

        private void PrepareDLCDownloaderManager()
        {
            if (m_dlc_downloader_mgr == null)
            {
                m_dlc_downloader_mgr = new GameObject("dlc_downloader").AddComponent<DLCDownloaderManager>();
                GameObject.DontDestroyOnLoad(m_dlc_downloader_mgr.gameObject);

                NotiCenter.AddStaticListener(NOTI_DLC_DOWNLOAD_COMPLETE, (INotification noti)=>
                {
                    MarkDLC((noti as Notification<string>).data);
                });
            }
        }

        public DLCFilesInfo GetDLCInfo(string dlcName)
        {
            DLCFilesInfo dlcInfo;
            s_dlcInfoMap.TryGetValue(dlcName, out dlcInfo);
            return dlcInfo;
        }

        public bool IsDLCReady(string dlcName)
        {
            DLCFilesInfo dlcInfo;
            if (!s_dlcInfoMap.TryGetValue(dlcName, out dlcInfo))
            {
#if !UNITY_EDITOR
                Framework.Log.Logger.LogWarning("DLC Info Not Found: " + dlcName);
#endif
                return true;
            }

            if (dlcInfo.allReady && !m_dlcMarks.Contains(dlcName))
            {
                MarkDLC(dlcName);
            }
            return dlcInfo.allReady;
        }

        public void DownloadDLC(string dlcName)
        {
            DLCFilesInfo dlcInfo;
            if (!s_dlcInfoMap.TryGetValue(dlcName, out dlcInfo))
            {
                Framework.Log.Logger.LogError("DLC Info Not Found: " + dlcName);
                return;
            }
            if (dlcInfo.allReady)
            {
                NotiCenter.DispatchStatic<string>(NOTI_DLC_DOWNLOAD_COMPLETE, dlcName);
                return; 
            }

            PrepareDLCDownloaderManager();
            m_dlc_downloader_mgr.PushTask(dlcInfo);
        }

        public bool MarkDLC(string dlcName)
        {
            if (m_dlcMarks == null || m_dlcMarks.Contains(dlcName)) return false;
            m_dlcMarks.Add(dlcName);
            File.WriteAllText(LOCAL_DLC_MARKS, string.Join(",", m_dlcMarks));
            return true;
        }

        public void SetCarrierDataNetworkPremitted(bool isPremitted)
        {
            PrepareDLCDownloaderManager();
            m_dlc_downloader_mgr.carrierDataNetworkPremitted = isPremitted;
        }
    }

    public class DLCDownloaderManager : MonoBehaviour
    {
        private static int RETRY_TIMES = 3;

        public bool carrierDataNetworkPremitted = true; //默認許可
        
        private Queue<DLCDownloadTask> m_waitingTasks = new Queue<DLCDownloadTask>();
        private DLCDownloadTask m_activeTask;
        private Thread m_worker;
        private bool m_network_ok;

        public class DLCDownloadTask
        {
            public enum STATE
            {
                WAITIING = 0,
                DOWNLOADING,
                SUCCESSED,
                FAILED,
                INTERRUPTED,
            }

            public DLCFilesInfo dlcInfo;
            public int currIdx;
            public float totalProgress;
            public float currProgress;
            public bool progressDirty;
            public volatile STATE state;
            public string error;

            public DLCDownloadTask(DLCFilesInfo dlcInfo)
            {
                this.dlcInfo = dlcInfo;
                this.currIdx = 0;
                this.totalProgress = 0f;
                this.currProgress = 0f;
                this.progressDirty = false;
                this.state = STATE.WAITIING;
                this.error = string.Empty;
            }
        }

        private void OnLoadedBytesProgress(long loadedBytes)
        {
            if (m_activeTask != null)
            {
                m_activeTask.totalProgress = (m_activeTask.dlcInfo.readySize + loadedBytes) * 1f / m_activeTask.dlcInfo.totalSize;
                m_activeTask.currProgress = loadedBytes * 1f / m_activeTask.dlcInfo.dlcFiles[m_activeTask.currIdx].size;
                m_activeTask.progressDirty = true;
            }
        }

        private void Update()
        {
            if (m_activeTask != null)
            {
                if (m_activeTask.state == DLCDownloadTask.STATE.SUCCESSED)
                {
                    NotiCenter.DispatchStatic<string>(FileSystem.NOTI_DLC_DOWNLOAD_COMPLETE, m_activeTask.dlcInfo.dlcName);
                    m_activeTask = null;
                }
                else if (m_activeTask.state == DLCDownloadTask.STATE.FAILED)
                {
                    NotiCenter.DispatchStatic<string, string>(FileSystem.NOTI_DLC_DOWNLOAD_FAILED, m_activeTask.dlcInfo.dlcName, m_activeTask.error);
                    m_activeTask = null;
                }
                else if (m_activeTask.state == DLCDownloadTask.STATE.INTERRUPTED)
                {
                    NotiCenter.DispatchStatic<string>(FileSystem.NOTI_DLC_DOWNLOAD_INTERRUPTED, m_activeTask.dlcInfo.dlcName);
                    m_activeTask = null;
                }
                else if (m_activeTask.state == DLCDownloadTask.STATE.DOWNLOADING)
                {
                    if (m_activeTask.progressDirty)
                    {
                        m_activeTask.progressDirty = false;
                        NotiCenter.DispatchStatic<string, float, string, float, int, int>(FileSystem.NOTI_DLC_DOWNLOAD_PROGRESS, m_activeTask.dlcInfo.dlcName, m_activeTask.totalProgress, m_activeTask.dlcInfo.dlcFiles[m_activeTask.currIdx].path, m_activeTask.currProgress, m_activeTask.dlcInfo.dlcFiles.Count, m_activeTask.currIdx+1);
                    }
                }
            }

            if (Application.internetReachability == NetworkReachability.NotReachable || 
                !carrierDataNetworkPremitted && Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork)
            {
                m_network_ok = false;
            }
            else
            {
                m_network_ok = true;
            }

            if (m_network_ok)
            {
                if (m_activeTask == null && m_waitingTasks.Count > 0)
                {
                    m_activeTask = m_waitingTasks.Dequeue();
                    m_activeTask.state = DLCDownloadTask.STATE.DOWNLOADING;
                    NotiCenter.DispatchStatic<string>(FileSystem.NOTI_DLC_DOWNLOAD_STARTED, m_activeTask.dlcInfo.dlcName);
                    m_worker = new Thread(() => WorkProc(m_activeTask));
                    m_worker.Start();
                }
            }
            else
            {
                if (m_activeTask != null && m_activeTask.state == DLCDownloadTask.STATE.DOWNLOADING)
                {
                    if (m_worker != null) //理论上STATE.DOWNLOADING时，对应worker一定存在
                    {
                        m_worker.Abort(); //通过主动触发线程异常中断DLCDownloader
                        m_worker = null;
                    }
                    //看文档，Abort后只会再执行catch,finally代码块，目前只有Downloader中捕获了AbortException进行资源释放
                    //所以此处理论上不存在被线程覆盖掉INTERRUPTED的可能性
                    m_activeTask.state = DLCDownloadTask.STATE.INTERRUPTED;
                }
            }
        }

        private void OnDestroy()
        {
            if (m_worker != null)
            {
                m_worker.Abort();
                m_worker = null;
            }
        }

        public void PushTask(DLCFilesInfo dlcInfo)
        {
            if (m_activeTask != null && m_activeTask.dlcInfo.dlcName == dlcInfo.dlcName && m_activeTask.state == DLCDownloadTask.STATE.DOWNLOADING)
            {
                return;
            }
            foreach (DLCDownloadTask waitingTask in m_waitingTasks)
            {
                if (waitingTask.dlcInfo.dlcName == dlcInfo.dlcName)
                {
                    return;
                }
            }

            DLCDownloadTask task = new DLCDownloadTask(dlcInfo);
            m_waitingTasks.Enqueue(task);
        }

        /**
        *   一个线程只负责下载一个指定Task(m_activeTask，如要扩展并行多下载任务，理论上线程之间状态也互不影响)，如此TaskQueue的状态维护只在主线程中进行
        */
        private void WorkProc(DLCDownloadTask task)
		{
			for (int i = 0; i < task.dlcInfo.dlcFiles.Count; ++i)
            {
                if (!m_network_ok) return;   //中断检测点#1，阻止循环
                if (task.dlcInfo.dlcFilesReady[i]) continue;
                
                task.currIdx = i;
                AssetFileInfo fileInfo = task.dlcInfo.dlcFiles[i];
                string tempPath = FileSystem.TEMP_LOCAL_ROOT + fileInfo.path;
                string dstPath = FileSystem.LOCAL_ROOT + fileInfo.path;
                if (File.Exists(tempPath)) File.Delete(tempPath);   //删除之前残留的临时下载文件
                if (File.Exists(dstPath) && FileHelper.CalcFileMd5(dstPath, true) == fileInfo.md5)
                {
                    //DLC之间重叠文件，已被之前的任务下载完成
                    task.dlcInfo.dlcFilesReady[i] = true;
                    continue;
                }

                if (!m_network_ok) return;   //中断检测点#2，阻止Downloader创建
                DLCDownloader downloader = new DLCDownloader(fileInfo);
                int retryTimes = 0;
                int backupIdx = 0;
                string error = string.Empty;

                if (!m_network_ok) return;   //中断检测点#3，阻止Download执行
                while (retryTimes < RETRY_TIMES && !string.IsNullOrEmpty(error = downloader.Download(OnLoadedBytesProgress)))
                {
                    ++retryTimes;
                    if (!m_network_ok) return;   //中断检测点#4，阻止重试

                    string backupUrl = FileSystem.GetRemoteBackupUrl(downloader.origUrl, ref backupIdx);
                    if (!string.IsNullOrEmpty(backupUrl))
                    {
                        downloader.url = backupUrl;
                        retryTimes = 0;
                    }
                }
                
                task.dlcInfo.dlcFilesReady[i] = retryTimes < RETRY_TIMES;
                if (!task.dlcInfo.dlcFilesReady[i])
                {
                    task.error = error;
                    //线程中task.status状态变更作为最后执行步骤，不再写task
			        task.state = DLCDownloadTask.STATE.FAILED;
                    return;
                }
            }
            //线程中task.status状态变更作为最后执行步骤，不再写task
            task.state = DLCDownloadTask.STATE.SUCCESSED;
		}
    }

    public class DLCDownloader
    {
        private static int BLOCK_SIZE = 8 * 1024;
        private static int SPEED_LIMIT = 800 * 1024;    //800k/s
        private static int TIME_OUT = 5 * 1000;
        private static int READ_TIME_OUT = 5 * 1000;

        public string origUrl;
        public string url;
        public string randomParam;
        public string md5;
        public long size;
        public string dstPath;
        public string tempPath;

        private long m_head;


        public DLCDownloader(AssetFileInfo fileInfo)
        {
            this.origUrl = this.url = FileSystem.REMOTE_ROOT + fileInfo.path;
            this.randomParam = string.Empty;
            this.md5 = fileInfo.md5;
            this.size = fileInfo.size;
            this.dstPath = FileSystem.LOCAL_ROOT + fileInfo.path;
            this.tempPath = FileSystem.TEMP_LOCAL_ROOT + fileInfo.path;
        }

        public string Download(System.Action<long> progressCallback)
        {
            HttpWebResponse response = null;
            FileStream fileStream = null;
            try
            {
                FileHelper.CreateDirectoriesByPath(tempPath);

                fileStream = new FileStream(tempPath, FileMode.OpenOrCreate, FileAccess.Write);
                m_head = fileStream.Length;
                fileStream.Seek(m_head, SeekOrigin.Begin);

                HttpWebRequest request = HttpWebRequest.Create(url + randomParam) as HttpWebRequest;
                request.Method = "GET";
                request.AddRange(m_head);
                request.Timeout = TIME_OUT;

                byte[] buffer = new byte[BLOCK_SIZE];

                response = (HttpWebResponse)request.GetResponse();
                Stream stream = response.GetResponseStream();
                stream.ReadTimeout = READ_TIME_OUT;
                ThrottledStream CurStream = new ThrottledStream(stream, SPEED_LIMIT);
                int readCount = -1;
                while ((readCount = CurStream.Read(buffer, 0, BLOCK_SIZE)) > 0)
                {
                    m_head += readCount;
                    fileStream.Write(buffer, 0, readCount);
                    if (progressCallback != null) progressCallback(m_head);
                }

                response.Close();
                fileStream.Close();

                if (m_head == size && FileHelper.CalcFileMd5(tempPath) == md5)
                {
                    if (File.Exists(dstPath)) File.Delete(dstPath);
                    FileHelper.RawDeleteFileMd5Cache(dstPath);//务必注意顺序保证异常逻辑也是正常的
                    File.Move(tempPath, dstPath);
                    FileHelper.RawWriteFileMd5Cache(dstPath, md5);
                    return string.Empty;
                }
                else
                {
                    if (string.IsNullOrEmpty(randomParam)) randomParam = "?" + FileSystem.remoteResNo;
                    return "invalide file size or md5.";
                }
            }
            catch (Exception e)
            {
                if (response != null) response.Close();
                if (fileStream != null) fileStream.Close();
                return e.Message;
            }
        }
    }
}










        // public void DownloadDLC(string dlcName, System.Action<string> completeCallback, System.Action<string, string, float, float, int, int, long, long> progressCallback, System.Action<string, string> errorCallback)
        // {
        //     DLCFilesInfo dlcInfo;
        //     if (!s_dlcInfoMap.TryGetValue(dlcName, out dlcInfo))
        //     {
        //         Framework.Log.Logger.LogError("DLC Info Not Found: " + dlcName);
        //         return;
        //     }
        //     if (dlcInfo.allReady)
        //     {
        //         if (completeCallback != null) completeCallback(dlcName);
        //         return; 
        //     }
        //     if (s_dlc_downloading_coroutine_dict.ContainsKey(dlcName))
        //     {
        //         Framework.Log.Logger.LogWarning("Duplicated Download DLC: " + dlcName);
        //         return;
        //     }

        //     if (s_dlc_downloader == null)
        //     {
        //         s_dlc_downloader = new GameObject("DLC Downloader").AddComponent<DLCFilesDownloader>();
        //         GameObject.DontDestroyOnLoad(s_dlc_downloader.gameObject);
        //     }

        //     List<FileSession> sessions = new List<FileSession>();
        //     for (int i = 0; i < dlcInfo.dlcFiles.Count; ++i)
        //     {
        //         if (dlcInfo.dlcFilesReady[i]) continue;
        //         AssetFileInfo fileInfo = dlcInfo.dlcFiles[i];
        //         string tempFilePath = TEMP_LOCAL_ROOT + fileInfo.path;
        //         FileSession session = new FileSession(remoteResNo, "dlc_downloading", fileInfo.path, REMOTE_ROOT + fileInfo.path, tempFilePath, fileInfo.size, fileInfo.md5, false);
        //         sessions.Add(session);
        //     }
        //     if (sessions.Count <= 0)
        //     {
        //         if (completeCallback != null) completeCallback(dlcName);
        //         return;  
        //     }

        //     s_dlc_downloading_coroutine_dict.Add(dlcName, s_dlc_downloader.StartCoroutine(DoDownloadDLC(dlcInfo, sessions, completeCallback, progressCallback, errorCallback)));
        // }

        // private IEnumerator DoDownloadDLC(DLCFilesInfo dlcInfo, List<FileSession> sessions, System.Action<string> completeCallback, System.Action<string, string, float, float, int, int, long, long> progressCallback, System.Action<string, string> errorCallback)
        // {
        //     System.Text.StringBuilder errorSB = new System.Text.StringBuilder();
        //     yield return DoFileSessions(sessions, progressCallback, errorSB, 0);
        //     s_dlc_downloading_coroutine_dict.Remove(dlcInfo.dlcName);
        //     if (errorSB.Length > 0)
        //     {
        //         for (int i = 0; i < dlcInfo.dlcFilesReady.Count; ++i)
        //         {
        //             if (!dlcInfo.dlcFilesReady[i])
        //             {
        //                 if (System.IO.File.Exists(dlcInfo.dlcFiles[i].realPath))
        //                 {
        //                     string localMD5 = FileHelper.CalcFileMd5(dlcInfo.dlcFiles[i].realPath);
        //                     if (localMD5 == dlcInfo.dlcFiles[i].md5) dlcInfo.dlcFilesReady[i] = true;
        //                 }
        //             }
        //         }
        //         errorCallback(dlcInfo.dlcName, errorSB.ToString());
        //     }
        //     else
        //     {
        //         for (int i = 0; i < dlcInfo.dlcFilesReady.Count; ++i)
        //         {
        //             dlcInfo.dlcFilesReady[i] = true;
        //         }
        //         completeCallback(dlcInfo.dlcName);
        //     }
        // }