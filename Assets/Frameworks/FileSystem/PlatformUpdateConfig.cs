using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Xml;

namespace Framework.FileSys
{
    public class PlatformUpdateConfig
    {
        public static PlatformUpdateConfig LoadConfig(bool Obb = false)
        {
            PlatformUpdateConfig config = new PlatformUpdateConfig();
            string error;
            string localplatform = FileHelper.ReadAllText(FileSystem.UPDATE_CONFIG_PLATFORM_TYPE_FILEPATH, out error).Trim();
            string platform_type = string.IsNullOrEmpty(error) ? localplatform : "default";
            if (Obb)
            {
                platform_type = platform_type + "-obb";
            }
            string allplatformcontent = FileHelper.ReadAllText(FileSystem.UPDATE_CONFIG_ALLPLATFORM_FILEPATH, out error);
            if (string.IsNullOrEmpty(error))
            {
                XmlDocument xmldoc = new XmlDocument();
                xmldoc.LoadXml(allplatformcontent);
                XmlNodeList nodeList = xmldoc.SelectSingleNode("all_platform_config").ChildNodes;
                for (var i = 0; i < nodeList.Count; i++)
                {
                    if (nodeList[i].Name == "remote_backups")
                    {
                        foreach (XmlElement backupNode in nodeList[i].ChildNodes)
                        {
                            config.Resource_Update_Backups.Add(backupNode.GetAttribute("remote_ip"));
                            config.Resource_Update_Backups.Add(backupNode.GetAttribute("backup_ip"));
                        }
                    }
                    else if (nodeList[i].Name == platform_type)
                    {
                        XmlAttributeCollection xac = nodeList[i].Attributes;
                        string ip = xac.GetNamedItem("remote_ip").Value;
                        string root = xac.GetNamedItem("remote_root").Value;
                        if (!string.IsNullOrEmpty(ip) && !string.IsNullOrEmpty(root))
                        {
                            config.Resource_Update_RemoteIP = ip.Replace("\"", "");
                            config.Resource_Update_RemoteRoot = root.Replace("\"", "");
                            Debug.Log("UpdateLoadConfig:" + nodeList[i].Name);
                            Debug.Log("RemoteIP:" + config.Resource_Update_RemoteIP);
                            Debug.Log("RemoteRoot:" + config.Resource_Update_RemoteRoot);
                            return config;
                        }
                        else
                        {
                            Debug.Log("Error:UpdateConfig read all_platform_config error");
                            return null;
                        }
                    }
                }
                Debug.Log("Error:UpdateConfig not find platform_type error");
            }
            else
            {
                Debug.LogFormat("Error:UpdateConfig Read Asset Config:{0}", error);
            }

            return null;
        }

        public string Resource_Update_RemoteIP;
        public string Resource_Update_RemoteRoot;
        public List<string> Resource_Update_Backups = new List<string>();
    }
}