using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using System.Threading;
using MiniJson;

namespace Spate
{
    /// <summary>
    /// 网络管理,提供最基本的网络消息发送和接收，维护网络环境
    /// </summary>
    public class NetManager : BaseManager
    {
        private static string SessionID;

        // 记录上一个请求，用于做Retry功能,仅限前台请求
        private static HttpRequest mFrontRequest;

        public override void OnStart()
        {
            OnReset();
        }

        public override void OnReset()
        {
            SessionID = "0";
            mFrontRequest = null;
            NetCounter.Reset();
            NetAPI.Reset();
        }

        public override void OnDestroy()
        {
            SessionID = null;
            mFrontRequest = null;
            NetCounter.Reset();
        }

        /// <summary>
        /// 发送前台消息，不支持并发
        /// </summary>
        public static bool Send(Action<int, Dictionary<string, object>> callback,
            string main, string sub, string cmd)
        {
            DataManager.ClearUpdateCacheData();
            if (mFrontRequest != null)
            {
                Logger.LogNet(LogType.Error, "上一条消息尚未处理完毕", mFrontRequest.ToString());
                return false;
            }
            mFrontRequest = new HttpRequest(SessionID, main, sub, cmd, callback, true);
            DoPost(mFrontRequest);
            return true;
        }

        // 前台消息重试,前提条件是之前有发送过消息
        public static void Retry()
        {
            if (mFrontRequest == null)
                throw new InvalidOperationException("非法调用Retry");
            //mFrontRequest.SetSequence();//递增一个Sequence
            DoPost(mFrontRequest);
        }

        /// <summary>
        /// 发送后台消息,后台消息采用静默方式执行
        /// </summary>
        public static void SendBackstage(Action<int, Dictionary<string, object>> callback,
            string main, string sub, string cmd)
        {
            DoPost(new HttpRequest(SessionID, main, sub, cmd, callback, false));
        }

        // 执行Get
        private static void DoPost(HttpRequest req)
        {
            Notifier.tempCodeDic.Clear();
            if (req.IsFront)
            {
                // 网络日志
                Logger.LogNet(LogType.Log, "发送:" + req.ToString());
                HttpClient.Post(req, req.ToUrl(), req.ToData(), OnPostCallback);
            }
            else
            {
                HttpClient.Post(req, req.ToUrl(), req.ToData(), OnPostCallbackForBackstage);
            }
        }

        private static void OnPostCallback(object tag, NetState state, WWW www)
        {
            switch (state)
            {
                case NetState.Begine:
                    {
                        // 通知显示连接中提示
                        Notifier.Notify(NetState.Begine);
                    }
                    break;
                case NetState.Error:
                    {
                        // 通知显示网络错误提示
                        Notifier.Notify<string>(NetState.Error, "网络异常");
                        Logger.LogNet(LogType.Error, "接收(Error):" + www.error);
                    }
                    break;
                case NetState.Timeout:
                    {
                        // 通知显示网络超时提示
                        Notifier.Notify(NetState.Timeout);
                        Logger.LogNet(LogType.Error, "接收(Timeout)");
                    }
                    break;
                case NetState.Succeed:
                    {
                        // 启动线程进行网络数据处理
                        AsyncManager.StartThread(TranslateAsync, TranslateAsyncEnd, new object[] { tag, www.bytes });
                    }
                    break;
            }
        }

        private static void OnPostCallbackForBackstage(object tag, NetState state, WWW www)
        {
            if (state == NetState.Succeed)
            {
                AsyncManager.StartThread(TranslateAsync, TranslateAsyncEnd, new object[] { tag, www.bytes });
            }
        }

        // 线程中处理服务器返回的Json数据
        private static object TranslateAsync(object arg)
        {
            object[] argArray = arg as object[];
            HttpRequest req = argArray[0] as HttpRequest;
            byte[] buffer = argArray[1] as byte[];
            // 解密解压缩
            Encrypter.EncodeXor(buffer);
            if (GZipUtil.IsGZip(buffer))
                buffer = GZipUtil.Uncompression(buffer);
            string orgText = Encoding.UTF8.GetString(buffer);
            // 网络日志
            Logger.LogNet(LogType.Log, "接收:" + orgText);
            // Json化
            req.respData = Json.Deserialize(orgText) as Dictionary<string, object>;
            if (req.respData != null)
            {
                Dictionary<string, object> data = req.respData;
                if (data.ContainsKey("sid"))
                {
                    SessionID = data["sid"].ToString();
                    data.Remove("sid");
                }
                if (data.ContainsKey("ret"))
                {
                    req.ret = int.Parse(data["ret"].ToString());
                    data.Remove("ret");
                }
                if (data.ContainsKey("serverTime"))
                {
                    AlarmManager.SetDateTime(data["serverTime"].ToString());
                }
                if (data.ContainsKey("fids"))
                {
                    GlobalUtil.AddSuccesFriend = data["fids"].ToString();
                }
                // 填充至DataManager中去
                if (DataManager.UpdatesCache != null)
                    DataManager.UpdatesCache.Clear();
                if (DataManager.DeletesCache.Count > 0)
                    DataManager.DeletesCache.Clear();
                if (DataManager.AddsCache.Count > 0)
                    DataManager.AddsCache.Clear();
                DataManager.FillNetData(data);
            }
            // 将req返回给TranslateAsyncEnd
            return req;
        }

        // 主线程中回调
        private static void TranslateAsyncEnd(object arg)
        {
            HttpRequest req = arg as HttpRequest;
            if (req.IsFront)
            {
                // 通知关闭网络提示
                Notifier.Notify(NetState.Succeed);
                mFrontRequest = null;
            }
            // 通知数据被修改
            if (DataManager.UpdatesCache != null)
            {
                Dictionary<BaseData, CacheEntity> dic = new Dictionary<BaseData, CacheEntity>();
                for (int i = 0; i != DataManager.UpdatesCache.Count; i++)
                {
                    object[] arr = DataManager.UpdatesCache[i] as object[];
                    if (arr == null || arr.Length != 4)
                        continue;
                    BaseData data = (BaseData)arr[0];
                    string fieldName = arr[1].ToString();
                    object newValue = arr[2];
                    object oldValue = arr[3];
                    // 检测是否为同一个对象
                    CacheEntity entity = null;
                    if (!dic.TryGetValue(data, out entity))
                    {
                        entity = new CacheEntity();
                        dic.Add(data, entity);
                    }
                    entity.names.Add(fieldName);
                    entity.newValues.Add(newValue);
                    entity.oldValues.Add(oldValue);


                    data.OnDataUpdate(fieldName, newValue, oldValue);
                }
                // 遍历dic
                using (Dictionary<BaseData, CacheEntity>.Enumerator erator = dic.GetEnumerator())
                {
                    while (erator.MoveNext())
                    {
                        CacheEntity entity = erator.Current.Value;
                        erator.Current.Key.OnDataUpdate(entity.names, entity.newValues, entity.oldValues);
                    }
                }

                DataManager.UpdatesCache.Clear();
            }

            if (DataManager.DeletesCache.Count > 0)
            {
                foreach (BaseData data in DataManager.DeletesCache)
                {
                    data.OnDataDelete();
                }
                DataManager.DeletesCache.Clear();
            }

            if (DataManager.AddsCache.Count > 0)
            {
                foreach (BaseData data in DataManager.AddsCache)
                {
                    data.OnDataAdd();
                }
                DataManager.AddsCache.Clear();
            }
            // 回调
            req.Callback();
            req.Dispose();
        }


        private class CacheEntity
        {
            public List<string> names = new List<string>();
            public List<object> newValues = new List<object>();
            public List<object> oldValues = new List<object>();
        }
    }
}
