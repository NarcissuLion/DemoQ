using UnityEngine;
using XLua;

namespace Framework.Lua
{
    public class LuaMonoBehaivour : MonoBehaviour
    {
        public LuaTable luaTbl;
        private LuaFunction _luaStart;
        private LuaFunction _luaOnDestroy;
        private LuaFunction _luaOnEnable;
        private LuaFunction _luaOnDisable;
        private LuaFunction _luaUpdate;
        private LuaFunction _luaLateUpdate;
        private LuaFunction _luaFixedUpdate;

        public static void Create(LuaTable luaTbl, string monoName = "LuaMono")
        {
            GameObject go = new GameObject(monoName);
            GameObject.DontDestroyOnLoad(go);
            var luaMono = go.AddComponent<LuaMonoBehaivour>();
            luaMono.luaTbl = luaTbl;
        }

        void Start()
        {
            _luaStart = luaTbl.Get<LuaFunction>("Start");
            _luaOnDestroy = luaTbl.Get<LuaFunction>("OnDestroy");
            _luaOnEnable = luaTbl.Get<LuaFunction>("OnEnable");
            _luaOnDisable = luaTbl.Get<LuaFunction>("OnDisable");
            _luaUpdate = luaTbl.Get<LuaFunction>("Update");
            _luaLateUpdate = luaTbl.Get<LuaFunction>("LateUpdate");
            _luaFixedUpdate = luaTbl.Get<LuaFunction>("FixedUpdate");

            if (_luaStart != null) _luaStart.Call();
        }

        void OnDestroy()
        {
            if (_luaOnDestroy != null) _luaOnDestroy.Call();
        }

        void OnEnable()
        {
            if (_luaOnEnable != null) _luaOnEnable.Call();
        }

        void OnDisable()
        {
            if (_luaOnDisable != null) _luaOnDisable.Call();
        }

        void Update()
        {
            if (_luaUpdate != null) _luaUpdate.Call(Time.deltaTime, Time.unscaledTime);
        }

        void LateUpdate()
        {
            if (_luaLateUpdate != null) _luaLateUpdate.Call(Time.deltaTime, Time.unscaledTime);
        }

        void FixedUpdate()
        {
            if (_luaFixedUpdate != null) _luaFixedUpdate.Call(Time.fixedDeltaTime);
        }
    }
}