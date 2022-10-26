using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;

namespace Framework.LuaMVC
{
    public class LuaViewFacade : MonoBehaviour
    {
        public string viewName;
        public List<LuaViewComponent> comps = new List<LuaViewComponent>();

        public void SetComps(LuaTable viewTbl)
        {
            foreach (LuaViewComponent comp in comps)
            {
                viewTbl.Set(comp.name, comp.target);
            }
        }
    }

    [System.Serializable]
    public class LuaViewComponent
    {
        public string type;  // 类型参见LuaViewFacadeEditor中的__supportedCompTypes
        public string name;
        public Object target;
        public string paramStr;
        public int paramInt;
        public Object paramObj;
    }
}
