using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Lua;
using Framework.Network;
using Framework.Noti;
using XLua;

public class TestLuaMVC : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        LuaManager.Instance.AddLoadPath(GlobalDefine.LUA_ROOT_DIR + "Network/");
        LuaManager.Instance.Launch("main");
    }

    // Update is called once per frame
    void Update()
    {
    }

    void OnRecieveMsg(object msg)
    {
    }
}
