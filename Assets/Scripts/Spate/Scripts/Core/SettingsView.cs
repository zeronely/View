using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;

namespace Spate
{
    public sealed class SettingsView : MonoBehaviour
    {
        /// <summary>
        /// 显示Settings界面,等待界面关闭的回调
        /// </summary>
        public static void ShowView(GameObject go, Action callback)
        {
            if (Settings.Debug)
            {
                SettingsView inst = go.GetComponent<SettingsView>();
                if (inst == null)
                    inst = go.AddComponent<SettingsView>();
                inst.mCallback = callback;
            }
            else
            {
                // 外网正式服
                NetHost.SetHost("http://120.25.227.65:9000");
                ResHost.SetHost(null);
                ResHost.SetUseCsv(false);
                Settings.ShowDebugInfo = false;
                callback();
            }
        }

        private const string KEY_CONFIG_NET = "Settings_Config_Net";
        private const string KEY_CONFIG_RES = "Settings_Config_Res";
        private const string KEY_CONFIG_CSV = "Settings_Config_Csv";
        private const string KEY_CONFIG_SHOWDEBUG = "Settings_Config_ShowDebug";
        private const string KEY_CONFIG_SKIPGUIDE = "Settings_Config_SkipGuide";

        private Vector2 mScrollPosition;
        private Action mCallback;

        // NetHost
        private int mNetIndex = 0;
        private List<ConfigPair> mNetConfigs;//全部的网络配置选项
        private string mTmpNetKey = "";
        private string mTmpNetValue = "";
        // ResHost
        private int mResIndex = 0;
        private List<ConfigPair> mResConfigs;//全部的资源配置选项
        private string mTmpResKey = "";
        private string mTmpResValue = "";

        void Start()
        {
            List<ConfigPair> listUserDef = null;

            mNetConfigs = new List<ConfigPair>();
            mNetConfigs.Add(new ConfigPair("内网服", "http://192.168.1.100:9000", true));
            mNetConfigs.Add(new ConfigPair("外网服", "http://120.25.227.65:9000", true));
            // 从PlayerPrefs中读取用户自定义的数据
            if (ReadPrefs(KEY_CONFIG_NET, out mNetIndex, out listUserDef))
            {
                mNetConfigs.AddRange(listUserDef);
                if (mNetIndex >= mNetConfigs.Count)
                    mNetIndex = 0;
            }
            else
            {
                mNetIndex = 0;
            }
            SetNetIndex(mNetIndex, false);


            mResConfigs = new List<ConfigPair>();
            mResConfigs.Add(new ConfigPair("自动", true));
            mResConfigs.Add(new ConfigPair("内网", "http://192.168.1.150:8080/SupremeFileServer/", true));
            // 从PlayerPrefs中读取用户自定义的数据
            if (ReadPrefs(KEY_CONFIG_RES, out mResIndex, out listUserDef))
            {
                mResConfigs.AddRange(listUserDef);
                if (mResIndex >= mResConfigs.Count)
                    mResIndex = 0;
            }
            else
            {
                mResIndex = 0;
            }
            SetResIndex(mResIndex, false);

            // UseCsv 和 ShowDebug
            ResHost.SetUseCsv((PlayerPrefs.GetInt(KEY_CONFIG_CSV, 0) == 1));
            Settings.ShowDebugInfo = (PlayerPrefs.GetInt(KEY_CONFIG_SHOWDEBUG, 0) == 1);
            Settings.IsKipGuide = (PlayerPrefs.GetInt(KEY_CONFIG_SKIPGUIDE, 0) == 1);
        }

        /// <summary>
        /// 绘制出配置界面
        /// </summary>
        void OnGUI()
        {
            mScrollPosition = GUILayout.BeginScrollView(mScrollPosition);

            GUILayout.Space(20);
            GUILayout.BeginHorizontal();
            GUILayout.Space(30);
            GUILayout.BeginVertical();

            OnGUI_Net();
            GUILayout.Space(20);
            OnGUI_Res();
            GUILayout.Space(20);
            OnGUI_GameData();
            GUILayout.Space(20);
            OnGUI_Control();
            GUILayout.Space(20);
            OnGUI_DebugInfo();
            GUILayout.Space(20);
            OnGUI_GuideInfo();

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.EndScrollView();
        }

        private void OnGUI_Net()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("网络服务器:", GUILayout.Width(70));
            GUI.changed = false;
            mNetIndex = GUILayout.SelectionGrid(mNetIndex, GetAllNetKeys(), 6);
            if (GUI.changed)
                SetNetIndex(mNetIndex);
            GUILayout.EndHorizontal();

            // 显示"添加"功能
            GUILayout.BeginHorizontal();

            GUILayout.Label("备注:", GUILayout.Width(40));
            mTmpNetKey = GUILayout.TextField(mTmpNetKey, GUILayout.Width(100));
            GUILayout.Space(3);
            GUILayout.Label("地址:", GUILayout.Width(40));
            mTmpNetValue = GUILayout.TextField(mTmpNetValue, GUILayout.Width(300));

            if (GUILayout.Button("添加", GUILayout.Width(60)))
            {
                if (!string.IsNullOrEmpty(mTmpNetKey) && !string.IsNullOrEmpty(mTmpNetValue)
                    && GetNetCountByName(mTmpNetKey) == 0)
                {
                    // 添加
                    mNetConfigs.Add(new ConfigPair(mTmpNetKey, mTmpNetValue, false));
                    SetNetIndex(mNetConfigs.Count - 1);
                }
            }
            GUILayout.Space(5);
            if (CheckCurrentNetCanModify() && GUILayout.Button("修改", GUILayout.Width(60)))
            {
                if (!string.IsNullOrEmpty(mTmpNetKey) && !string.IsNullOrEmpty(mTmpNetValue)
                    && GetNetCountByName(mTmpNetKey) <= 1)
                {
                    // 修改
                    mNetConfigs[mNetIndex].SetValue(mTmpNetValue);
                    SetNetIndex(mNetIndex);
                }
            }
            GUILayout.Space(5);
            if (CheckCurrentNetCanModify() && GUILayout.Button("删除", GUILayout.Width(80)))
            {
                // 删除
                mNetConfigs.RemoveAt(mNetIndex);
                SetNetIndex(0);
            }

            GUILayout.EndHorizontal();
        }

        private void OnGUI_Res()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("资源服务器:", GUILayout.Width(70));
            GUI.changed = false;
            mResIndex = GUILayout.SelectionGrid(mResIndex, GetAllResKeys(), 6);
            if (GUI.changed)
                SetResIndex(mResIndex);
            GUILayout.EndHorizontal();

            // 显示"添加"功能
            GUILayout.BeginHorizontal();

            GUILayout.Label("备注:", GUILayout.Width(40));
            mTmpResKey = GUILayout.TextField(mTmpResKey, GUILayout.Width(100));
            GUILayout.Space(3);
            GUILayout.Label("地址:", GUILayout.Width(40));
            if (mTmpResValue == null)
                GUILayout.Space(200);
            else
                mTmpResValue = GUILayout.TextField(mTmpResValue, GUILayout.Width(300));

            if (GUILayout.Button("添加", GUILayout.Width(60)))
            {
                if (!string.IsNullOrEmpty(mTmpResKey) && !string.IsNullOrEmpty(mTmpResValue)
                    && GetResCountByName(mTmpResKey) == 0)
                {
                    // 添加
                    mResConfigs.Add(new ConfigPair(mTmpResKey, mTmpResValue, false));
                    SetResIndex(mResConfigs.Count - 1);
                }
            }
            GUILayout.Space(5);
            if (CheckCurrentResCanModify() && GUILayout.Button("修改", GUILayout.Width(60)))
            {
                if (!string.IsNullOrEmpty(mTmpResKey) && !string.IsNullOrEmpty(mTmpResValue)
                    && GetResCountByName(mTmpResKey) == 1)
                {
                    // 修改
                    mResConfigs[mResIndex].SetValue(mTmpResValue);
                    SetResIndex(mResIndex);
                }
            }
            GUILayout.Space(5);
            if (CheckCurrentResCanModify() && GUILayout.Button("删除", GUILayout.Width(80)))
            {
                // 删除
                mResConfigs.RemoveAt(mResIndex);
                SetResIndex(0);
            }


            GUILayout.EndHorizontal();
        }

        private void OnGUI_GameData()
        {
            // 只有在Editor环境下才能使用CSV
#if UNITY_EDITOR
            GUI.changed = false;
            ResHost.SetUseCsv(GUILayout.Toggle(ResHost.UseCsv, "使用CSV", GUILayout.Width(80)));
            if (GUI.changed)
            {
                PlayerPrefs.SetInt(KEY_CONFIG_CSV, ResHost.UseCsv ? 1 : 0);
                PlayerPrefs.Save();
            }
#endif
        }

        private void OnGUI_Control()
        {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("清理", GUILayout.Width(80), GUILayout.Height(80)))
            {
                // 清理自定义数据
                PlayerPrefs.DeleteKey(KEY_CONFIG_NET);
                PlayerPrefs.DeleteKey(KEY_CONFIG_RES);
                PlayerPrefs.Save();
                // 重新加载数据
                Start();
            }

            GUILayout.Space(10);

            if (GUILayout.Button("完成", GUILayout.Width(80), GUILayout.Height(80)))
            {
                Destroy(this);
                mCallback();
            }

            GUILayout.EndHorizontal();
        }

        private void OnGUI_DebugInfo()
        {
            GUILayout.BeginHorizontal();
            GUI.changed = false;
            Settings.ShowDebugInfo = GUILayout.Toggle(Settings.ShowDebugInfo, "显示调试信息", GUILayout.Width(80));
            if (GUI.changed)
            {
                PlayerPrefs.SetInt(KEY_CONFIG_SHOWDEBUG, Settings.ShowDebugInfo ? 1 : 0);
                PlayerPrefs.Save();
            }
            GUILayout.EndHorizontal();
        }

        private void OnGUI_GuideInfo()
        {
            GUILayout.BeginHorizontal();
            GUI.changed = false;
            Settings.IsKipGuide = GUILayout.Toggle(Settings.IsKipGuide, "跳过新手引导", GUILayout.Width(80));
            if (GUI.changed)
            {
                PlayerPrefs.SetInt(KEY_CONFIG_SKIPGUIDE, Settings.IsKipGuide ? 1 : 0);
                PlayerPrefs.Save();
            }
            GUILayout.EndHorizontal();
        }

        private bool ReadPrefs(string key, out int selectedIndex, out List<ConfigPair> listConfig)
        {
            try
            {
                string prefs = PlayerPrefsUtil.GetLongString(key);
                int leftBracket = prefs.IndexOf('[');
                selectedIndex = int.Parse(prefs.Substring(0, leftBracket));
                string[] listPrefs = prefs.Substring(leftBracket + 1).Replace("]", "").Split(';');
                if (listPrefs.Length == 1 && string.IsNullOrEmpty(listPrefs[0]))
                {
                    listConfig = new List<ConfigPair>(0);
                }
                else
                {
                    listConfig = new List<ConfigPair>(listPrefs.Length);
                    for (int i = 0; i < listPrefs.Length; i++)
                    {
                        string[] itemArray = listPrefs[i].Split('=');
                        listConfig.Add(new ConfigPair(itemArray[0], itemArray[1], false));
                    }
                }
                return true;
            }
            catch
            {
                selectedIndex = 0;
                listConfig = null;
                return false;
            }
        }

        private string[] GetAllNetKeys()
        {
            string[] allKeys = new string[mNetConfigs.Count];
            for (int i = 0; i < allKeys.Length; i++)
            {
                allKeys[i] = mNetConfigs[i].Key;
            }
            return allKeys;
        }

        // 更新网路主机索引
        private void SetNetIndex(int index, bool needSave = true)
        {
            if (index >= mNetConfigs.Count)
                index = 0;
            mNetIndex = index;
            mTmpNetKey = mNetConfigs[index].Key;
            mTmpNetValue = mNetConfigs[index].Value;
            if (needSave)
                SaveNetPrefs();
            NetHost.SetHost(mTmpNetValue);
        }

        private bool CheckCurrentNetCanModify()
        {
            return !mNetConfigs[mNetIndex].IsFixed;
        }

        private int GetNetCountByName(string key)
        {
            int num = 0;
            for (int i = 0; i < mNetConfigs.Count; i++)
            {
                if (string.Equals(mNetConfigs[i].Key, key))
                {
                    num++;
                }
            }
            return num;
        }

        // 保存网络配置信息
        private void SaveNetPrefs()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(mNetIndex);
            sb.Append("[");
            for (int i = 0; i < mNetConfigs.Count; i++)
            {
                if (mNetConfigs[i].IsFixed)
                    continue;
                sb.AppendFormat("{0}={1}", mNetConfigs[i].Key, mNetConfigs[i].Value);
                if (i != (mNetConfigs.Count - 1))
                    sb.Append(";");
            }
            sb.Append("]");
            PlayerPrefsUtil.SetLongString(KEY_CONFIG_NET, sb.ToString());
        }




        private string[] GetAllResKeys()
        {
            string[] allKeys = new string[mResConfigs.Count];
            for (int i = 0; i < allKeys.Length; i++)
            {
                allKeys[i] = mResConfigs[i].Key;
            }
            return allKeys;
        }

        private void SetResIndex(int index, bool needSave = true)
        {
            if (index >= mResConfigs.Count)
                index = 0;
            mResIndex = index;
            mTmpResKey = mResConfigs[mResIndex].Key;
            mTmpResValue = mResConfigs[mResIndex].Value;
            if (needSave)
                SaveResPrefs();
            ResHost.SetHost(mTmpResValue);
        }

        private bool CheckCurrentResCanModify()
        {
            return !mResConfigs[mResIndex].IsFixed;
        }

        private int GetResCountByName(string key)
        {
            int num = 0;
            for (int i = 0; i < mResConfigs.Count; i++)
            {
                if (string.Equals(mResConfigs[i].Key, key))
                {
                    num++;
                    break;
                }
            }
            return num;
        }

        private void SaveResPrefs()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(mResIndex);
            sb.Append("[");
            for (int i = 0; i < mResConfigs.Count; i++)
            {
                if (mResConfigs[i].IsFixed)
                    continue;
                sb.AppendFormat("{0}={1}", mResConfigs[i].Key, mResConfigs[i].Value);
                if (i != (mResConfigs.Count - 1))
                    sb.Append(";");
            }
            sb.Append("]");
            PlayerPrefsUtil.SetLongString(KEY_CONFIG_RES, sb.ToString());
        }

        private class ConfigPair
        {
            public string Key { get; private set; }
            public string Value { get; private set; }
            public bool IsFixed { get; private set; }

            public ConfigPair(string key, bool isFixed)
                : this(key, null, isFixed)
            {
            }

            public ConfigPair(string key, string value, bool isFixed)
            {
                Key = key;
                Value = value;
                IsFixed = isFixed;
            }

            public void SetValue(string newValue)
            {
                Value = newValue;
            }
        }
    }
}