using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Utils;
using XLua;

namespace Framework.Lua
{
    public class LuaManager : Singleton<LuaManager>
    {
        private LuaEnv _env;
        private ILuaLoader _loader;

        public LuaManager()
        {
            //init env
            _env = new LuaEnv();
            _env.AddBuildin("pb", XLua.LuaDLL.Lua.LoadPB);
            SetLoader(new LuaLoader(), LuaGlobalDefine.LUA_ROOT_DIR);
        }

        public override void Dispose()
        {
            _env.Dispose();
            base.Dispose();
        }

        public void SetLoader(ILuaLoader loader, params string[] loadPaths)
        {
            _loader = loader;
            foreach (string loadPath in loadPaths)
            {
                _loader.AddLoadPath(loadPath);
            }
            _env.AddLoader(_loader.Load);
        }

        public void AddLoadPath(string loadPath)
        {
            _loader.AddLoadPath(loadPath);
        }

        public void Launch(string mainLua)
        {
            _env.DoString("require '" + mainLua + "'");
        }

        public byte[] LoadPB(string path)
        {
            return System.IO.File.ReadAllBytes(path);
        }
    }
}
