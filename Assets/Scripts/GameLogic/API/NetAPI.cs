using System;
using System.Collections.Generic;

using Spate;

/// <summary>
/// 网络通讯的API
/// </summary>
public static class NetAPI
{
    public static bool CheckRet(int ret)
    {
        bool enable = ret == 0;
        //if (!enable)
        //{
        //    Notifier.Notify<int>(GlobalUtil.SERVERRET, ret);
        //}
        //ErrorCodeCsvData ErrorInfo = DataManager.Get<ErrorCodeCsvData>(ret);
        //if (ErrorInfo != null)
        //{
        //    enable = ret == 0 || (ret != 0 && ErrorInfo.type == 1);
        //}
        return enable;
    }

    /// <summary>
    /// 发送心跳包
    /// </summary>
    public static void HeartBeat()
    {
        NetManager.SendBackstage(null, "user", "heart", null);
    }

    //#region Version
    /// <summary>
    /// 获取指定渠道的最新版本
    /// </summary>
    /// <param name="platform">1表示android,2表示ios,3表示windows</param>
    /// <param name="channelID">表示在这个platform下的渠道ID</param>
    //public static void GetVersion(Action<AppinfoSvrData> callback, int channelID)
    //{
    //    IsSendStep = false;
    //    string cmd = string.Format("channelid={0}", channelID);
    //    cmd = WrapCmd(cmd);
    //    NetManager.Send((int ret, Dictionary<string, object> respData) =>
    //    {
    //        AppinfoSvrData data = DataManager.Get<AppinfoSvrData>();
    //        if (ResHost.FromLogicServer)
    //            ResHost.SetHost(data.resUrl);
    //        ResHost.SetVersion(data.resVersion);
    //        if (CheckRet(ret))
    //            callback(data);
    //    }, "app", "version", cmd);
    //}
    /// <summary>
    /// 跑马灯
    /// </summary>

    public static void Reset()
    {
        step = 0;
        subStep = 0;
    }

    public static string taskid;
    public static string progress;
    public static int step; //新手引导Id
    private static int cacheStep;
    public static int subStep;  //新手引导子步骤
    private static int cacheSubStep;
    private static bool IsSendStep = true;
}
