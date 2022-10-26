using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using UnityEngine.Networking;
using System.Linq;
using System;
using Framework.Noti;

namespace Framework.FileSys
{
    public partial class FileSystem
    {
        //文件的版本信息
        public class FileVersion
        {
            public string name;         //文件名
            public string extension;    //文件后缀
            public string md5;          //文件md5
            public long size;           //文件大小
            public string belongDLC;    //所属DLC名
            public FileVersion(string name, string md5, long size, string belongDlc)
            {
                this.name = name;
                this.md5 = md5;
                this.size = size;
                this.belongDLC = belongDlc;
                int dotIdx = name.LastIndexOf(".") + 1;
                if (dotIdx > 0 && dotIdx < name.Length - 1) this.extension = name.Substring(dotIdx);
            }

            public bool isInLaterDLC
            {
                //__APP跟包文件和__FIRST首个DLC需要立刻更新
                get { return !string.IsNullOrEmpty(belongDLC) && belongDLC != "__APP" && belongDLC != "__FIRST"; }
            }

            public override string ToString()
            {
                return name+"|"+md5+"|"+size+"|"+belongDLC;
            }
        }

        //文件更新会话
        public class FileSession
        {
            public long resNo;    //构建号
            public string phase;    //更新阶段
            public string name;     //文件名
            public string url;      //更新url
            public string path;     //写入路径
            public long size;       //文件大小
            public string md5;      //文件md5
            public bool skippable;  //可否跳过

            public FileSession(long resNo, string phase, string name, string url, string path, long size, string md5, bool skippable)
            {
                this.resNo = resNo;
                this.phase = phase;
                this.name = name;
                this.url = url;
                this.path = path;
                this.size = size;
                this.md5 = md5;
                this.skippable = skippable;
            }
        }

        private Dictionary<string, FileVersion> m_localVerMap;      //设备本地文件版本信息表     P.S.只为不开启更新的全包构建资源表时使
        private Dictionary<string, FileVersion> m_packageVerMap;    //游戏包内文件版本信息
        private Dictionary<string, FileVersion> m_remoteVerMap;     //远端服务器文件版本信息
        private HashSet<string> m_dlcMarks;                         //以安装DLC标记

        /**
        *   构建文件版本信息表
        */
        private IEnumerator ParseFileVerInfo(string[] lines, Dictionary<string, FileVersion> map)
        {
            float timer = Time.realtimeSinceStartup;
            map.Clear();
            foreach (string line in lines)
            {
                string[] strArr = line.Split('|');
                if (strArr.Length != 3 && strArr.Length != 4) //兼容一下老MD5File，没有第4项认为不在任何DLC中
                {
                    if (line.Trim().Length > 0) Debug.LogError("FileSystem->ParseFileVerInfo Invalide line : " + line);
                    continue;
                }
                long size;
                long.TryParse(strArr[2], out size);
                string dlc = strArr.Length == 4 ? strArr[3].Trim() : null;
                FileVersion verInfo = new FileVersion(strArr[0], strArr[1], size, dlc);
                map.Add(verInfo.name, verInfo);

                if (Time.realtimeSinceStartup - timer > 0.02)
                {
                    yield return null;
                    timer = Time.realtimeSinceStartup;
                }
            }
        }

        public static long localResNo;        //本地构建号
        public static long packageResNo;      //包体构建号
        public static long remoteResNo;       //远端构建号

        public List<FileSession> downloadSessions;      //待下载文件会话列表
        public List<FileSession> extractSessions;       //待解压文件会话列表

        public static bool needInterruptDownloadSignal;       //中断更新标记

#region prepare
        /**
        *   预备更新，检查更新文件版本，计算更新大小，设置下载解压任务列表
        */
        public IEnumerator PreUpdate(Action<string, string> errorCallback)
        {
            if (File.Exists(LOCAL_ROOT + "forceclear")) //版本修复，清空本地资源
            {
                // if (File.Exists(LOCAL_ROOT + "actiondata.dpc")) File.Delete(LOCAL_ROOT + "actiondata.dpc");
                // if (File.Exists(LOCAL_ROOT + "buffdata.dpc")) File.Delete(LOCALstring.Format_ROOT + "buffdata.dpc");
                // if (File.Exists(LOCAL_ROOT + "elementdata.dpc")) File.Delete(LOCAL_ROOT + "elementdata.dpc");
                // if (File.Exists(LOCAL_ROOT + "elementdata_split.dpc")) File.Delete(LOCAL_ROOT + "elementdata_split.dpc");
                // if (File.Exists(LOCAL_ROOT + "mazedata.dpc")) File.Delete(LOCAL_ROOT + "mazedata.dpc");
                // if (File.Exists(LOCAL_ROOT + "worldmapinfo.dpc")) File.Delete(LOCAL_ROOT + "worldmapinfo.dpc");
                // if (Directory.Exists(LOCAL_ROOT + "bundles")) Directory.Delete(LOCAL_ROOT + "bundles", true);
                // if (Directory.Exists(LOCAL_ROOT + "LanguageDPC")) Directory.Delete(LOCAL_ROOT + "LanguageDPC", true);
                File.Delete(LOCAL_ROOT + "forceclear");
            }

            yield return null;
            
            string error;

            m_localVerMap = new Dictionary<string, FileVersion>();
            m_packageVerMap = new Dictionary<string, FileVersion>();
            m_remoteVerMap = new Dictionary<string, FileVersion>();

            yield return null;
            string localResNoStr = FileHelper.ReadAllText(LOCAL_RES_NO, out error);
            localResNo = string.IsNullOrEmpty(error) ? long.Parse(localResNoStr) : 0;

            yield return null;
            string packageResNoStr = FileHelper.ReadAllText(PACKAGE_RES_NO, out error);
            packageResNo = string.IsNullOrEmpty(error) ? long.Parse(packageResNoStr) : 0;
            // if (!string.IsNullOrEmpty(error))
            // {
            //     if (errorCallback != null) errorCallback("package_no", error);
            //     yield break;
            // }

            yield return null;
            string remoteResNoStr = REMOTE_IP.Contains("localhost") ? "0" : FileHelper.ReadAllText(REMOTE_RES_NO + FileHelper.GenerateRandomURLParam(12), out error);
            remoteResNo = string.IsNullOrEmpty(error) ? long.Parse(remoteResNoStr) : 0;
            if (!string.IsNullOrEmpty(error))
            {
                if (errorCallback != null) errorCallback("remote_no", error);
                yield break;
            }

            yield return null;
            string[] localMd5Lines = FileHelper.ReadAllLines(LOCAL_FILES_MD5, out error);
            if (string.IsNullOrEmpty(error)) yield return ParseFileVerInfo(localMd5Lines, m_localVerMap);

            yield return null;
            string[] packageMd5Lines = FileHelper.ReadAllLines(PACKAGE_FILES_MD5, out error);
            if (string.IsNullOrEmpty(error)) yield return ParseFileVerInfo(packageMd5Lines, m_packageVerMap);

            yield return null;
            if (remoteResNo > 0)
            {
                string[] remoteMd5Lines = FileHelper.ReadAllLines(REMOTE_FILES_MD5 + FileHelper.GenerateRandomURLParam(12), out error);
                if (string.IsNullOrEmpty(error)) yield return ParseFileVerInfo(remoteMd5Lines, m_remoteVerMap);
                else
                {
                    if (errorCallback != null) errorCallback("read_remote_md5", error);
                    yield break;
                }
            }

            yield return null;
            Debug.Log(string.Format("LOCAL:{0}  PACKAGE:{1}  REMOTE:{2}", localResNo, packageResNo, remoteResNo));

            // 通过DLC Marks文件，设置已经下载完成的DLC集合
            m_dlcMarks = new HashSet<string>();
            if (File.Exists(LOCAL_DLC_MARKS))
            {
                string[] dlcStrArr = File.ReadAllText(LOCAL_DLC_MARKS).Split(',');
                foreach (string dlcStr in dlcStrArr)
                {
                    if (string.IsNullOrEmpty(dlcStr)) continue;
                    m_dlcMarks.Add(dlcStr.Trim());
                }
            }

            downloadSessions = new List<FileSession>();
            extractSessions = new List<FileSession>();
            yield return GatherDownloadAndExtractRequests(downloadSessions, extractSessions);
        }

        /**
        *   获取需要下载文件总数量
        */
        public int GetNeedDownloadFileCount()
        {
            return downloadSessions == null ? 0 : downloadSessions.Count;
        } 
        /**
        *   获取需要下载文件总大小
        */
        public long GetNeedDownloadBytes()
        {
            if (downloadSessions == null) return 0;
            long totalBytes = 0;
            foreach (FileSession session in downloadSessions)
            {
                if (!session.skippable) totalBytes += session.size;
            }
            return totalBytes;
        }

        /**
        *   收集构建下载和解压任务列表
        */
        private IEnumerator GatherDownloadAndExtractRequests(List<FileSession> downloads, List<FileSession> extracts)
        {
            float timer = Time.realtimeSinceStartup;

            HashSet<string> _downloadFiles = new HashSet<string>(); //已确认需要下载的文件名单
            HashSet<string> _sameWithRemoteFiles = new HashSet<string>(); //本地和远端版本一致的文件

            int counter = 0;
            //Download
            foreach (FileVersion remoteVer in m_remoteVerMap.Values)
            {
                counter++;
                if (Time.realtimeSinceStartup - timer > 0.02)
                {
                    // if (m_remoteVerMap.Values.Count > 0)
                    // {
                    //     float fakeprogress = (float)counter / m_remoteVerMap.Values.Count;
                    //     // GameUpdater.instance.FakeProgress(fakeprogress, fakeprogress, 0);
                    //     NotiCenter.DispatchStatic<float, float, int>(FrameworkNotis.FILE_SYS_UPDATE_PROGRESS, fakeprogress, fakeprogress, 0);
                    // }
                    yield return null;
                    timer = Time.realtimeSinceStartup;
                }

                string localMd5 = FileHelper.CalcFileMd5(LOCAL_ROOT + remoteVer.name, true);

                FileVersion packageVer;
                m_packageVerMap.TryGetValue(remoteVer.name, out packageVer);

                if (localMd5 == remoteVer.md5) { _sameWithRemoteFiles.Add(remoteVer.name); continue; }    //本地文件和遠端文件一致
                if (packageVer != null && packageVer.md5 == remoteVer.md5) continue;    //虽然本地远端不一致，但是包内和遠端一致，等待解压
                if (packageVer != null && packageResNo > remoteResNo && packageVer.md5 != remoteVer.md5) continue;  //虽然包内远端也不一致，但是包内更新，等待解压
                if (remoteVer.isInLaterDLC && !m_dlcMarks.Contains(remoteVer.belongDLC)) continue; //资源所属的所有DLC都没有下载完成过，则这个资源在此处跳过，留到游戏中随DLC下载

                //添加下載任務
                string tempFilePath = TEMP_LOCAL_ROOT + remoteVer.name;
                bool skippable = false;
                if (File.Exists(tempFilePath)) // 临时文件内的MD5和远端不一致，上次断更后的残留文件不可续传，直接删除
                {
                    string tempMD5 = FileHelper.CalcFileMd5(tempFilePath, true);
                    if (tempMD5 == remoteVer.md5) skippable = true;
                }
                downloads.Add(new FileSession(remoteResNo, "downloading", remoteVer.name, REMOTE_ROOT + remoteVer.name, tempFilePath, remoteVer.size, remoteVer.md5, skippable));
                _downloadFiles.Add(remoteVer.name);
            }

            //Extract
            counter = 0;
            foreach (FileVersion packageVer in m_packageVerMap.Values)
            {
                counter++;
                if (Time.realtimeSinceStartup - timer > 0.02)
                {
                    // if (m_remoteVerMap.Values.Count > 0)
                    // {
                    //     float fakeprogress = (float)counter / m_packageVerMap.Values.Count;
                    //     // GameUpdater.instance.FakeProgress(fakeprogress, fakeprogress, 0);
                    //     NotiCenter.DispatchStatic<float, float, int>(FrameworkNotis.FILE_SYS_UPDATE_PROGRESS, fakeprogress, fakeprogress, 0);
                    // }
                    yield return null;
                    timer = Time.realtimeSinceStartup;
                }

                string localFilePath = LOCAL_ROOT + packageVer.name;
                string localMd5 = FileHelper.CalcFileMd5(localFilePath, true);

                if (_downloadFiles.Contains(packageVer.name)) continue; //文件已被列入下載任務
                if (localMd5 == packageVer.md5) continue;    //等待解压，但是本地和包内一致
                if (remoteResNo > packageResNo && _sameWithRemoteFiles.Contains(packageVer.name)) continue;  //虽然本地和包内不一致，但是本地与最新远端一致

                //添加解壓任務
                // Debug.Log("EXTRACT:" + PACKAGE_ROOT + packageVer.name);
                if (File.Exists(localFilePath)) File.Delete(localFilePath);
                FileHelper.RawDeleteFileMd5Cache(localFilePath);
                extracts.Add(new FileSession(packageResNo, "extracting", packageVer.name, PACKAGE_ROOT + packageVer.name, TEMP_LOCAL_ROOT + packageVer.name, packageVer.size, packageVer.md5, false));
            }
        }
#endregion

#region update
#if (UNITY_IPHONE || UNITY_IOS)
        private HashSet<string> m_inPackageFiles = new HashSet<string>();
#endif
        public IEnumerator DoUpdate(System.Action<string, string, float, float, int, int, long, long> progressCallback, System.Action<string, string> errorCallback)
        {
            string error;
            StringBuilder errorSB = new StringBuilder();
            //从远端下载更新文件
            if (downloadSessions != null && downloadSessions.Count > 0)
            {
                yield return DoFileSessions(downloadSessions, progressCallback, errorSB, 0);
                if (errorSB.Length > 0)
                {
                    errorCallback("downloading", errorSB.ToString());
                    yield break;
                }
            }

#if (UNITY_IPHONE || UNITY_IOS) //IOS平台直接读取包内资源
            if (extractSessions != null && extractSessions.Count > 0)  // 包内比本地新，需要拷贝
            {
                foreach (FileSession session in extractSessions) m_inPackageFiles.Add(session.name);
            }
#else   //非IOS平台解压资源到本地
            if (extractSessions != null && extractSessions.Count > 0)  // 包内比本地新，需要拷贝
            {
                yield return DoFileSessions(extractSessions, progressCallback, errorSB, 1);
                if (errorSB.Length > 0)
                {
                    errorCallback("extracting", errorSB.ToString());
                    yield break;
                }
            }
#endif

            //构建最新的文件版本表
            long newestResNo = Math.Max(remoteResNo, packageResNo);
            Dictionary<string, FileVersion> newestMap = new Dictionary<string, FileVersion>();
            foreach (KeyValuePair<string, FileVersion> kv in m_localVerMap)
            {
                newestMap.Add(kv.Key, kv.Value);
            }
            if (packageResNo > localResNo)
            {
                foreach (KeyValuePair<string, FileVersion> kv in m_packageVerMap)
                {
                    if (packageResNo < remoteResNo && m_remoteVerMap.ContainsKey(kv.Key)) continue;
                    if (newestMap.ContainsKey(kv.Key)) newestMap[kv.Key] = kv.Value;
                    else newestMap.Add(kv.Key, kv.Value);
                }
            }
            if (remoteResNo > localResNo)
            {
                foreach (KeyValuePair<string, FileVersion> kv in m_remoteVerMap)
                {
                    if (remoteResNo < packageResNo && m_packageVerMap.ContainsKey(kv.Key)) continue;
                    if (newestMap.ContainsKey(kv.Key)) newestMap[kv.Key] = kv.Value;
                    else newestMap.Add(kv.Key, kv.Value);
                }
            }

            yield return null;
            if (progressCallback != null) progressCallback("build_asset", string.Empty, 0f, 0f, 0, 0, 0, 0); 
            yield return null;

            //遍历最新的文件版本表，构建游戏内资源信息表
            float timer = Time.realtimeSinceStartup;
            var e = newestMap.GetEnumerator();
            int counter = 0;
            int totalCount = newestMap.Count;
            s_bundleInfoMap.Clear();
            s_assetInfoMap.Clear();
            s_dlcInfoMap.Clear();
            while (e.MoveNext())
            {
                counter++;
                FileVersion ver = e.Current.Value;
                AssetFileInfo info;
                if (FileHelper.IsBundleFile(ver.extension))
                {
                    info = new BundleFileInfo();
                    s_bundleInfoMap.Add(ver.name, (BundleFileInfo)info);
                }
                else
                {
                    info = new AssetFileInfo();
                    s_assetInfoMap.Add(ver.name, info);
                }
                info.path = ver.name;
                info.md5 = ver.md5;
                info.size = ver.size;
#if (UNITY_IPHONE || UNITY_IOS)
                info.realPath = (m_inPackageFiles.Contains(info.path) ? PACKAGE_ROOT : LOCAL_ROOT) + info.path;
#else
                info.realPath = LOCAL_ROOT + info.path;
#endif

                //构建DLC资源信息表
                if (ver.isInLaterDLC)
                {
                    DLCFilesInfo dlcInfo;
                    if (!s_dlcInfoMap.TryGetValue(ver.belongDLC, out dlcInfo))
                    {
                        dlcInfo = new DLCFilesInfo();
                        dlcInfo.dlcName = ver.belongDLC;
                        s_dlcInfoMap.Add(ver.belongDLC, dlcInfo);
                    }
                    dlcInfo.dlcFiles.Add(info);
                    if (info.realPath.StartsWith(PACKAGE_ROOT)) //包中的DLC资源直接标为ready
                    {
                        dlcInfo.dlcFilesReady.Add(true);
                    }
                    else //通过比较MD5，标记DLC资源ready状态
                    {
                        if (File.Exists(info.realPath))
                        {
                            string localMD5 = FileHelper.CalcFileMd5(info.realPath, true);
                            dlcInfo.dlcFilesReady.Add(localMD5 == info.md5);
                        }
                        else dlcInfo.dlcFilesReady.Add(false);
                    }
                }

                if (Time.realtimeSinceStartup - timer > 0.02)
                {
                    if (progressCallback != null) progressCallback("build_asset", string.Empty, counter * 1f / totalCount * 0.2f, 0f, 0, 0, 0, 0);
                    yield return null;
                    timer = Time.realtimeSinceStartup;
                }
            }

            yield return null;
            if (progressCallback != null) progressCallback("build_asset", string.Empty, 0.2f, 0f, 0, 0, 0, 0);
            yield return null;

            //遍历bundle内文件信息，补全游戏内资源信息表
            timer = Time.realtimeSinceStartup;
            AssetFileInfo inBundleMap;
            if (s_assetInfoMap.TryGetValue(IN_BUNDLE_ASSETS_MAP, out inBundleMap))
            {
                string[] lines = FileHelper.ReadAllLines(inBundleMap.realPath, out error);
                if (string.IsNullOrEmpty(error))
                {
                    for (int i = 0; i < lines.Length; ++i)
                    {
                        string line = lines[i];
                        string[] strArr = line.Split('|');
                        if (strArr.Length != 2)
                        {
                            Debug.LogError("FileSystem->ParseInBundleMap Invalide line : " + line);
                            continue;
                        }
                        string assetName = strArr[0];
                        string bundleName = strArr[1];
                        AssetFileInfo assetInfo = new AssetFileInfo();
                        assetInfo.path = assetName;
                        s_bundleInfoMap.TryGetValue(bundleName, out assetInfo.belongBundle);
                        s_assetInfoMap.Add(assetInfo.path, assetInfo);

                        if (Time.realtimeSinceStartup - timer > 0.02)
                        {
                            if (progressCallback != null) progressCallback("build_belong", 0.2f + string.Empty, i * 1f / lines.Length * 0.4f, 0f, lines.Length, i, 0, 0);
                            yield return null;
                            timer = Time.realtimeSinceStartup;
                        }
                    }
                }
                else
                {
                    if (errorCallback != null) errorCallback("building", error);
                    yield break;
                }
            }

            yield return null;
            if (progressCallback != null) progressCallback("build_belong", string.Empty, 0.6f, 0f, 0, 0, 0, 0);
            yield return null;

            //设置bundleDependency
            timer = Time.realtimeSinceStartup;
            AssetFileInfo bundleDependency;
            if (s_assetInfoMap.TryGetValue(BUNDLE_DEPENDENCY, out bundleDependency))
            {
                string[] lines = FileHelper.ReadAllLines(bundleDependency.realPath, out error);
                if (string.IsNullOrEmpty(error))
                {
                    for (int j = 0; j < lines.Length; ++j)
                    {
                        string line = lines[j];
                        string[] strArr = line.Split('|');
                        if (strArr.Length < 2)
                        {
                            Debug.LogError("FileSystem->ParseBundleDenpendency Invalide line : " + line);
                            continue;
                        }
                        BundleFileInfo bundleInfo;
                        if (s_bundleInfoMap.TryGetValue(strArr[0], out bundleInfo))
                        {
                            for (int i = 1; i < strArr.Length; i++)
                            {
                                BundleFileInfo dependBundleInfo;
                                if (s_bundleInfoMap.TryGetValue(strArr[i], out dependBundleInfo))
                                {
                                    if (bundleInfo.dependencies == null) bundleInfo.dependencies = new List<BundleFileInfo>(strArr.Length - 1);
                                    bundleInfo.dependencies.Add(dependBundleInfo);
                                }
                                else Debug.LogError("FileSystem->ParseBundleDenpendency Can't find the bundle : " + strArr[i]);
                            }
                        }
                        else Debug.LogError("FileSystem->ParseBundleDenpendency Can't find the bundle : " + strArr[0]);

                        if (Time.realtimeSinceStartup - timer > 0.02)
                        {
                            if (progressCallback != null) progressCallback("build_dependency", string.Empty, 0.6f + j * 1f / lines.Length * 0.15f, 0f, lines.Length, j, 0, 0);
                            yield return null;
                            timer = Time.realtimeSinceStartup;
                        }
                    }
                }
                else
                {
                    if (errorCallback != null) errorCallback("building", error);
                    yield break;
                }
            }

            yield return null;
            progressCallback("build_dependency", string.Empty, 0.75f, 0f, 0, 0, 0, 0);
            yield return null;

            // 写入最新的MD5文件和ResNo
            if (newestResNo > localResNo)
            {
                int idx = 0;
                string[] md5Lines = new string[newestMap.Count];
                foreach (KeyValuePair<string, FileVersion> kv in newestMap)
                {
                    md5Lines[idx++] = kv.Value.ToString();
                }
                File.WriteAllLines(LOCAL_FILES_MD5, md5Lines);
                File.WriteAllText(LOCAL_RES_NO, newestResNo + "");
            }

            yield return null;
            m_packageVerMap = null;
            m_remoteVerMap = null;
            downloadSessions = null;
            extractSessions = null;

            yield return null;
            System.GC.Collect();
            yield return null;
        }

        private IEnumerator DoFileSessions(List<FileSession> fileSessions, System.Action<string, string, float, float, int, int, long, long> progressCallback, StringBuilder errorSB, int seq)
        {
            if (!Directory.Exists(TEMP_LOCAL_ROOT)) Directory.CreateDirectory(TEMP_LOCAL_ROOT);
            float timer = Time.realtimeSinceStartup;
            int count = fileSessions.Count;
            long totalBytes = 0;
            long basedBytes = 0;
            foreach (FileSession session in fileSessions)
            {
                totalBytes += session.size;
            }

            HttpDownLoad http = null;
            int i = 0;
            while (i < count)
            {
                FileSession session = fileSessions[i];
                if (!session.skippable && File.Exists(session.path))
                {
                    FileHelper.RawDeleteFileMd5Cache(session.path);
                    File.Delete(session.path);
                }
                if (FileHelper.NeedWebRequest(session.url))
                {
                    if (http == null) http = new HttpDownLoad();
                    yield return http.Start(session.url, session.path, session.size, session.md5, session.resNo, session.phase, session.name, basedBytes, totalBytes, i+1, count, progressCallback);
                    if (http.done)
                    {
                        basedBytes += session.size;
                        ++i;
                    }
                    else
                    {
                        errorSB.AppendLine(!string.IsNullOrEmpty(http.error) ? http.error : "unknown error.");
                        yield break;
                    }
                }
                else
                {
                    if (Time.realtimeSinceStartup - timer > 0.02)
                    {
                        if (progressCallback != null) progressCallback(session.phase, session.name, (i * 1f / count), 0f, count, i+1, session.size, 0);
                        yield return null;
                        timer = Time.realtimeSinceStartup;
                    } 
                    try
                    {
                        string dstFilePath = session.path;
                        FileHelper.CreateDirectoriesByPath(dstFilePath);
                        FileHelper.RawDeleteFileMd5Cache(dstFilePath);
                        File.Copy(session.url, dstFilePath, true);
                        ++i;
                    }
                    catch (System.Exception e)
                    {
                        errorSB.AppendLine(e.Message);
                        yield break;
                    }
                }
            }

            if (errorSB.Length == 0)
            {
                i = 0;
                foreach (FileSession session in fileSessions)
                {
                    string srcFilePath = TEMP_LOCAL_ROOT + session.name;
                    string dstFilePath = LOCAL_ROOT + session.name;
                    try
                    {
                        string tempMD5 = FileHelper.CalcFileMd5(srcFilePath, true);
                        if (tempMD5 == session.md5)
                        {
                            FileHelper.CreateDirectoriesByPath(dstFilePath);
                            FileHelper.RawDeleteFileMd5Cache(dstFilePath);//顺序务必注意，这样即便异常也没问题。
                            if (File.Exists(dstFilePath)) File.Delete(dstFilePath);
                            File.Move(srcFilePath, dstFilePath);
                            FileHelper.RawWriteFileMd5Cache(dstFilePath, session.md5);
                            // if (session.name == LUA_BUNDLE_NAME) needReleaseLua = true;
                            // else if (session.name == TOLUA_BUNDLE_NAME) needReleaseToLua = true;
                        }
                        else
                        {
                            int tempLength = File.ReadAllBytes(srcFilePath).Length;
                            throw new Exception(string.Format("Check MD5 failed: {0} [{1}|{2}] [{3}|{4}]", session.name, tempMD5, tempLength, session.md5, session.size));
                        }
                        ++i;
                    }
                    catch (System.Exception e)
                    {
                        errorSB.AppendLine(e.Message);
                        yield break;
                    }
                    if (Time.realtimeSinceStartup - timer > 0.02)
                    {
                        //wang wei 问题在这里!之前没有更新进度条
                        if (progressCallback != null) progressCallback("moving", session.name, i * 1f / count, 0f, count, i + 1, session.size, 0);
                        yield return null;
                        timer = Time.realtimeSinceStartup;
                    }
                }
            }

            if (Directory.Exists(TEMP_LOCAL_ROOT)) Directory.Delete(TEMP_LOCAL_ROOT, true);
        }
#endregion
    }
}