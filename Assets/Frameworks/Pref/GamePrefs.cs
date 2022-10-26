using UnityEngine;
using System.Collections.Generic;
using System.Text;
using Framework.Utils;

namespace Framework.Pref
{    
    public class GamePrefs : Singleton<GamePrefs>
    {
        private StringBuilder _sb = new StringBuilder();

        public void ClearAll()
        {
            PlayerPrefs.DeleteAll();      
        }

        public void SetInt(string key, int value, string perfix = "")
        {
            if (!string.IsNullOrEmpty(perfix)) key = _sb.Append(perfix).Append("_").Append(key).ToString();
            PlayerPrefs.SetInt(key, value);
        }
        public int GetInt(string key, int defaultValue = 0, string perfix = "")
        {
            if (!string.IsNullOrEmpty(perfix)) key = _sb.Append(perfix).Append("_").Append(key).ToString();
            return PlayerPrefs.GetInt(key, defaultValue);
        }
        public void SetFloat(string key, float value, string perfix = "")
        {
            if (!string.IsNullOrEmpty(perfix)) key = _sb.Append(perfix).Append("_").Append(key).ToString();
            PlayerPrefs.SetFloat(key, value);
        }
        public float GetFloat(string key, float defaultValue = 0, string perfix = "")
        {
            if (!string.IsNullOrEmpty(perfix)) key = _sb.Append(perfix).Append("_").Append(key).ToString();
            return PlayerPrefs.GetFloat(key, defaultValue);
        }
        public void SetString(string key, string value, string perfix = "")
        {
            if (!string.IsNullOrEmpty(perfix)) key = _sb.Append(perfix).Append("_").Append(key).ToString();
            PlayerPrefs.SetString(key, value);
        }
        public string GetString(string key, string defaultValue = "", string perfix = "")
        {
            if (!string.IsNullOrEmpty(perfix)) key = _sb.Append(perfix).Append("_").Append(key).ToString();
            return PlayerPrefs.GetString(key, defaultValue);
        }
    }
}
