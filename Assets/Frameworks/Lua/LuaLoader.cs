using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace Framework.Lua
{
    public class LuaLoader : ILuaLoader
    {
        private List<string> _loadPaths = new List<string>();

        public void AddLoadPath(string path)
        {
            if (_loadPaths.Contains(path)) return;
            if (!Directory.Exists(path)) return;
            _loadPaths.Add(path);
        }

        public byte[] Load(ref string filepath)
        {
            string finalPath = null;
            foreach (string loadPath in _loadPaths)
            {
                string realpath = loadPath + filepath.Replace(".", "/") + ".lua";
                if (File.Exists(realpath)) { finalPath = realpath; break; }
            }
            if (string.IsNullOrEmpty(finalPath)) throw new System.Exception("Can't find lua script: " + filepath);
            else return File.ReadAllBytes(finalPath);
        }
    }
}
