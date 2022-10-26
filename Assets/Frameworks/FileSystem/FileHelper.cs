using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using System.Security.Cryptography;
using System.Text;
using System;

namespace Framework.FileSys
{
    public class FileHelper
    {
        public static bool IsBundleFile(string extension)
        {
            return extension == string.Empty || extension == "unity3d";
        }

        public static bool NeedWebRequest(string path)
        {
            return path.StartsWith("http://") || path.StartsWith("https://") || path.StartsWith("ftp://") || path.StartsWith("file:///") || path.StartsWith("jar:");
        }

        public static bool IsAssetRequest(string path)
        {
            return path.StartsWith("RES::");
        }

        public static bool IsRemoteRequest(string path)
        {
            return path.StartsWith("http://") || path.StartsWith("https://") || path.StartsWith("ftp://");
        }

        private static Stack<DirectoryInfo> m_dirStack = new Stack<DirectoryInfo>();
        public static void CreateDirectoriesByPath(string path)
		{
            m_dirStack.Clear();
			DirectoryInfo dir = new DirectoryInfo(Path.GetDirectoryName(path));
			while (!dir.Exists)
			{
				m_dirStack.Push(dir);
				dir = dir.Parent;
			}

			while (m_dirStack.Count > 0)
			{
				dir = m_dirStack.Pop();
				dir.Create();
			}
		}

        public static string ReadAllText(string path, out string error, bool tryBackups = true)
        {
            error = string.Empty;
            string allText = string.Empty;
            string origPath = path;
            int backupIdx = 0;
            if (NeedWebRequest(path))
            {
                int tryTimes = 0;
                bool requestDone = false;
                while (!requestDone)
                {
                    ++tryTimes;
                    using (UnityWebRequest request = UnityWebRequest.Get(path))
                    {
                        request.timeout = FileSystem.DEFAULT_REQUEST_TIME_OUT;
                        UnityWebRequestAsyncOperation asyncOp = request.SendWebRequest();
                        while (!asyncOp.isDone) {}
                        if (!string.IsNullOrEmpty(request.error))
                        {
                            if (tryTimes > FileSystem.REQUEST_MAX_RETRY_TIMES)
                            {
                                if (tryBackups)     //尝试备用url
                                {
                                    string backupUrl = FileSystem.GetRemoteBackupUrl(origPath, ref backupIdx);
                                    if (string.IsNullOrEmpty(backupUrl))    //未命中备用url
                                    {
                                        error = request.error;
                                        requestDone = true;
                                    }
                                    else
                                    {
                                        path = backupUrl;
                                        tryTimes = 0;
                                    }
                                }
                                else
                                {
                                    error = request.error;
                                    requestDone = true;
                                }
                            }
                        }
                        else
                        {
                            allText = request.downloadHandler.text;
                            requestDone = true;
                        }
                    }
                }
                // UnityEngine.Debug.Log("ReadAllText(Web) -> " + path + " : " + error);
                return allText;
            }
            else if (IsAssetRequest(path))
            {
                try { allText = Resources.Load<TextAsset>(path.Substring(5)).text; }
                catch (System.Exception e) { error = e.Message; }
                // UnityEngine.Debug.Log("ReadAllText(File) -> " + path + " : " + error);
                return allText;
            }
            else
            {
                try { allText = File.ReadAllText(path); }
                catch (System.Exception e) { error = e.Message; }
                // UnityEngine.Debug.Log("ReadAllText(File) -> " + path + " : " + error);
                return allText;
            }
        }

        public static string[] ReadAllLines(string path, out string error, bool tryBackups = true)
        {
            error = string.Empty;
            string[] allLines = null;
            string origPath = path;
            int backupIdx = 0;
            if (NeedWebRequest(path))
            {
                int tryTimes = 0;
                bool requestDone = false;
                while (!requestDone)
                {
                    ++tryTimes;
                    using (UnityWebRequest request = UnityWebRequest.Get(path))
                    {
                        request.timeout = FileSystem.DEFAULT_REQUEST_TIME_OUT;
                        UnityWebRequestAsyncOperation asyncOp = request.SendWebRequest();
                        while (!asyncOp.isDone) {}
                        if (!string.IsNullOrEmpty(request.error))
                        {
                            if (tryTimes > FileSystem.REQUEST_MAX_RETRY_TIMES)
                            {
                                if (tryBackups)     //尝试备用url
                                {
                                    string backupUrl = FileSystem.GetRemoteBackupUrl(origPath, ref backupIdx);
                                    if (string.IsNullOrEmpty(backupUrl))    //未命中备用url
                                    {
                                        error = request.error;
                                        requestDone = true;
                                    }
                                    else
                                    {
                                        path = backupUrl;
                                        tryTimes = 0;
                                    }
                                }
                                else
                                {
                                    error = request.error;
                                    requestDone = true;
                                }
                            }
                        }
                        else
                        {
                            allLines = request.downloadHandler.text.Split('\n');
                            requestDone = true;
                        }
                    }
                }
                // UnityEngine.Debug.Log("ReadAllLines(Web) -> " + path + " : " + error);
                return allLines;
            }
            else if (IsAssetRequest(path))
            {
                try { allLines = Resources.Load<TextAsset>(path.Substring(5)).text.Split('\n'); }
                catch (System.Exception e) { error = e.Message; }
                // UnityEngine.Debug.Log("ReadAllLines(File) -> " + path + " : " + error);
                return allLines;
            }
            else
            {
                try { allLines = File.ReadAllLines(path); }
                catch (System.Exception e) { error = e.Message; }
                // UnityEngine.Debug.Log("ReadAllLines(File) -> " + path + " : " + error);
                return allLines;
            }
        }

        public static void Copy(string srcPath, string dstPath, out string error, bool tryBackups = true)
        {
            error = string.Empty;
            if (NeedWebRequest(dstPath))
            {
                error = "FileHelper->Copy dstPath is url : " + dstPath;
                return;
            }
            CreateDirectoriesByPath(dstPath);
            if (NeedWebRequest(srcPath) || IsAssetRequest(srcPath))
            {
                string text = ReadAllText(srcPath, out error, tryBackups);
                if (!string.IsNullOrEmpty(error)) return;

                try { File.WriteAllText(dstPath, text); }
                catch (System.Exception e) { error = e.Message; }
            }
            else
            {
                try
                {
                    RawDeleteFileMd5Cache(dstPath);
                    File.Copy(srcPath, dstPath, true);
                }
                catch (System.Exception e) { error = e.Message; }
            }
        }


        public static string CacheMd5Ext = ".mdc";
        private readonly static MD5 s_MD5 = MD5.Create();
        public static void RawDeleteFileMd5Cache(string path)
        {
//#if (UNITY_IPHONE || UNITY_IOS)
//            return;
//#endif
            string cachefile = path + CacheMd5Ext;
            if(File.Exists(cachefile))
                File.Delete(cachefile);
        }
        public static void RawWriteFileMd5Cache(string path, string md5)
        {
//#if (UNITY_IPHONE || UNITY_IOS)
//            return;
//#endif
            string cachefile = path + CacheMd5Ext;
            if (File.Exists(cachefile))
                File.Delete(cachefile);
            File.WriteAllText(cachefile, md5);
        }
        public static string CalcFileMd5(string path, bool usecache = false, bool forceFlushCache = false)
        {
//#if (UNITY_IPHONE || UNITY_IOS)
//            usecache = false;
//#endif
            //usecache仅在更新中使用true,同时不应在只读权限文件中进行,forceFlushCache用于强制更新
            if (usecache)
            {
                string md5 = "";
                string cachefile = path + CacheMd5Ext;
                if (!File.Exists(path))
                {
                    if (File.Exists(cachefile))
                        File.Delete(cachefile);
                    return md5;
                }

                if (File.Exists(cachefile))
                {
                    if (forceFlushCache)
                        File.Delete(cachefile);
                    else
                        md5 = File.ReadAllText(cachefile);
                }

                if (!string.IsNullOrEmpty(md5))
                    return md5;

                using (var stream = File.Open(path, FileMode.Open, FileAccess.Read))
                {
                    var hashData = s_MD5.ComputeHash(stream);
                    md5 = System.BitConverter.ToString(hashData).Replace("-", "");
                }
                File.WriteAllText(cachefile, md5);
                return md5;
            }
            else
            {
                using (var stream = File.Open(path, FileMode.Open, FileAccess.Read))
                {
                    var hashData = s_MD5.ComputeHash(stream);
                    return System.BitConverter.ToString(hashData).Replace("-", "");
                }
            }
        }

        private static StringBuilder _sb = new StringBuilder();
        public static string GenerateRandomURLParam(int num) 
        { 
            int number;
            char code; 
            _sb.Clear();
            _sb.Append('?');
            System.Random random = new System.Random(); 
            for (int i = 0; i < num; i++) 
            {
                number = random.Next(); 
                if (number % 2 == 0) code = (char)('0' + (char)(number % 10));
                else code = (char)('A' + (char)(number % 26));
                _sb.Append(code);
            } 
             
            return _sb.ToString();
        }
    }
}
