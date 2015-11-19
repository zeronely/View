using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Spate
{
    public sealed class AssetDownloader
    {
        // 默认超时时间
        private const float TIMEOUT_TIME = 5.0f;

        public string Url { get; private set; }
        public object UserData { get; private set; }
        public string Hash { get; private set; }
        public AssetDownloaderState State { get; private set; }
        public byte[] Data { get; private set; }
        public string Error { get; private set; }

        public Action<int> OnProgress;

        public void DoWork(string url)
        {
            DoWork(url, null);
        }

        public void DoWork(string url, object userData)
        {
            DoWork(url, userData, null);
        }

        public void DoWork(string url, object userData, string hash)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException("url can not be null or empty.");
            Free();
            Url = url;
            UserData = userData;
            Hash = hash;
            State = AssetDownloaderState.Work;
            AsyncManager.StartCoroutine(DownloadCore());
        }

        private IEnumerator DownloadCore()
        {
            yield return 0;
            float progress = 0f;
            float freezeTime = 0f;//用来记录进度没有变化的时长
            WWW www = new WWW(Url);
            while (true)
            {
                yield return 1;// 每一帧进行判定，这样可以合理的判定时间
                if (www.isDone)
                {
                    if (www.HasError())
                        SetError(www.error);
                    else if (www.size == 0)
                        SetError("Url不存在");
                    else
                        SetSucess(www.bytes);
                    break;
                }
                else
                {
                    if (progress == www.progress)
                    {
                        if (progress != 1f)
                        {
                            freezeTime += Time.deltaTime;
                            if (freezeTime > TIMEOUT_TIME)
                            {
                                SetTimeout();
                                break;
                            }
                        }
                    }
                    else
                    {
                        freezeTime = 0f;
                        progress = www.progress;
                        if (OnProgress != null)
                            OnProgress(www.bytesDownloaded);
                    }
                }
            }
            www = null;
        }

        public void Free()
        {
            Url = null;
            UserData = null;
            Hash = null;
            Data = null;
            Error = null;
            State = AssetDownloaderState.Idle;
        }

        private void SetError(string error)
        {
            Error = error;
            State = AssetDownloaderState.Error;
        }

        private void SetSucess(byte[] raw)
        {
            Data = raw;
            // 尝试进行完整性校验
            if (!string.IsNullOrEmpty(Hash) && !string.Equals(Hasher.CalcHash(raw), Hash))
            {
                Data = null;
                State = AssetDownloaderState.LossData;
            }
            else
            {
                State = AssetDownloaderState.Succeed;
            }
        }

        private void SetTimeout()
        {
            State = AssetDownloaderState.Timeout;
        }
    }
}
