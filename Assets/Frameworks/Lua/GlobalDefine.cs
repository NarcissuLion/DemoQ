using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Lua
{
    public class GlobalDefine
    {
#if UNITY_EDITOR
        public static string LUA_ROOT_DIR = Application.dataPath.Substring(0, Application.dataPath.Length - 6) + "Lua/";
        public static string PB_DESC_FILE = LUA_ROOT_DIR + "protobuf/pb/descriptor.pb";
#endif

    }
}
