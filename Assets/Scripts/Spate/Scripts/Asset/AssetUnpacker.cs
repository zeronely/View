using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spate
{

    public class AssetUnpacker : BaseBehaviour
    {
        private const string KEY = "Supreme_Cfg_AssetUnpacked";

        private static AssetUnpacker _inst = null;

        private Action onFinish;
        private Action<string, float> onProgress;

        private bool mFinished;
        private string mSourceFolder;

        private uint mTotalSize;
        private uint mUnpackedSize;

        public static void Start(GameObject go, Action<string, float> onProgress, Action onFinish)
        {
            if (_inst == null)
            {
                _inst = go.AddComponent<AssetUnpacker>();
                _inst.onFinish = onFinish;
                _inst.onProgress = onProgress;
            }
        }

        void Awake()
        {
            if (_inst != null)
            {
                Finish();
                return;
            }
        }

        void Start()
        {
            // 仅手机平台可用
            if (!Settings.isMobilePlatform)
            {
                Finish();
                return;
            }
            // 如果已经Unpack过就不需要再Unpack
            if (PlayerPrefs.HasKey(KEY))
            {
                Finish();
                return;
            }
            // jar:file:///data/app/supreme.apk/!/assets
            //string pkgPath = Application.streamingAssetsPath;
            //pkgPath = pkgPath.Substring(11);
            //pkgPath = pkgPath.Substring(0, pkgPath.IndexOf('!') - 1);
            //AsyncManager.StartThread(Unpack, UnpackCallback, pkgPath);
            mTotalSize = mUnpackedSize = 0u;
            NotifyProgress(null);
            mSourceFolder = UrlUtil.Combine(Application.streamingAssetsPath, "raw");
            StartCoroutine(UnpackAsync());
        }

        void OnDestroy()
        {
            _inst = null;
            onFinish();
        }

        private void Finish()
        {
            PlayerPrefs.SetInt(KEY, 1);
            Destroy(this);
        }

        private IEnumerator UnpackAsync()
        {
            // 第一步，下载files,如果不存在就结束
            string url = UrlUtil.Combine(mSourceFolder, "files");
            WWW www = new WWW(url);
            yield return www;
            if (string.IsNullOrEmpty(www.error))
            {
                Encoding encoding = new UTF8Encoding(false);
                string filesText = encoding.GetString(www.bytes);
                www = null;
                yield return 1;
                // 获取待Unpack的所有项
                string[] allFiles = filesText.Replace("\r\n", "\n").Split('\n');
                List<UnpackItem> list = new List<UnpackItem>(allFiles.Length);
                if (allFiles != null && allFiles.Length > 0)
                {
                    for (int i = 0; i < allFiles.Length; i++)
                    {
                        string[] cells = allFiles[i].Split('|');
                        string shortName = cells[0];
                        uint size = Convert.ToUInt32(cells[2]);
                        list.Add(new UnpackItem(shortName, size));
                        mTotalSize += size;
                    }
                }
                if (list.Count > 0)
                {
                    // 逐个更新
                    for (int i = 0; i != list.Count; i++)
                    {
                        UnpackItem item = list[i];
                        yield return StartCoroutine(UnpackFile(item));
                        if ((i + 1) % 10 == 0)
                            System.GC.Collect();
                    }
                    // 保存files
                    File.WriteAllText(UrlUtil.Combine(Settings.UNITY_RAW_FOLDER, "files"), filesText);
                    System.GC.Collect();
                    yield return 1;
                }
            }
            www = null;
            Finish();
        }

        private IEnumerator UnpackFile(UnpackItem item)
        {
            string srcUrl = UrlUtil.Combine(mSourceFolder, item.shortName);
            string dstUrl = UrlUtil.Combine(Settings.UNITY_RAW_FOLDER, item.shortName);
            FileUtil.EnsureFileParent(dstUrl);

            WWW www = new WWW(srcUrl);
            yield return www;
            if (string.IsNullOrEmpty(www.error))
                File.WriteAllBytes(dstUrl, www.bytes);
            www = null;

            mUnpackedSize += item.size;
            NotifyProgress(item.shortName);
        }

        private struct UnpackItem
        {
            public string shortName;
            public uint size;

            public UnpackItem(string name, uint s)
            {
                shortName = name;
                size = s;
            }
        }

        private void NotifyProgress(string name)
        {
            if (onProgress != null)
            {
                float percent = 0f;
                if (mTotalSize > 0f)
                    percent = mUnpackedSize * 1.0f / mTotalSize;
                onProgress(name, percent);
            }
        }
    }
}