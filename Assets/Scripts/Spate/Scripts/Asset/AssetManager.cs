using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Spate
{
    /// <summary>
    /// 资源管理
    /// </summary>
    public sealed class AssetManager : BaseManager
    {
        // 请求队列
        private static Queue<AssetRequest> mRequestQueue = new Queue<AssetRequest>();
        // 缓存字典
        private static Dictionary<string, Asset> mCache = new Dictionary<string, Asset>(50);
        private static AssetLoader mLoader;



        public override void OnStart()
        {
            base.OnStart();
            mLoader = new AssetLoader();
        }

        public override void OnUpdate()
        {
            // 检查是否空闲
            switch (mLoader.State)
            {
                case AssetLoaderState.Idle:
                    {
                        AssetRequest req = NextRequest();
                        if (req != null)
                            mLoader.DoWork(req);
                    }
                    break;
                case AssetLoaderState.Done:
                    {
                        if (mLoader.IsSuccess)
                        {
                            AssetRequest req = mLoader.Request;
                            if (mLoader.mainAsset != null)
                            {
                                Asset asset = new Asset(req.isStatic, mLoader.mainAsset);
                                if (mCache.ContainsKey(req.key))
                                    mCache[req.key] = asset;
                                else
                                    mCache.Add(req.key, asset);
                            }
                            // 回调
                            req.Callback();
                        }
                        else
                        {
                            // 出错了
                            Logger.LogAsset(LogType.Error, mLoader.Error);
                        }
                        mLoader.Free();
                    }
                    break;
            }
        }

        public override void OnReset()
        {
            ClearCfg();
            Release(true);
        }

        public override void OnDestroy()
        {
            ClearCfg();
            Release(true);
        }

        /// <summary>
        /// 当前是否没有请求
        /// </summary>
        public static bool IsNoRequest()
        {
            return GetRequestNum() == 0;
        }
        /// <summary>
        /// 获取剩余还剩多少个请求
        /// </summary>
        public static int GetRequestNum()
        {
            return mRequestQueue.Count + (mLoader.State == AssetLoaderState.Idle ? 0 : 1);
        }
        public static void Load(string key)
        {
            Load(key, false);
        }
        public static void Load(string key, bool isStatic)
        {
            Load(key, isStatic, null, null);
        }
        public static void Load(string key, Func<string, byte[], byte[]> handle)
        {
            Load(key, false, handle, null);
        }
        public static void Load(string key, Func<string, byte[], byte[]> handle, Action<string> callback)
        {
            Load(key, false, handle, callback);
        }
        public static void Load(string key, bool isStatic, Func<string, byte[], byte[]> handle, Action<string> callback)
        {
            if (!IsLoadingOrLoaded(key))
            {
                AssetRequest req = new AssetRequest(key, isStatic, handle, callback);
                mRequestQueue.Enqueue(req);
            }
        }

        /// <summary>
        /// 加载所有的音效
        /// </summary>
        public static void LoadAllSoundEffects()
        {
            Object[] seArray = Resources.LoadAll("se");
            for (int i = 0; i != seArray.Length; i++)
            {
                string shortName = string.Concat("se/", seArray[i].name);
                mCache.Add(shortName, new Asset(true, seArray[i]));
            }
        }

        /// <summary>
        /// 检查指定资源是否正在Loading中
        /// </summary>
        private static bool IsLoadingOrLoaded(string key)
        {
            // 检测是否在缓存中
            if (mCache.ContainsKey(key))
                return true;
            // 检测是否在排队中
            Queue<AssetRequest>.Enumerator erator = mRequestQueue.GetEnumerator();
            while (erator.MoveNext())
            {
                if (string.Equals(erator.Current.key, key))
                {
                    return true;
                }
            }
            // 检测是否正在下载中
            if (mLoader != null && mLoader.Request != null && string.Equals(mLoader.Request.key, key))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 检查是否已被加载
        /// </summary>
        public static bool ExistsObject(string key)
        {
            return mCache.ContainsKey(key);
        }
        /// <summary>
        /// 从缓存中检索
        /// </summary>
        public static Object GetObject(string path, bool isStatic = false)
        {
            Asset asset = null;
            mCache.TryGetValue(path, out asset);
            if (asset == null)
            {
                string file = UrlUtil.Combine(Settings.UNITY_RAW_FOLDER, path);
                if (File.Exists(file))
                {
                    AssetBundle bundle = AssetBundle.CreateFromFile(file);
                    if (bundle != null)
                    {
                        asset = new Asset(false, bundle.mainAsset);
                        mCache[path] = asset;
                        bundle.Unload(false);
                    }
                }
            }
            return asset == null ? null : asset.Prefab;
        }
        /// <summary>
        /// 从缓存中检索
        /// </summary>
        public static T GetObject<T>(string path, bool isStatic = false) where T : Object
        {
            Object o = GetObject(path, isStatic);
            return o as T;
        }
        /// <summary>
        /// 释放全部缓存,force指示是否释放static的资源
        /// </summary>
        public static void Release(bool force, bool gc = false)
        {
            List<string> list = new List<string>(mCache.Count);
            foreach (KeyValuePair<string, Asset> pair in mCache)
            {
                if (force || !pair.Value.IsStatic)
                {
                    pair.Value.Unload();
                    list.Add(pair.Key);
                }
            }
            foreach (string key in list)
            {
                mCache.Remove(key);
            }
            if (gc) GCManager.GC();
        }
        /// <summary>
        /// 释放指定缓存
        /// </summary>
        public static void ReleaseObject(string key, bool gc = false)
        {
            // 释放指定项
            if (ExistsObject(key))
            {
                Asset asset = mCache[key];
                asset.Unload();
                mCache.Remove(key);
                if (gc) GCManager.GC();
            }
        }
        /// <summary>
        /// 获取下一个待执行的Request
        /// </summary>
        private static AssetRequest NextRequest()
        {
            if (mRequestQueue.Count == 0)
                return null;
            return mRequestQueue.Dequeue();
        }


        private static Dictionary<string, AssetCfg> mAssetCfgMap = new Dictionary<string, AssetCfg>();

        public static void AddCfg(AssetCfg cfg)
        {
            mAssetCfgMap.Add(cfg.key, cfg);
        }

        public static void SetCfg(IDictionary<string, AssetCfg> source)
        {
            ClearCfg();
            mAssetCfgMap = new Dictionary<string, AssetCfg>(source);
        }

        public static AssetCfg GetCfg(string key)
        {
            AssetCfg cfg = null;
            mAssetCfgMap.TryGetValue(key, out cfg);
            return cfg;
        }
        public static void ClearCfg()
        {
            mAssetCfgMap.Clear();
        }

        public static List<string> GetCategory(string category)
        {
            List<string> list = new List<string>();
            using (Dictionary<string, AssetCfg>.Enumerator erator = mAssetCfgMap.GetEnumerator())
            {
                while (erator.MoveNext())
                {
                    KeyValuePair<string, AssetCfg> pair = erator.Current;
                    if (pair.Key.StartsWith(category))
                    {
                        list.Add(pair.Key);
                    }
                }
            }
            return list;
        }
    }
}
