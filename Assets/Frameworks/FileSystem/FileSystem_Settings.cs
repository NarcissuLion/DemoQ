using System.Collections.Generic;
using UnityEngine;
using System;

namespace Framework.FileSys
{
    public partial class FileSystem
    {
#region network-settings
        public const int DEFAULT_REQUEST_TIME_OUT = 30;
        public const int LOWEST_DOWNLOAD_SPEED = 1024 * 20;
        public const int REQUEST_MAX_RETRY_TIMES = 2;

        public static string REMOTE_IP = "http://localhost:8090/";

        public static string[] REMOTE_BACKUPS = new string[0]{};
        public static string GetRemoteBackupUrl(string origUrl, ref int backupIdx)
        {
                try
                {
                        bool hitBackup = false;
                        for (; backupIdx < REMOTE_BACKUPS.Length - 1; backupIdx += 2)
                        {
                                if (origUrl.IndexOf(REMOTE_BACKUPS[backupIdx]) < 0) continue;
                                hitBackup = true;
                                break;
                        }
                        if (hitBackup)
                        {
                                string newUrl = origUrl.Replace(REMOTE_BACKUPS[backupIdx], REMOTE_BACKUPS[backupIdx + 1]);
                                backupIdx += 2;
                                UnityEngine.Debug.Log("Fallback to backup url: " + backupIdx + " => " + newUrl);
                                return newUrl;
                        }
                        else
                        {
                                return string.Empty;
                        }
                }
                catch (Exception)
                {
                        return string.Empty;
                }
        }
#endregion

#region path-roots
        public static string CONFIGS_IN_PACK_ROOT = "RES::Config/";  // Resources.Load
        public static string PACKAGE_ROOT = Application.streamingAssetsPath + "/";
        public static string REMOTE_ROOT = REMOTE_IP + "knight_res/test/";
#if UNITY_EDITOR
        public static string LOCAL_ROOT = "__LocalPersistent/Res/";
        public static string TEMP_LOCAL_ROOT = "__LocalPersistent/Temp/";
#elif UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX//PC版本直接放到exe data目录下persistentDataPath一般会指向C盘
        public static string LOCAL_ROOT = Application.dataPath + "/persistentData/";
        public static string TEMP_LOCAL_ROOT = Application.dataPath + "/persistentData/Temp/";
#else
        public static string LOCAL_ROOT = Application.persistentDataPath + "/";
        public static string TEMP_LOCAL_ROOT = Application.persistentDataPath + "/Temp/";
#endif
#endregion

#region version-file-name
        public static string APP_NO = "AppNo.txt";
        public static string RES_NO = "ResNo.txt";
        public static string FILES_MD5 = "FilesMD5.txt";
        public static string DLC_MARKS = "DlcMarks.txt";

        public static string IN_BUNDLE_ASSETS_MAP = "bundles/InBundleAssetsMap.txt";
        public static string BUNDLE_DEPENDENCY = "bundles/BundleDependency.txt";
#endregion

#region version-file-path
        public static string PACKAGE_BUNDLE_PATH = PACKAGE_ROOT + "bundles/";
        public static string REMOTE_BUNDLE_PATH { get {return REMOTE_ROOT + "bundles/";}}
        public static string LOCAL_BUNDLE_PATH = LOCAL_ROOT + "bundles/";

        public static string PACKAGE_APP_NO = CONFIGS_IN_PACK_ROOT + APP_NO;
        public static string REMOTE_APP_NO { get { return REMOTE_ROOT + APP_NO; } }

        public static string PACKAGE_RES_NO = CONFIGS_IN_PACK_ROOT + RES_NO;
        public static string REMOTE_RES_NO { get { return REMOTE_ROOT + RES_NO; } }
        public static string LOCAL_RES_NO = LOCAL_ROOT + RES_NO;


        public static string PACKAGE_FILES_MD5 = PACKAGE_ROOT + FILES_MD5;
        public static string REMOTE_FILES_MD5 { get {return REMOTE_ROOT + FILES_MD5 ; }}
        public static string LOCAL_FILES_MD5 = LOCAL_ROOT + FILES_MD5;

        public static string LOCAL_DLC_MARKS = LOCAL_ROOT + DLC_MARKS;
#endregion

#region update-configs
        public static string UPDATE_CONFIG_PLATFORM_TYPE_FILEPATH = CONFIGS_IN_PACK_ROOT + "update_platform";
        public static string UPDATE_CONFIG_ALLPLATFORM_FILEPATH = CONFIGS_IN_PACK_ROOT + "update_allplatform";
        public static string UPDATE_LANGUAGE_FILEPATH = CONFIGS_IN_PACK_ROOT + "update_language";
        public static string UPDATE_LANGUAGE_FILEPATH_OVERRIDE = LOCAL_ROOT + "update_language.xml";
        public static string UPDATE_LANGUAGE_SELECT = LOCAL_ROOT + "update_language_select.txt";
        public static string APP_ENVIRONMENT_FILEPATH = CONFIGS_IN_PACK_ROOT + "app_environment";
#endregion
    }
}
