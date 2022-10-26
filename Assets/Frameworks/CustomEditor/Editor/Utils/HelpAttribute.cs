#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace UnityEditor
{

    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = true)]
    public class HelpAttribute : Attribute
    {
        public string Module = string.Empty;
        public string Title = string.Empty;
        public string Context = string.Empty;

        public bool DisplayContext { get; set; }
        public int ModuleOrder { get; set; }
        public int TitleOrder { get; set; }
        public int ContextOrder { get; set; }

        public HelpAttribute(string helpModule, string helpTitle, string helpContext)
        {
            Module = helpModule;
            Title = helpTitle;
            Context = helpContext;
        }
    }

    public static class HelpInfo
    {
        private static HelpAttribute[] s_helpAttributes;
        public static HelpAttribute[] HelpAttributes
        {
            get
            {
                if (s_helpAttributes == null)
                {
                    List<HelpAttribute> result = new List<HelpAttribute>();
                    Assembly[] assemblies = EditorReflectionUtilly.GetCustomAssemblies();
                    foreach (Assembly assembly in assemblies)
                    {
                        var helpAttributes = Attribute.GetCustomAttributes(assembly).Where(attr => attr is HelpAttribute).Select(attr => attr as HelpAttribute).ToArray();
                        result.AddRange(helpAttributes);

                        var types = assembly.GetTypes();
                        foreach (var type in types)
                        {
                            helpAttributes = Attribute.GetCustomAttributes(type).Where(attr => attr is HelpAttribute).Select(attr => attr as HelpAttribute).ToArray();
                            result.AddRange(helpAttributes);
                        }
                    }
                    s_helpAttributes = result.ToArray();
                }
                return s_helpAttributes;
            }
        }
        public static string GetHelpInfo(string helpModule)
        {
            HelpAttribute[] helpAttributes = GetHelpAttributes(helpModule);

            StringBuilder text = new StringBuilder();

            string currentTitle = string.Empty;

            foreach (HelpAttribute help in helpAttributes)
            {
                if (currentTitle != help.Title)
                {
                    currentTitle = help.Title;
                    text.AppendLine("\n\t" + help.Title + "\n");
                }
                text.AppendLine(help.Context);
            }

            return text.ToString();
        }

        public static HelpAttribute[] GetHelpAttributes(string helpModule)
        {
            HelpAttribute[] helpAttributes = HelpAttributes.Where(attr => attr.Module.Equals(helpModule)).ToArray();
            Array.Sort(helpAttributes, (a, b) =>
            {
                if (a.TitleOrder.Equals(b.TitleOrder))
                {
                    return a.ContextOrder.CompareTo(b.ContextOrder);
                }
                else
                {
                    return a.TitleOrder.CompareTo(b.TitleOrder);
                }
            });
            return helpAttributes;
        }

        //public static HelpAttribute[] GetAllHelpInfo()
        //{
        //    Dictionary<string, List<HeaderAttribute>> dict = new Dictionary<string, List<HeaderAttribute>>();
        //    foreach (HelpAttribute helpAttribute in HelpAttributes)
        //    {

        //    }
        //    List<HelpAttribute> result = new List<HelpAttribute>();

        //    return result.ToArray();
        //}
    }
}
#endif