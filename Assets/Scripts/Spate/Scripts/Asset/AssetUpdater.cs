using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spate;


namespace Spate
{
    /// <summary>
    /// 资源更新器
    /// </summary>
    public sealed class AssetUpdater : BaseBehaviour
    {
        private static AssetUpdater _inst = null;

        private Step mStep;
        private Step mPreStep;
        private Step mCurStep;
        private AssetDownloader mDownloader;

        private string mServerHash;
        private string mServerFiles;
        private uint mTotalUpdateSize;
        private uint mUpdatedSize;
        private Queue<string> mUpdateQueue;//待更新的队列


        private string mRetry;
        private string mCurrent;
        private bool mPause;

        private Action<string, float> onProgress;
        private Action onFinish;

        private Dictionary<string, AssetCfg> mLocalCfgMap = null;

        public static void Start(GameObject go, Action<string, float> _onProgress, Action _finish)
        {
            if (_inst == null)
            {
                _inst = go.AddComponent<AssetUpdater>();
                _inst.onProgress = _onProgress;
                _inst.onFinish = _finish;
            }
        }

        public static void Retry()
        {
            if (_inst != null)
            {
                _inst.mPause = false;
                _inst.mStep = _inst.mCurStep;
            }
        }

        void Awake()
        {
            if (_inst != null)
            {
                Logger.LogError("重复挂载AssetUpdater");
                Destroy(this);
                return;
            }
        }

        void Start()
        {
            mDownloader = new AssetDownloader();
            mPreStep = Step.None;
            mStep = Step.HashmapHash;

            // 读取本地的配置列表
            string localFiles = UrlUtil.Combine(Settings.UNITY_RAW_FOLDER, "files");
            if (File.Exists(localFiles))
            {
                string[] array = File.ReadAllLines(localFiles);
                mLocalCfgMap = new Dictionary<string, AssetCfg>(array.Length);
                for (int i = 0; i < array.Length; i++)
                {
                    string line = array[i];
                    if (string.IsNullOrEmpty(line))
                        continue;
                    string[] cells = line.Split('|');
                    if (cells == null || cells.Length == 0)
                        continue;
                    AssetCfg cfg = new AssetCfg(cells);
                    mLocalCfgMap.Add(cfg.key, cfg);
                }
            }
            else
            {
                mLocalCfgMap = new Dictionary<string, AssetCfg>();
            }
        }

        void OnDestroy()
        {
            _inst = null;
        }

        void Update()
        {
            switch (mStep)
            {
                case Step.HashmapHash:
                    {
                        // 获取服务器上的hashmaphash
                        mPreStep = mStep;
                        mStep = Step.Wait;
                        mCurStep = Step.HashmapHash;
                        mDownloader.DoWork(GetServerUrl("hash"));
                    }
                    break;
                case Step.Hashmap:
                    {
                        // 获取服务器上的hashmap
                        mPreStep = mStep;
                        mStep = Step.Wait;
                        mCurStep = Step.Hashmap;
                        mDownloader.DoWork(GetServerUrl("files"));
                    }
                    break;
                case Step.Update:
                    {
                        mCurStep = Step.Update;
                        // 从服务器上下载item
                        if (!mPause)
                        {
                            mPreStep = mStep;
                            mStep = Step.Wait;
                            mCurrent = null;
                            if (mRetry != null)
                            {
                                mCurrent = mRetry;
                                mRetry = null;
                            }
                            else
                                mCurrent = mUpdateQueue.Dequeue();
                            mDownloader.OnProgress = OnItemProgress;
                            mDownloader.DoWork(GetServerUrl(mCurrent), mCurrent);
                        }
                    }
                    break;
                case Step.Patch:
                    {
                        mPreStep = Step.None;
                        mStep = Step.None;
                        AsyncManager.StartThread(PatchHandleAsync, null);
                    }
                    break;
                case Step.Finish:
                    {
                        mStep = Step.None;
                        OnFinish();
                    }
                    break;
                case Step.Wait:
                    {
                        switch (mDownloader.State)
                        {
                            case AssetDownloaderState.Error:
                                OnError(mDownloader.Url, mDownloader.Error);
                                mDownloader.Free();
                                break;
                            case AssetDownloaderState.Timeout:
                                OnTimeout(mDownloader.Url);
                                mDownloader.Free();
                                break;
                            case AssetDownloaderState.LossData:
                                OnLossData(mDownloader.Url);
                                mDownloader.Free();
                                break;
                            case AssetDownloaderState.Succeed:
                                OnSucceed(mDownloader.Url, mDownloader.Data, mDownloader.UserData);
                                mDownloader.Free();
                                break;
                        }
                    }
                    break;
            }
        }

        // 如果没有需要更新的项则需要调用该方法
        private void SendLocalMapToAssetManager()
        {
            if (mLocalCfgMap == null)
                throw new Exception("仅在无更新的情况下才执行此操作");
            AssetManager.SetCfg(mLocalCfgMap);
        }

        private void OnItemProgress(int number)
        {
            float percent = (mUpdatedSize + number) * 1.0f / mTotalUpdateSize;
            onProgress(mCurrent, percent);
        }

        private void Finish()
        {
            mStep = Step.Finish;
        }

        private void OnFinish()
        {
            Destroy(this);
            onProgress(mCurrent, 1f);
            onFinish();
        }

        private string GetServerUrl(string key)
        {
            return UrlUtil.Combine(ResHost.HostUrl, key);
        }

        private string GetRawPath(string key, bool ensure)
        {
            string path = UrlUtil.Combine(Settings.UNITY_RAW_FOLDER, key);
            if (ensure)
                FileUtil.EnsureFileParent(path);
            return path;
        }

        private void OnError(string url, string error)
        {
            mRetry = url;
            mPause = true;
            mStep = mPreStep;
            Notifier.Notify<string, string>(AssetDownloadCode.Error, url, error);
        }

        private void OnTimeout(string url)
        {
            mRetry = url;
            mPause = true;
            mStep = mPreStep;
            Notifier.Notify<string>(AssetDownloadCode.Timeout, url);
        }

        private void OnLossData(string url)
        {
            mRetry = url;
            mPause = true;
            mStep = mPreStep;
            Notifier.Notify<string>(AssetDownloadCode.LossData, url);
        }

        private void OnSucceed(string url, byte[] raw, object userData)
        {
            switch (mPreStep)
            {
                case Step.HashmapHash:
                    {
                        // 和本地的HashmapHash进行对比
                        mServerHash = new UTF8Encoding(false).GetString(raw);
                        string local_hashmaphash = GetLocalHashmapHash();
                        if (string.Equals(local_hashmaphash, mServerHash))
                        {
                            SendLocalMapToAssetManager();
                            Finish();
                        }
                        else
                            mStep = Step.Hashmap;
                    }
                    break;
                case Step.Hashmap:
                    {
                        mServerFiles = new UTF8Encoding(false).GetString(raw);
                        string[] array = mServerFiles.Replace("\r\n", "\n").Split('\n');
                        mUpdateQueue = new Queue<string>(array.Length);// 筛选出待更新的项
                        mTotalUpdateSize = 0u;
                        for (int i = 0; i < array.Length; i++)
                        {
                            string line = array[i];
                            if (string.IsNullOrEmpty(line))
                                continue;
                            string[] cells = line.Split('|');
                            if (cells == null || cells.Length == 0)
                                continue;
                            string key = cells[0];
                            AssetCfg item = new AssetCfg(cells, GetLocalItemHash(key));
                            AssetManager.AddCfg(item);
                            if (item.needUpdate)
                            {
                                mUpdateQueue.Enqueue(key);
                                mTotalUpdateSize += item.size;
                            }
                        }
                        mUpdateQueue.TrimExcess();
                        // 选择Patch或更新
                        if (mUpdateQueue.Count == 0)
                        {
                            SendLocalMapToAssetManager();
                            mStep = Step.Patch;
                            onProgress(null, 1f);
                        }
                        else
                        {
                            mStep = Step.Update;
                            mUpdatedSize = 0u;
                            onProgress(null, 0f);
                        }
                    }
                    break;
                case Step.Update:
                    {
                        // 写入到Raw中
                        string key = userData.ToString();
                        string rawPath = GetRawPath(key, true);
                        File.WriteAllBytes(rawPath, raw);
                        // 检查队列
                        if (mUpdateQueue.Count > 0)
                            mStep = Step.Update;
                        else
                            mStep = Step.Patch;
                        // 更新进度
                        AssetCfg item = AssetManager.GetCfg(key);
                        mUpdatedSize += item.size;
                        float percent = (mUpdatedSize * 1.0f / mTotalUpdateSize);
                        onProgress(key, percent);
                    }
                    break;
            }
        }

        private string GetLocalHashmapHash()
        {
            // 获取本地的files
            string localFilesPath = UrlUtil.Combine(Settings.UNITY_RAW_FOLDER, "files");
            if (!File.Exists(localFilesPath))
                return string.Empty;
            return Hasher.CalcFileHash(localFilesPath);
        }

        private string GetLocalItemHash(string key)
        {
            // TODO:应该先读本地files
            //string cachePath = GetRawPath(key, false);
            //if (File.Exists(cachePath))
            //    return Hasher.CalcFileHash(cachePath);
            AssetCfg cfg = null;
            mLocalCfgMap.TryGetValue(key, out cfg);
            if (cfg == null)
                return string.Empty;
            return cfg.hash;
        }

        private object PatchHandleAsync(object args)
        {
            // 写files
            string path = UrlUtil.Combine(Settings.UNITY_RAW_FOLDER, "files");
            File.WriteAllBytes(path, new UTF8Encoding(false).GetBytes(mServerFiles));
            System.GC.Collect();
            // 完成Patch
            Finish();
            return args;
        }

        private enum Step
        {
            None,
            Wait,
            HashmapHash,
            Hashmap,
            Update,
            Patch,
            Finish
        }
    }
}