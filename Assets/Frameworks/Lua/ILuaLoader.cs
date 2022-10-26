
namespace Framework.Lua
{
    public interface ILuaLoader
    {
        void AddLoadPath(string path);
        byte[] Load(ref string filepath);
    }
}
