using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System;

namespace Framework.Utils
{

    public class NativeUtil
    {
        //apple id:1561915737
        public static void RestartApp(int delay)
        {
            //安卓上直接重启游戏的接口
    #if UNITY_ANDROID && !UNITY_EDITOR
            AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject mainActivity = jc.GetStatic<AndroidJavaObject>("currentActivity");
            mainActivity.Call("RestartApp", delay);
            jc.Dispose();
            mainActivity.Dispose();
    #endif

    #if UNITY_IPHONE || UNITY_IOS
            Application.Quit();//ios直接退出
    #endif
        }

        public static void openAPPMarket()
        {
            openAPPMarket(Application.identifier);
        }

        public static void OpenAppStore(string appID)
        {
    #if UNITY_IOS && !UNITY_EDITOR
            Application.OpenURL("itms-apps://itunes.apple.com/app/id" + appID);
    #endif
        }

        private static void openAPPMarket(string appid)
        {
    #if UNITY_ANDROID && !UNITY_EDITOR
            Debug.Log("openAPPMarket Google Start");
            //init AndroidJavaClass
            AndroidJavaClass UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"); ;
            AndroidJavaClass Intent = new AndroidJavaClass("android.content.Intent");
            AndroidJavaClass Uri = new AndroidJavaClass("android.net.Uri");
            // get currentActivity
            AndroidJavaObject currentActivity = UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject jstr_content = new AndroidJavaObject("java.lang.String", "market://details?id=" + appid);
            AndroidJavaObject intent = new AndroidJavaObject("android.content.Intent", Intent.GetStatic<AndroidJavaObject>("ACTION_VIEW"), Uri.CallStatic<AndroidJavaObject>("parse", jstr_content));
            currentActivity.Call("startActivity", intent);
            Debug.Log("openAPPMarket End");
    #endif

    #if UNITY_IPHONE || UNITY_IOS
            OpenAppStore("1561915737");
    #endif
            Debug.Log("openAPPMarket Calling");
        }

        public static void OpenFacebook()
        {
            Application.OpenURL("https://www.facebook.com/MagnumQuest");
        }

        public static string GetSignMd5()
        {
    #if UNITY_ANDROID && !UNITY_EDITOR
            AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject mainActivity = jc.GetStatic<AndroidJavaObject>("currentActivity");
            string SignMd5 = mainActivity.Call<string>("GetSignMd5Str");
            jc.Dispose();
            mainActivity.Dispose();
            //Debug.Log("GetSignMd5:" + SignMd5);
            return SignMd5;
    #endif
            return "UNKNOWN";
        }

        public static bool JudgeHaveApp(string pageName)
        {
    #if UNITY_ANDROID && !UNITY_EDITOR
            AndroidJavaClass up = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject ca = up.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject packageManager = ca.Call<AndroidJavaObject>("getPackageManager");
            AndroidJavaObject appList = packageManager.Call<AndroidJavaObject>("getInstalledPackages", 0);
            int num = appList.Call<int>("size");
            for (int i = 0; i < num; i++)
            {
                AndroidJavaObject appInfo = appList.Call<AndroidJavaObject>("get", i);
                string packageNew = appInfo.Get<string>("packageName");
                if (packageNew.CompareTo(pageName) == 0)
                {
                    return true;
                }
            }
            return false;
    #endif
            return true;
        }

        public static long GetFreeDiskSpace()//返回0说明失败，直接跳过即可
        {
    #if UNITY_ANDROID && !UNITY_EDITOR
            AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject mainActivity = jc.GetStatic<AndroidJavaObject>("currentActivity");
            long diskSpace = mainActivity.Call<long>("GetFreeDiskSpace");
            jc.Dispose();
            mainActivity.Dispose();
            Debug.Log("GetDiskSpace:" + diskSpace);
            return diskSpace;
    #endif
            Debug.Log("GetFreeDiskSpace Calling");
            return 0;
        }

        //仅安卓有效,persistentDataPath的图片文件将刷新至相册
        //FilePath举例：Application.persistentDataPath + "/sharephoto.png"
        public static void RefreshPhotoAlbumPreview(string FilePath)
        {
    #if UNITY_ANDROID && !UNITY_EDITOR
            using (AndroidJavaClass PlayerActivity = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                AndroidJavaObject playerActivity = PlayerActivity.GetStatic<AndroidJavaObject>("currentActivity");
                using (AndroidJavaObject Conn = new AndroidJavaObject("android.media.MediaScannerConnection", playerActivity, null))
                {
                    string[] paths = new string[] { FilePath };
                    Conn.CallStatic("scanFile", playerActivity, paths, null, null);
                }
            }
            /*
            using (AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                using (AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    jo.Call("scanFile", FilePath);
                }
            }
            */
    #endif
            Debug.Log(" Refresh PhotoAlbum Preview");
        }


        //是否是夜间模式
    #if UNITY_IPHONE || UNITY_IOS
    [DllImport("__Internal")]
    private static extern bool IOS_IsDarkMode();
    [DllImport("__Internal")]
    private static extern bool IOS_InAppReview();

    [DllImport("__Internal")]
    private static extern void IOS_RequestTrackingAuthorizationWithCompletionHandler();
    [DllImport("__Internal")]
    private static extern int IOS_GetAppTrackingAuthorizationStatus();

        [DllImport("__Internal")]
        private static extern void  IOS_SaveImageToPhotosAlbum(string path);
    #endif

        public static void RequestTrackingAuthorizationWithCompletionHandler_IOS()
        {
    #if UNITY_IPHONE || UNITY_IOS
            IOS_RequestTrackingAuthorizationWithCompletionHandler();
    #endif
        }

        public static int GetAppTrackingAuthorizationStatus_IOS()
        {
    #if UNITY_IPHONE || UNITY_IOS
            return IOS_GetAppTrackingAuthorizationStatus();
    #endif
            return -1;
        }

        public static bool IsDarkMode()
        {
    #if UNITY_IPHONE || UNITY_IOS
        return IOS_IsDarkMode();
    #endif
            return false;
        }


        public static bool InAppReview_iOS()
        {
    #if UNITY_IPHONE || UNITY_IOS
        return IOS_InAppReview();
    #endif
            return false;
        }

        public static void SaveImageToPhotosAlbum_IOS(string FilePath)
        {
    #if UNITY_IPHONE || UNITY_IOS
            IOS_SaveImageToPhotosAlbum(FilePath);
    #endif
        }

        public static void OpenInAppReview()
        {
    #if UNITY_ANDROID && !UNITY_EDITOR
            AndroidJavaClass playerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject mainActivity = playerClass.GetStatic<AndroidJavaObject>("currentActivity");
            mainActivity.Call("InAppReview");
            playerClass.Dispose();
            mainActivity.Dispose();
            Debug.Log("Call Android In-App Review");
    #endif

    #if UNITY_IPHONE || UNITY_IOS
            Debug.Log("Call iOS In-App Review not Impl");
    #endif
        }

        public static bool isReviewCanShow()
        {
            int day = DateTime.Today.DayOfYear;
            int pseudo_week = DateTime.Today.DayOfYear / 7;
            int pseudo_month = Mathf.Clamp(DateTime.Today.DayOfYear / 30, 0, 12);
            ReviewData jsonData;
            string data = PlayerPrefs.GetString("Review_data", "");
            if (data == "")
            {
                jsonData = new ReviewData();
                data = JsonUtility.ToJson(jsonData);
                PlayerPrefs.SetString("Review_data", data);
            }
            else
            {
                jsonData = JsonUtility.FromJson<ReviewData>(data);
            }

            if (jsonData == null)
            {
                Debug.Log("Error:ReviewData is Empty");
                return false;
            }

            bool isReturn = false;
            if (jsonData.lastDay == day)
            {
                Debug.Log("ReviewData:already show flow,today");
                isReturn = true;
            }
            else
            {
                jsonData.lastDay = day;
                //Debug.Log("ReviewData:next day");
            }

            if (jsonData.lastWeek == pseudo_week)
            {
                jsonData.weekTime++;
                if (jsonData.weekTime > 2)
                {
                    Debug.Log("ReviewData:already show flow,week");
                    isReturn = true;
                }
            }
            else
            {
                jsonData.lastWeek = pseudo_week;
                jsonData.weekTime = 0;
                //Debug.Log("ReviewData:next week");
            }

            if (jsonData.lastMonth == pseudo_month)
            {
                jsonData.monthTime++;
                if (jsonData.monthTime > 4)
                {
                    Debug.Log("ReviewData:already show flow,month");
                    isReturn = true;
                }
            }
            else
            {
                jsonData.lastMonth = pseudo_month;
                jsonData.monthTime = 0;
                //Debug.Log("ReviewData:next month");
            }

            //save
            string newdata = JsonUtility.ToJson(jsonData);
            PlayerPrefs.SetString("Review_data", newdata);
            if (isReturn)
                return false;

            return true;
        }
    }

    public class ReviewData
    {
        public int lastDay = -1;
        public int lastWeek = -1;
        public int lastMonth = -1;
        public int weekTime = 0;
        public int monthTime = 0;
    }
}