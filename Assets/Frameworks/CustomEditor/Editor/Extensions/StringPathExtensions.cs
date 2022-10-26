#if UNITY_EDITOR
namespace UnityEditor
{
    public static class StringPathExtensions
    {
        /// <summary>
        /// 将一个包含Resources路径转换为Resources的相对路径。
        /// </summary>
        /// <param name="withExtension">返回值是否包含文件扩展名</param>
        public static string ToReourcesPath(this string fullPath, bool withExtension = false)
        {
            const string ROOT_CHECK = "/resources/";
            fullPath = fullPath.Replace("\\", "/");
            int index = fullPath.ToLower().IndexOf(ROOT_CHECK);
            if (index < 0)
            {
                throw new System.Exception(string.Format("{0} 不是Resources路径", fullPath));
            }
            string path = fullPath.Substring(index + ROOT_CHECK.Length);
            if (!withExtension)
            {
                index = path.LastIndexOf('.');
                path = path.Remove(index);
            }
            return path;
        }

        /// <summary>
        /// 将一个全路径转换为Assets的相对路径。
        /// </summary>
        public static string ToAssetsPath(this string fullPath)
        {
            var curDir = System.Environment.CurrentDirectory.Replace("\\", "/") + "/";
            fullPath = fullPath.Replace("\\", "/");

            if (curDir.StartsWith(curDir))
            {
                fullPath = fullPath.Substring(curDir.Length);
            }
            return fullPath;
        }
    }
}
#endif