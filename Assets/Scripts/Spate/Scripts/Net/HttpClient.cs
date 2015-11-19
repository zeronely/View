using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Spate
{
    public delegate void HttpClientHandler(object tag, NetState state, WWW www);

    public sealed class HttpClient
    {
        // 默认超时时间
        private const float TIMEOUT_TIME = 10.0f;

        public static void Get(object tag, string url, HttpClientHandler handler)
        {
            Get(tag, url, null, TIMEOUT_TIME, handler);
        }

        public static void Get(object tag, string url, Dictionary<string, string> headers, HttpClientHandler handler)
        {
            Get(tag, url, headers, TIMEOUT_TIME, handler);
        }

        public static void Get(object tag, string url, float timeoutTime, HttpClientHandler handler)
        {
            Get(tag, url, null, timeoutTime, handler);
        }

        public static void Get(object tag, string url, Dictionary<string, string> headers, float timeoutTime, HttpClientHandler handler)
        {
            new HttpClient(tag, url, null, headers, timeoutTime, handler);
        }

        public static void Post(object tag, string url, byte[] postData, HttpClientHandler handler)
        {
            Post(tag, url, postData, null, TIMEOUT_TIME, handler);
        }

        public static void Post(object tag, string url, byte[] postData, Dictionary<string, string> headers, HttpClientHandler handler)
        {
            Post(tag, url, postData, headers, TIMEOUT_TIME, handler);
        }

        public static void Post(object tag, string url, byte[] postData, float timeoutTime, HttpClientHandler handler)
        {
            Post(tag, url, postData, null, timeoutTime, handler);
        }

        public static void Post(object tag, string url, byte[] postData, Dictionary<string, string> headers, float timeoutTime, HttpClientHandler handler)
        {
            if (postData == null)
                throw new Exception("Post调度不合法,请设定postData,或许可以用Get替代");
            new HttpClient(tag, url, postData, headers, timeoutTime, handler);
        }

        private string mUrl;
        private byte[] mPostData;
        private float mTimeoutTime;
        private Dictionary<string, string> mHeaders;
        private object mTag;
        private HttpClientHandler mHandler;

        private WWW mWWW;

        private HttpClient() { }

        private HttpClient(object tag, string url, byte[] postData, Dictionary<string, string> headers, float timeoutTime, HttpClientHandler handler)
        {
            if (string.IsNullOrEmpty(url))
                throw new Exception("url不能为null或Empty");
            if (timeoutTime < 0f)
                throw new ArgumentException("timeoutTime不能为负数");
            mUrl = url;
            mPostData = postData;
            mHeaders = headers;
            mTimeoutTime = timeoutTime;
            mTag = tag;
            mHandler = handler;
            mWWW = null;
            Notify(NetState.Begine);
            // 启动协同进行Execute
            AsyncManager.StartCoroutine(ExecuteAndWaitResponse());
        }

        /// <summary>
        /// 通知Handler,1表示Start,2表示进度变化,3表示出错，4表示超时，5表示成功
        /// </summary>
        private void Notify(NetState state)
        {
            if (mHandler != null)
                mHandler(mTag, state, mWWW);
        }

        /// <summary>
        /// 执行并等待结果
        /// </summary>
        IEnumerator ExecuteAndWaitResponse()
        {
            yield return 0;
            float progress = 0f;
            float freezeTime = 0f;//用来记录进度没有变化的时长
            mWWW = new WWW(mUrl, mPostData, mHeaders);
            while (true)
            {
                yield return 1;// 每一帧进行判定，这样可以合理的判定时间
                if (mWWW.isDone)
                {
                    // 出错 || mWWW.bytes.Length != mWWW.bytesDownloaded
                    if (mWWW.HasError())
                        Notify(NetState.Error);
                    // 成功
                    else Notify(NetState.Succeed);
                    break;
                }
                else
                {
                    if (progress == mWWW.progress)
                    {
                        // 即使WWW.progrss =1后还有一小段时间用于内部处理AssetBundle才会isDone=true，所以不用计入超时
                        if (progress != 1f)
                        {
                            freezeTime += Time.deltaTime;//由于是获取Time.deltaTime,所以必须要yield return 1;
                            if (freezeTime > mTimeoutTime)
                            {
                                Notify(NetState.Timeout);
                                // 由于是超时,所以需要进行中断处理
                                // mWWW.Dispose();
                                break;
                            }
                        }
                    }
                    else
                    {
                        freezeTime = 0f;
                        progress = mWWW.progress;
                        // 汇报进度变化
                        Notify(NetState.Progress);
                    }
                }
            }
            // WWW结束
            mWWW = null;
            mUrl = null;
            mPostData = null;
            mHeaders = null;
            mTag = null;
            mHandler = null;
        }
    }
}