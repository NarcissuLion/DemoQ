using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Lua;
using System.IO;
using XLua;

public class DemoRoot : MonoBehaviour
{
    void Awake()
    {
        Application.targetFrameRate = 60;
        GameObject.DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        LuaManager.Instance.AddLoadPath(Application.streamingAssetsPath + "/Lua/"); // 留着这个LaodPath，打包的话直接丢到StreamingAssets下比较快
        LuaManager.Instance.Launch("DemoMain");
    }

    void Update()
    {

    }
}
