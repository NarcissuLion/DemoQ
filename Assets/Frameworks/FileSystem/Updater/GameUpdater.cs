using UnityEngine;
using Framework.Noti;
using Framework.FileSys;
using Framework.Utils;
using System.Collections;
using UnityEngine.UI;
using System.Xml;
using System;
using System.Collections.Generic;
using UnityEngine.Events;
using DG.Tweening;
using Framework.Pref;
using Framework.Asset;

public class GameUpdater : MonoBehaviour
{
    private bool _network_ready;                        //网络环境状态
    private bool _user_premitted_without_wifi = true;   //用户授权非wifi环境更新状态
    private bool _download_failed;                      //下载失败信号
    private bool _step_done;                            //阶段步骤完毕信号

    private bool _interrupt_by_dialog;                  //对话框提示中断状态
    private bool _waiting_for_retry;                    //等待主动重试状态

    public Canvas canvas;                               //主Canvas
    public Text txt_debug;                              //debug信息文本框
    public GameObject logo;                             //游戏主LOGO
    public AudioSource bgm;                             //界面BGM

    [Header("出发面板")]
    public GameObject panel_go;                         //首次登录界面
    public Button btn_go;                               //首次登录确认按钮
    public Text label_btn_go;                           //首次登录确认按钮文案
    public Text tips_network;                           //首次登录网络提示信息

    [Header("更新面板")]
    public GameObject panel_update;                     //更新界面
    public Text tips_update;                            //更新信息文本
    public Text label_update_progress;                  //更新进度文本
    public Transform update_progress_bar_total;         //总更新进度条
    public Transform update_progress_bar_single;        //单文件更新进度条

    [Header("加载面板")]
    public GameObject panel_loading;                    //初始化加载面板
    public Text tips_loading;                           //初始化加载信息文本
    public Text label_loading_progress;                 //初始化加载进度文本
    public Transform loading_progress_bar;              //初始化加载进度条


    [Header("提示框")]
    public GameObject dialogRoot;                       //对话框节点
    public GameObject dialogStyleOk;                    //Ok风格节点
    public GameObject dialogStyleYesNo;                 //YesOrNo风格节点
    public Button dialogBtnOk;                          //Ok按钮
    public Button dialogBtnYes;                         //Yes按钮
    public Button dialogBtnNo;                          //No按钮
    public Text dialogTitle;                            //对话框标题文本
    public Text dialogContent;                          //对话框内容文本
    public Text touchScreen;                            //触屏提示文本

    [Header("参数")]
    public float startDelay = 1.5f;                     //初始等待时间

#if UNITY_EDITOR
    [Header("模拟线上")]
    public bool simOnlineResWorkflow;                   //打开模拟线上资源工作流
    public int remoteResVersion = 0;                    //目标内网测试资源版本
#endif

    private Dictionary<string, string> _language;       //更新界面文案表
    private string GetText(string key)
    {
        string text;
        if (!_language.TryGetValue(key, out text))
        {
            text = "missing text";
        }
        return text;
    }


    [HideInInspector]
    private bool _checking_wifi_healthy;                //wifi环境检查状态

    public static GameUpdater instance;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        DontDestroyOnLoad(gameObject);

#if UNITY_EDITOR
        if (simOnlineResWorkflow)
        {
            AssetManager.ASSETDATABASE_MODE = false;
        }
#endif
    }

    void Start()
    {
        if(logo != null) logo.SetActive(true);
        float bgmVolume = GamePrefs.Instance.GetFloat("BGM_VOLUME", 1f);
        bgm.volume = bgmVolume;
    }

    void OnDestroy()
    {
        instance = null;
    }

    private string GetTagByLanguage()
    {
        string error;
        if (System.IO.File.Exists(FileSystem.UPDATE_LANGUAGE_SELECT))
        {
            string selectTag = FileHelper.ReadAllText(FileSystem.UPDATE_LANGUAGE_FILEPATH, out error);
            if (string.IsNullOrEmpty(error)) return selectTag;
        }

        //首次无论如何也要根据系统语言判断
        SystemLanguage language = Application.systemLanguage;
        if (language == SystemLanguage.English)
        {
            return "en_us";
        }
        else if(language == SystemLanguage.Indonesian)
        {
            return "id_id";
        }
        else if(language == SystemLanguage.Portuguese)
        {
            return "pt_br";
        }
        else if (language == SystemLanguage.Russian)
        {
            return "ru_ru";
        }
        else if (language == SystemLanguage.Spanish)
        {
            return "es_es";
        }
        else if (language == SystemLanguage.Thai)
        {
            return "en_us";// "th_th";
        }
        else if (language == SystemLanguage.French)
        {
            return "fr_fr";
        }
        else if (language == SystemLanguage.German)
        {
            return "de_de";
        }
        else if (language == SystemLanguage.Vietnamese)
        {
            return "vn_vn";
        }
        else if (language == SystemLanguage.Italian)
        {
            return "it_it";
        }
        else if (language == SystemLanguage.Polish)
        {
            return "pl_pl";
        }
        else if (language == SystemLanguage.Turkish)
        {
            return "tu_tu";
        }
        else if (language == SystemLanguage.ChineseSimplified)
        {
            return "zh_cn";
        }
        else
        {
            return "en_us";
        }
    }

    /**
    *   wifi_download           tip 游戏会消耗网络流量
    *   start                   btn 开始
    *   tap_retry               touch 请触屏重试
    *   check_ver               tip 正在检查版本
    *   check_network           dialog 请检查网络状况
    *   updateapp_tips          dialog 请去商店更新
    *   reinstall_tips          dialog 请重新安装应用
    *   wifi_tips               dialog 本次更新消耗{0}流量
    *   initializing            tip 游戏初始化中
    *   downloading             tip 资源下载中
    *   extracting              tip 资源解压中
    *   network_error           dialog 网络异常
    */
    void ParseLanguage()
    {
        _language = new Dictionary<string, string>();
        string error;
        try
        {
            bool hasOverride = System.IO.File.Exists(FileSystem.UPDATE_LANGUAGE_FILEPATH_OVERRIDE);
            string languageContent = hasOverride ? FileHelper.ReadAllText(FileSystem.UPDATE_LANGUAGE_FILEPATH_OVERRIDE, out error) : FileHelper.ReadAllText(FileSystem.UPDATE_LANGUAGE_FILEPATH, out error);
            if (string.IsNullOrEmpty(error))
            {
                XmlDocument xmldoc = new XmlDocument();
                xmldoc.LoadXml(languageContent);

                string tagName = GetTagByLanguage();
                XmlNode langNode = xmldoc.GetElementsByTagName(tagName)[0];
                foreach (XmlNode node in langNode.ChildNodes)
                {
                    _language.Add(node.Name, node.InnerText);
                }
            }
        }
        catch
        {
            //覆盖的语言表出错，用内部的英文提示替换，发布时一定正确，不可能错误，错误就只能重新发包了
            _language.Clear();
            string languageContent = FileHelper.ReadAllText(FileSystem.UPDATE_LANGUAGE_FILEPATH, out error);
            if (string.IsNullOrEmpty(error))
            {
                XmlDocument xmldoc = new XmlDocument();
                xmldoc.LoadXml(languageContent);
                XmlNode langNode = xmldoc.GetElementsByTagName("en_us")[0];
                foreach (XmlNode node in langNode.ChildNodes)
                {
                    _language.Add(node.Name, node.InnerText);
                }
            }
        }
    }


    public IEnumerator DoUpdate()
    {
        panel_go.SetActive(false);
        panel_update.SetActive(false);
        panel_loading.SetActive(false);
        yield return new WaitForSeconds(startDelay);

        ParseLanguage();

#if !UNITY_IOS
        //第-2步，如果设备第一次运行游戏，显示出发按钮
        if (GamePrefs.Instance.GetInt("Network_Permitted", 0) == 0)
        {
            _step_done = false;
            panel_go.SetActive(true);
            tips_network.text = GetText("wifi_download");
            label_btn_go.text = GetText("start");
            btn_go.onClick.AddListener(()=>
            {
                GamePrefs.Instance.SetInt("Network_Permitted", 1);
                _step_done = true;
            });
            while (!_step_done) yield return null;
            panel_go.SetActive(false);
        }
#endif

        panel_update.SetActive(true);
        touchScreen.text = GetText("tap_retry");
        SetLabelContents(0f, 0f, GetText("check_ver"));
        yield return null;

        //第-1步，如果有新版本，要提示退出游戏，去商店更新
        _step_done = false;
        while (!_step_done)  //直到第-1步成功
        {
            while (_interrupt_by_dialog || _waiting_for_retry) yield return null;

            // 检查网络是否可用
            _network_ready = false;
            while (!_network_ready) //直到网络可用为止
            {
                while (_interrupt_by_dialog || _waiting_for_retry) yield return null;
                if (Application.internetReachability == NetworkReachability.NotReachable)
                {
                    ShowDialogOk(GetText("check_network"), OnBtnCheckNetwork);
                }
                else
                {
                    _network_ready = true;
                }
            }
            //远端拉取程序版本号,对比，如果远端大于本机，就要重装
            PlatformUpdateConfig updateconfig = PlatformUpdateConfig.LoadConfig();
            if (updateconfig != null)
            {
                FileSystem.REMOTE_IP = updateconfig.Resource_Update_RemoteIP;
                FileSystem.REMOTE_ROOT = updateconfig.Resource_Update_RemoteRoot;
                FileSystem.REMOTE_BACKUPS = updateconfig.Resource_Update_Backups.ToArray();
            }
            if (FileSystem.REMOTE_IP.Contains("localhost"))
            {
                Debug.Log("localhost,Skip Check App Version,Step Done");
                _step_done = true;
            }
            else
            {
                float t = Time.realtimeSinceStartup;
                string error = "";
                string packageAppNoStr = FileHelper.ReadAllText(FileSystem.PACKAGE_APP_NO, out error);
                long packageAppNo = string.IsNullOrEmpty(error) ? long.Parse(packageAppNoStr) : 0;
                yield return null;
                string remoteAppNoStr = FileSystem.REMOTE_IP.Contains("localhost") ? "0" : FileHelper.ReadAllText(FileSystem.REMOTE_APP_NO + FileHelper.GenerateRandomURLParam(12), out error);
                long remoteAppNo = string.IsNullOrEmpty(error) ? long.Parse(remoteAppNoStr) : 0;
                yield return null;

                Debug.LogFormat("App Version Package:{0} Remote:{1}", packageAppNo, remoteAppNo);
                if (!string.IsNullOrEmpty(error))
                {
                    //弹出个提示，停留在此步骤.确认后整体重试
                    //远端版本文件有问题，断言不可能有这个问题，只可能是网路问题
                    Debug.Log("RemoteAppNo is Null,Retry");
                    ShowDialogOk(GetText("check_network"), OnBtnCheckNetwork);
                }
                if (packageAppNo < remoteAppNo)//远端程序版本号比较大
                {
                    Debug.Log("New App Version founded");
                    ShowDialogOk(GetText("updateapp_tips"), OnBtnUpdateApp);
                    while (true) yield return null;//直接停在这里
                }
                else
                {
                    Debug.Log("App Version is Latest,next step");
                    _step_done = true;//没有问题了，就跳过此步骤
                }
            }
        }

#if KNIGHT_AAB_APP
        //第0步，仅在Google Play AAB情况下检测Install-time pack是否正常
        _step_done = false;
        while (!_step_done)  //直到第0步成功
        {
            while (_interrupt_by_dialog || _waiting_for_retry) yield return null;

            Google.Play.AssetDelivery.PlayAssetPackRequest apRequest = Google.Play.AssetDelivery.PlayAssetDelivery.RetrieveAssetPackAsync("InstallTimePack");
            while (!apRequest.IsDone)
            {
                yield return null; 
            }
            if (apRequest.Error != Google.Play.AssetDelivery.AssetDeliveryErrorCode.NoError)
            {
                ShowDialogOk(GetText("reinstall_tips"), OnBtnReinstall);
                while (true) yield return null;//直接停在这里
            }
            else 
            {
                Debug.Log("AAB Application,Check Install-Time Pack Exist,Step Done");
                _step_done = true;
            }
        }
#endif

        //第1+2步，更新
        _step_done = false;
        while (!_step_done) //第1+2步直到成功
        {
            while (_interrupt_by_dialog || _waiting_for_retry) yield return null;

            //第1步，收集更新信息
            _network_ready = false;
            while (!_network_ready) //直到网络可用为止
            {
                while (_interrupt_by_dialog || _waiting_for_retry) yield return null;
                if (Application.internetReachability == NetworkReachability.NotReachable)
                {
                    ShowDialogOk(GetText("check_network"), OnBtnCheckNetwork);
                }
                else //此处只下载远端版本号和md5文件，先不提示wifi环境
                {
                    _network_ready = true;
                }
            }
            _download_failed = false;
            yield return FileSystem.Instance.PreUpdate(OnUpdateError);

            //第2步，下载和解压热更文件
            if (!_download_failed)
            {
                _network_ready = false;
                while (!_network_ready) //直到网络可用为止
                {
                    while (_interrupt_by_dialog || _waiting_for_retry) yield return null;
                    if (Application.internetReachability == NetworkReachability.NotReachable)
                    {
                        ShowDialogOk(GetText("check_network"), OnBtnCheckNetwork);
                    }
                    else if (!_user_premitted_without_wifi && Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork) // 提示用户当前为非wifi环境
                    {
                        int fileCount = FileSystem.Instance.GetNeedDownloadFileCount();
                        if (fileCount > 0)
                        {
                            long totalBytes = FileSystem.Instance.GetNeedDownloadBytes();
                            ShowDialogYesNo(string.Format(GetText("wifi_tips"), SizeSuffix(totalBytes)), OnBtnYesWithNoWifi, OnBtnNoWithNoWifi);
                        }
                        else _network_ready = true;
                    }
                    else
                    {
                        _network_ready = true;
                    }
                }
                if (true)
                {
                    int fileCount = FileSystem.Instance.GetNeedDownloadFileCount();
                    if (fileCount > 0)
                    {
                        long totalMB = FileSystem.Instance.GetNeedDownloadBytes()/(1024*1024);
                        long freeDiskSpace = NativeUtil.GetFreeDiskSpace();//MB
                        Debug.LogFormat(">>>>>>>>>>>> totalMB:{0},freeDiskSpace:{1}", totalMB, freeDiskSpace);
                    }
                }
                _download_failed = false;
                _checking_wifi_healthy = true;
                yield return FileSystem.Instance.DoUpdate(OnUpdateProgress, OnUpdateError);
                _checking_wifi_healthy = false;
                if (!_download_failed) _step_done = true;
            }
        }
        SetLabelContents(0.75f, 0f, GetText("initializing"));
    }

    private Tween _tween;
    public void FakeProgress(float begin, float end, float duration)
    {
        if (_tween != null) _tween.Kill();
        if (duration > 0)
        {
            update_progress_bar_total.localScale = new Vector3(begin, 1f, 1f);
            _tween = update_progress_bar_total.DOScaleX(end, duration).SetEase(Ease.Linear);
        }
        else
        {
            update_progress_bar_total.localScale = new Vector3(end, 1f, 1f);
        }
    }

    void SetLabelContents(float progress_total, float progress_single, string label, bool waiting = false)
    {
        update_progress_bar_total.localScale = new Vector3(progress_total, 1f, 1f);
        update_progress_bar_single.localScale = new Vector3(progress_single, 1f, 1f);
        label_update_progress.text = label;
        touchScreen.gameObject.SetActive(waiting);
    }

    void BeginWaiting()
    {
        _waiting_for_retry = true;
        NotiCenter.AddStaticListener(ScreenToucher.NOTI_SCREEN_TOUCH_DOWN_IGNORE_UI, OnTouchDownRetry);
        SetLabelContents(0f, 0f, string.Empty, true);
    }
    void OnTouchDownRetry(INotification noti)
    {
        _waiting_for_retry = false;
        NotiCenter.RemoveStaticListener(ScreenToucher.NOTI_SCREEN_TOUCH_DOWN_IGNORE_UI, OnTouchDownRetry);
        SetLabelContents(0f, 0f, string.Empty, false);
    }

    void ShowDialogOk(string content, UnityAction callback)
    {
        _interrupt_by_dialog = true;
        dialogRoot.SetActive(true);
        dialogStyleOk.SetActive(true);
        dialogStyleYesNo.SetActive(false);
        dialogContent.text = content;
        dialogBtnOk.onClick.AddListener(callback);
    }

    void ShowDialogYesNo(string content, UnityAction yesCallback, UnityAction noCallback)
    {
        _interrupt_by_dialog = true;
        dialogRoot.SetActive(true);
        dialogStyleOk.SetActive(false);
        dialogStyleYesNo.SetActive(true);
        dialogContent.text = content;
        dialogBtnYes.onClick.AddListener(yesCallback);
        dialogBtnNo.onClick.AddListener(noCallback);
    }

    void OnBtnCheckNetwork()
    {
        _interrupt_by_dialog = false;
        dialogBtnOk.onClick.RemoveAllListeners();
        dialogRoot.SetActive(false);
        BeginWaiting();
    }

    void OnBtnYesWithNoWifi()
    {
        _interrupt_by_dialog = false;
        dialogBtnYes.onClick.RemoveAllListeners();
        dialogBtnNo.onClick.RemoveAllListeners();
        dialogRoot.SetActive(false);
        _user_premitted_without_wifi = true;
        _network_ready = true;
    }

    void OnBtnNoWithNoWifi()
    {
        _interrupt_by_dialog = false;
        dialogBtnYes.onClick.RemoveAllListeners();
        dialogBtnNo.onClick.RemoveAllListeners();
        dialogRoot.SetActive(false);
        BeginWaiting();
    }

    void OnBtnConfirmError()
    {
        _interrupt_by_dialog = false;
        dialogBtnOk.onClick.RemoveAllListeners();
        dialogRoot.SetActive(false);
        _network_ready = false;
        BeginWaiting();
    }

    void OnBtnUpdateApp()
    {
        dialogBtnOk.onClick.RemoveAllListeners();
        NativeUtil.openAPPMarket();
        Application.Quit();
    }

    void OnBtnReinstall()
    {
        //_interrupt_by_dialog = false; //对话框永远不消失
        dialogBtnOk.onClick.RemoveAllListeners();
        //dialogRoot.SetActive(false);
        //_network_ready = false;
        //打开商店，退出游戏:
        NativeUtil.openAPPMarket();
        Application.Quit();
    }

    void OnBtnRestart()
    {
        _interrupt_by_dialog = false;
        dialogBtnOk.onClick.RemoveAllListeners();
        dialogRoot.SetActive(false);
        _network_ready = false;
        //Restart Game:
        NativeUtil.RestartApp(0);
    }


    void OnUpdateProgress(string phase, string fileName, float totalProgress, float singleProgress, int totalCount, int currIndex, long totalSize, long loadedSize)
    {
        if (phase == "downloading")
        {
            SetLabelContents(totalProgress, singleProgress, string.Format("{0} {1}% [{2}/{3}]", GetText("downloading"), (int)(totalProgress * 100), currIndex, totalCount));
            tips_update.text = GetText("waitting_download");
        }
        else if (phase == "extracting")
        {
            SetLabelContents(totalProgress, singleProgress, string.Format("{0} {1}% [{2}/{3}]", GetText("extracting"), (int)(totalProgress * 100), currIndex, totalCount));
            tips_update.text = string.Empty;
        }
        else if (phase == "moving")
        {
            SetLabelContents(totalProgress, 0f, GetText("check_ver"));
            tips_update.text = string.Empty;
        }
        else if (phase.StartsWith("build_asset"))
        {
            SetLabelContents(totalProgress, 0f, GetText("initializing"));
            tips_update.text = string.Empty;
        }
        else if (phase.StartsWith("build_belong"))
        {
            SetLabelContents(totalProgress, 0f, GetText("initializing"));
            tips_update.text = string.Empty;
        }
        else if (phase.StartsWith("build_dependency"))
        {
            SetLabelContents(totalProgress, 0f, GetText("initializing"));
            tips_update.text = string.Empty;
        }
    }

    void OnUpdateError(string phase, string message)
    {
        //wifi丢失后强制停止下载
        if (phase == "downloading" && message.Contains("Interrupted"))
        {
            FileSystem.needInterruptDownloadSignal = false;
            Debug.Log("FORCE INTERRUPT DOWNLOAD");
        }

        _download_failed = true;
        Debug.Log("UPDATE ERROR: " + message);
        ShowDialogOk(GetText("network_error")/*"UPDATE ERROR: " + message*/, OnBtnConfirmError);
    }

    public void OnLoadingProgress(float progress, string label)
    {
        panel_update.SetActive(false);
        panel_loading.SetActive(true);        

        tips_loading.gameObject.SetActive(false);
        label_loading_progress.text = label;
        loading_progress_bar.localScale = new Vector3(progress, 1f, 1f);
    }

    string[] SizeSuffixes = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
    string SizeSuffix(long value, int decimalPlaces = 1)
    {
        if (decimalPlaces < 0) { throw new ArgumentOutOfRangeException("decimalPlaces"); }
        if (value == 0) { return string.Format("{0:n" + decimalPlaces + "} bytes", 0); }

        // mag is 0 for bytes, 1 for KB, 2, for MB, etc.
        int mag = (int)Math.Log(value, 1024);

        // 1L << (mag * 10) == 2 ^ (10 * mag) 
        // [i.e. the number of bytes in the unit corresponding to mag]
        decimal adjustedSize = (decimal)value / (1L << (mag * 10));

        // make adjustment when the value is large enough that
        // it would round up to 1000 or more
        if (Math.Round(adjustedSize, decimalPlaces) >= 1000)
        {
            mag += 1;
            adjustedSize /= 1024;
        }

        return string.Format("{0:n" + decimalPlaces + "} {1}", 
            adjustedSize, 
            SizeSuffixes[mag]);
    }

    private void Update()
    {
        if (_checking_wifi_healthy)
        {
            // 没有wifi并且用户未确认流量下载
            if (!_user_premitted_without_wifi && Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork) 
            {
                FileSystem.needInterruptDownloadSignal = true;
                _checking_wifi_healthy = false;
            }
        }
    }
}