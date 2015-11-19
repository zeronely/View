using System;
using System.Text;
using UnityEngine;

namespace Spate
{
    /// <summary>
    /// PlayerPrefs扩展，提供针对长字符(含中文)的兼容
    /// </summary>
    public static class PlayerPrefsUtil
    {
        public static string GetLongString(string key, string defaultValue = null)
        {
            try
            {
                string[] bytesArray = PlayerPrefs.GetString(key).Split(' ');
                byte[] bytes = new byte[bytesArray.Length];
                for (int i = 0; i < bytesArray.Length; i++)
                {
                    bytes[i] = byte.Parse(bytesArray[i]);
                }
                string value = Encoding.UTF8.GetString(bytes);
                return value;
            }
            catch
            {
                return defaultValue;
            }
        }

        public static void SetLongString(string key, string value)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(value);
            StringBuilder sb = new StringBuilder();
            bool needAppedSpace = false;
            foreach (byte b in buffer)
            {
                if (needAppedSpace)
                    sb.Append(' ');
                sb.Append(b.ToString());
                needAppedSpace = true;
            }
            // 保存
            PlayerPrefs.SetString(key, sb.ToString());
            PlayerPrefs.Save();
        }
    }
}
