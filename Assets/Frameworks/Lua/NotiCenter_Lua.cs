using XLua;

namespace Framework.Noti
{
    public partial class NotiCenter
    {
        public void Dispatch_LuaTable(string notiName, LuaTable notiData, bool useCache = true)
        {
            Dispatch<LuaTable>(notiName, notiData, useCache);
        }

        public static void DispatchStatic_LuaTable(string notiName, LuaTable notiData, bool useCache = true)
        {
            DispatchStatic<LuaTable>(notiName, notiData, useCache);
        }
    }
}
