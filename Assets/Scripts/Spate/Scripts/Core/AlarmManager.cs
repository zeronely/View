using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spate
{
    public sealed class AlarmManager : BaseManager
    {
        private static DateTime mCurrent;
        /// <summary>
        /// 时间计数用(秒）
        /// </summary>
        private static long ServerTimeLong = 0;
        private float interval = 0f;
        /// <summary>
        /// 暂停时的服务器时间
        /// </summary>
        private long OnPauseTime = 0;
        /// <summary>
        /// 暂停时的游戏运行累计时
        /// </summary>
        //private long OnPauseGameTime = 0;

        //private int delaytime = 0;
        /// <summary>
        /// 延迟指定时间后需执行一次的回调
        /// </summary>
        private static List<TimeWork> list_actionDoOnce = new List<TimeWork>();
        /// <summary>
        /// 延迟指定时间后需循环执行的回调
        /// </summary>
        private static List<TimeWork> list_actionDoLoop = new List<TimeWork>();

        int RecoverTime = 0;
        int SecondCheck = 0;

        public override void OnReset()
        {
            base.OnReset();
            RecoverTime = 0;
            SecondCheck = 0;
        }

        public static void SetDateTime(string dateTime)
        {
            //mCurrent = StringUtil.TransStringToDateTimeSecond(dateTime);
            ServerTimeLong = (long)(mCurrent.Ticks * 0.0001 * 0.001);
        }

        public static void SetDateTime(long dateTime)
        {

        }

        public static DateTime GetCurrentDateTime()
        {
            DateTime currentServerTime = new DateTime(ServerTimeLong * 1000 * 10000, DateTimeKind.Utc);
            return currentServerTime;
        }

        public static bool IsToday(DateTime nowtime)
        {
            bool Istoday = false;
            if(nowtime.Day == GetCurrentDateTime().Day && (GetCurrentDateTime() - nowtime).TotalHours < 24)
            {
                Istoday = true;
            }
            return Istoday;
        }

        /// <summary>
        /// 添加一个delay秒后唤醒的闹钟
        /// </summary>
        public static void AddAlarm(int delay, Action callback)
        {
            if (callback != null)
            {
                bool IsSave = false;
                foreach (TimeWork item in list_actionDoOnce)
                {
                    if (item.callback == callback)
                    {
                        item.delay = delay;
                        IsSave = true;
                        break;
                    }
                }
                if (!IsSave)
                {
                    TimeWork timework = new TimeWork();
                    timework.callback = callback;
                    timework.delay = delay;
                    list_actionDoOnce.Add(timework);
                }
            }
        }

        /// <summary>
        /// 添加一个指定时间间隔唤醒的闹钟,callback返回true则表示继续循环,否则终止循环
        /// </summary>
        public static void AddReturnAlarm(int timeSpan, Func<bool> callback)
        {

        }

        /// <summary>
        /// 添加一个指定时间间隔唤醒的闹钟,loop=-1时表示永久循环
        /// </summary>
        public static void AddAlarm(int timeSpan, int loop, Action callback)
        {
            if (callback != null)
            {
                bool IsSave = false;
                foreach (TimeWork item in list_actionDoLoop)
                {
                    if(item.callback == callback)
                    {
                        item.delay = timeSpan;
                        item.delaytime = timeSpan;
                        item.loop = loop;
                        IsSave = true;
                        break;
                    }
                }
                if(!IsSave)
                {
                    TimeWork timework = new TimeWork();
                    timework.callback = callback;
                    timework.delay = timeSpan;
                    timework.loop = loop;
                    timework.delaytime = timeSpan;
                    list_actionDoLoop.Add(timework);
                }
            }
        }

        private void GetApReCoverTime()
        {
            //curRoleData = DataAPI.GetCurRoleData();
            //if (curRoleData != null && curRoleData.ap < GlobalUtil.ROLE_AP_MAX)
            //{
            //    if (!string.IsNullOrEmpty(curRoleData.lastRecoverTime))
            //    {
            //        RecoverTime = (int)(GlobalUtil.ROLE_AP_TIME - ((AlarmManager.GetCurrentDateTime() - StringUtil.TransStringToDateTimeSecond(curRoleData.lastRecoverTime)).TotalSeconds % GlobalUtil.ROLE_AP_TIME));
            //    }
            //    else
            //    {
            //        RecoverTime = 0;
            //    }
            //}
        }

        public override void OnUpdate()
        {
            if (ServerTimeLong > 0)
            {
                interval += Time.deltaTime;
                if (interval > 1)
                {
                    interval = 0;
                    ServerTimeLong += 1;
                    if ((GetCurrentDateTime().Hour == 12 || GetCurrentDateTime().Hour == 18) && GetCurrentDateTime().Minute == 0 && GetCurrentDateTime().Second < 2)
                    {
                        //KaifuActivityWindow kaifuactivitywindow = WindowManager.FindWindow<KaifuActivityWindow>();
                        //if(kaifuactivitywindow != null && kaifuactivitywindow.IsOpen)
                        //{
                        //    kaifuactivitywindow.InitGetAp();
                        //}
                    }
                    //执行一次的闹钟事件
                    for (int i = 0; i < list_actionDoOnce.Count; i++)
                    {
                        if (list_actionDoOnce[i].delay > 0)
                        {
                            list_actionDoOnce[i].delay -= 1;
                            if(list_actionDoOnce[i].delay == 0)
                            {
                                list_actionDoOnce[i].callback();
                            }
                        }
                    }
                    //执行多次的闹钟事件
                    for (int i = 0; i < list_actionDoLoop.Count; i++)
                    {
                        if(list_actionDoLoop[i].delay >0)
                        {
                            list_actionDoLoop[i].delay -= 1;
                            if(list_actionDoLoop[i].delay ==0)
                            {
                                list_actionDoLoop[i].callback();
                                if(list_actionDoLoop[i].loop >0)
                                {
                                    list_actionDoLoop[i].loop -= 1;
                                    list_actionDoLoop[i].delay = list_actionDoLoop[i].delaytime;
                                }
                                else if(list_actionDoLoop[i].loop == -1)
                                {
                                    list_actionDoLoop[i].delay = list_actionDoLoop[i].delaytime;
                                }
                            }
                        }
                    }
                    //if (SecondCheck >= 0)
                    //{
                    //    if (SecondCheck == 0) GetApReCoverTime();
                    //    SecondCheck += 1;
                    //    if (SecondCheck > GlobalUtil.ROLE_AP_TIME)
                    //    {
                    //        SecondCheck = 0;
                    //    }
                    //}
                    //if (RecoverTime > 0)
                    //{
                    //    RecoverTime -= 1;
                    //    if (RecoverTime == 0)
                    //    {
                    //        curRoleData.ap += 1;
                    //        GetApReCoverTime();
                    //        Notifier.Notify(GlobalUtil.ROLESVRDATAUPDATE);
                    //    }
                    //}
                }
            }
        }

        public override void OnFixedUpdate()
        {
            // 实现时钟滴答
            mCurrent.Add(TimeSpan.FromSeconds(Time.fixedDeltaTime));
        }

        public override void OnApplicationPause(bool pause)
        {
            if (!pause)
            {
                // 请求最新的时间
                ServerTimeLong = OnPauseTime + (long)Time.realtimeSinceStartup - OnPauseTime;
            }
            else
            {
                //保存暂停时的时间
                OnPauseTime = ServerTimeLong;
                //OnPauseGameTime = (long)Time.realtimeSinceStartup;
            }
        }

        private class TimeWork
        {
            public Action callback;
            public int delay;//间隔时间
            public int loop;//循环次数
            public int delaytime;//间隔时间克隆
        }
    }
}
