using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace Framework.FileSys
{
    public class HttpDownLoad
    {
        public long totalLength { get; private set; }
        public long fileLength { get; private set; }
        public bool done { get; private set; }
        public string error { get; private set; }

        public IEnumerator Start(string url, string filePath, long fileSize, string targetMd5, long resNo, string phase, string fileName, long basedBytes, long totalBytes, int currCount, int totalCount, System.Action<string, string, float, float, int, int, long, long> progressCallback)
        {
            totalLength = fileSize;
            fileLength = 0;
            done = false;
            
            string randomParam = string.Empty;

            FileHelper.CreateDirectoriesByPath(filePath);

            string origUrl = url;
            int backupIdx = 0;

            if (FileHelper.IsRemoteRequest(url))
            {
                // Debug.Log("INIT REQUEST: " + fileName + " -> " + url);
                byte[] datas = new byte[totalLength];
                if (File.Exists(filePath))
                {
                    try
                    {
                        byte[] bytes = File.ReadAllBytes(filePath);
                        System.Array.Copy(bytes, datas, bytes.Length);
                        fileLength = bytes.Length;
                        done = fileLength == totalLength;
                    }
                    catch (System.Exception e)
                    {
                        error = "RemoteRequest(Step1) Failed: " + url + " -> " + e.Message;
                        yield break;
                    }
                }
                while (!done)
                {
                    using (var request = UnityWebRequest.Get(url + randomParam))
                    {
                        float _timeDownLoad = Time.realtimeSinceStartup;
                        request.SetRequestHeader("Range", "bytes=" + fileLength + "-");
                        request.SendWebRequest();
                        // Debug.Log("BEGIN REQUEST [" + fileName + "]: " + fileLength + " -> " + totalLength + " >>>>");

                        int offset = 0;
                        float timeout = 0;
                        bool retry = false;
                        while (!request.isDone)
                        {
                            if (FileSystem.needInterruptDownloadSignal) // 为保证只打断远端下载，所以在这里拦住协程
                            {
                                error = "Interrupted";
                                yield break;
                            }
                            if (!string.IsNullOrEmpty(request.error))
                            {
                                error = request.error;
                                yield break;
                            }

                            yield return null;
                            byte[] buffer = request.downloadHandler.data;
                            if (buffer != null)
                            {
                                int length = buffer.Length - offset;
                                if (length > 0)
                                {
                                    try
                                    {
                                        System.Array.Copy(buffer, offset, datas, fileLength, length);
                                        offset += length;
                                        fileLength += length;
                                        timeout = 0;
                                        if (fileLength == totalLength)
                                        {
                                            File.WriteAllBytes(filePath, datas);
                                            // Debug.Log("WRITE DATA [" + fileName + "]: " + fileLength + " -> " + totalLength + " !!!!");
                                            done = true;
                                            float _resultTime = Time.realtimeSinceStartup - _timeDownLoad;
                                            // ThirdSDK.TuyooSdkUtil.GA_ReportJson("GAME", "c_cdndownload_speed", "speed_kbs", ((float)(request.downloadedBytes / 1024) / _resultTime).ToString());
                                        }
                                    }
                                    catch (System.Exception e)
                                    {
                                        error = "RemoteRequest(Step2) Failed: " + url + " -> " + e.Message;
                                        yield break;
                                    }
                                }
                                else
                                {
                                    timeout += Time.unscaledDeltaTime;
                                    if (timeout >= 5)
                                    {
                                        // Debug.Log("BREAK REQUEST [" + fileName + "]: " + fileLength + " -> " + totalLength + " <<<<");
                                        retry = true;
                                        break;
                                    }
                                }
                                if (progressCallback != null) progressCallback(phase, fileName, 1f * (basedBytes + fileLength) / totalBytes, 1f * fileLength / totalLength, totalCount, currCount, totalLength, fileLength);
                                if (retry) break;
                            }
                        }
                        if (!string.IsNullOrEmpty(request.error))
                        {
                            string backupUrl = FileSystem.GetRemoteBackupUrl(origUrl, ref backupIdx);
                            if (string.IsNullOrEmpty(backupUrl))
                            {
                                error = request.error;
                                yield break;
                            }
                            else
                            {
                                url = backupUrl;
                                if (File.Exists(filePath)) File.Delete(filePath);
                                fileLength = 0;
                                done = false;
                            }
                        }
                    }

                    if (done && FileHelper.CalcFileMd5(filePath, true, true) != targetMd5) //下载完成检查一下md5
                    {
                        FileHelper.RawDeleteFileMd5Cache(filePath);
                        if (string.IsNullOrEmpty(randomParam)) //如果还没绕过cdn缓存，绕一下试试
                        {
                            Debug.Log("TRY CDN AGAIN: " + fileName);
                            randomParam = "?" + resNo;
                            File.Delete(filePath);
                            fileLength = 0;
                            done = false;
                        }
                        else    //绕过md5还是不对，
                        {
                            string backupUrl = FileSystem.GetRemoteBackupUrl(origUrl, ref backupIdx);
                            if (string.IsNullOrEmpty(backupUrl))    //没有备用url了
                            {
                                Debug.Log("TRY CDN FAILED: " + fileName);
                                done = false;
                                error = string.Format("Download MD5 Error: {0}", filePath);
                                // ThirdSDK.TuyooSdkUtil.GA_ReportJson("GAME", "c_cdndownloadfailed", "filename", filePath);//极端情况上报GA
                                yield break;
                            }
                            else    //尝试备用url
                            {
                                url = backupUrl;
                                File.Delete(filePath);
                                fileLength = 0;
                                done = false;
                            }
                        }
                    }
                }

                if (progressCallback != null) progressCallback(phase, fileName, 1f * (basedBytes + fileLength) / totalBytes, 1f * fileLength / totalLength, totalCount, currCount, totalLength, fileLength);
                // Debug.Log("FINISH REQUEST [" + fileName + "]: " + (System.IO.File.ReadAllBytes(filePath).Length) + " -> " + fileLength + " ====================================================");
            }
            else
            {
                using (var request = UnityWebRequest.Get(url))
                {
                    request.timeout = FileSystem.DEFAULT_REQUEST_TIME_OUT;
                    request.SendWebRequest();
                    var eProgress = ShowProgress(request, phase, fileName, currCount * 1f / totalCount, totalCount, currCount, fileSize, progressCallback);
                    yield return eProgress;
                    if (!string.IsNullOrEmpty(request.error))
                    {
                        error = "Web Request From FileSystem Failed: " + url + " -> " + request.error;
                        yield break;
                    }
                    else
                    {
                        try
                        {
                            File.WriteAllBytes(filePath, request.downloadHandler.data);
                            done = true;
                        }
                        catch (System.Exception e)
                        {
                            error = "Web Request From FileSystem Failed: " + url + " -> " + e.Message;
                            yield break;
                        }
                    }
                }
            }
        }

        private IEnumerator ShowProgress(UnityWebRequest request, string phase, string name, float totalProgress, int length, int index, long size, System.Action<string, string, float, float, int, int, long, long> progressCallback)
        {
            // bool blocked = false;
            // ulong downloadedBytes = request.downloadedBytes;
            while (!request.isDone)
            {
                // if (downloadedBytes == request.downloadedBytes)
                // {
                //     if (blocked)
                //     {
                //         blocked = true;
                //         request.timeout = FileSystem.DEFAULT_REQUEST_TIME_OUT;
                //         Debug.Log("BLOCKED");
                //     }
                // }
                // else
                // {
                //     blocked = false;
                //     request.timeout = 0;
                //     downloadedBytes = request.downloadedBytes;
                // }

                if (progressCallback != null) progressCallback(phase, name, totalProgress, request.downloadProgress, length, index, size, (long)request.downloadedBytes);
                yield return null;
            }
        }
    }
}