using System;
using System.IO;
using UnityEngine;

namespace Spate
{
    public static class Settings
    {
        // 当前是否为Debug模式
        public static readonly bool Debug = false;
        // 当前是否为手机平台
        public static readonly bool isMobilePlatform;

        // 存放自定义数据的根目录,例如mpq，日志等
        public static readonly string UNITY_FOLDER;
        // 存放更新过程中的临时文件
        public static readonly string UNITY_RAW_FOLDER;
        // 存放日志文件
        public static readonly string UNITY_LOG_FOLDER;
        // 当前平台的名称(win,android,ios)
        public static readonly string PLATFORM_NAME;

        // 本地CSV的目录
        public static readonly string CSV_FOLDER;
        // 是否显示调试信息
        public static bool ShowDebugInfo { get; set; }

        public static bool IsKipGuide { get; set; }

        static Settings()
        {
#if UNITY_EDITOR
            Debug = true;
#else
            Debug = UnityEngine.Debug.isDebugBuild;
#endif

            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                    isMobilePlatform = true;
                    PLATFORM_NAME = "android";
                    UNITY_FOLDER = UrlUtil.Combine(Application.persistentDataPath, "Unity");
                    break;
                case RuntimePlatform.IPhonePlayer:
                    isMobilePlatform = true;
                    PLATFORM_NAME = "ios";
                    UNITY_FOLDER = UrlUtil.Combine(Application.persistentDataPath, "Unity");
                    break;
                default:
                    isMobilePlatform = false;
                    PLATFORM_NAME = "win";
                    UNITY_FOLDER = UrlUtil.Combine(Application.dataPath, "../Unity");
                    CSV_FOLDER = Application.dataPath + "/../../../Files/CSV/";
                    break;
            }
            UNITY_FOLDER = UNITY_FOLDER.Replace("\\", "/");
            FileUtil.EnsureFolder(UNITY_FOLDER);
            UNITY_RAW_FOLDER = UrlUtil.Combine(UNITY_FOLDER, "raw");
            FileUtil.EnsureFolder(UNITY_RAW_FOLDER);
            UNITY_LOG_FOLDER = UrlUtil.Combine(UNITY_FOLDER, "log");
            FileUtil.EnsureFolder(UNITY_LOG_FOLDER);
        }
    }
}
