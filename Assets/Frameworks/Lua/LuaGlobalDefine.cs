using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Lua
{
    public class LuaGlobalDefine
    {
#if UNITY_EDITOR
        public static string LUA_ROOT_DIR = Application.dataPath.Substring(0, Application.dataPath.Length - 6) + "Lua/";
#elif UNITY_IOS
        public static string LUA_ROOT_DIR = Application.persistentDataPath + "/Lua/";
#elif UNITY_ANDROID
        public static string LUA_ROOT_DIR = Application.persistentDataPath + "/Lua/";
#endif
        public static string PB_MESSAGE_DESC_FILE = LUA_ROOT_DIR + "protobuf/messages.pb";
    }
}
