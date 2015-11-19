using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Spate
{
    /*
    public sealed class WindowAlertExecutor : BaseManager
    {
        public static WindowAlertExecutor Ins;
        //跑马灯请求间隔
        private int marqueeTime = 60;
        public int intenval = 0;
        private float nexttime = 0f;
        public bool IsEnterRole = false;

        //网络或资源下载错误提示
        private bool isShowNetOrAssetError = false;
        public bool IsShowNetOrAssetError
        {
            get
            {
                return isShowNetOrAssetError;
            }
            set
            {
                isShowNetOrAssetError = value;
            }
        }
        //逻辑层错误弹窗
        private bool isShowLuoJiError = false;
        public bool IsShowLuoJiError
        {
            get
            {
                return isShowLuoJiError;
            }
            set
            {
                isShowLuoJiError = value;
            }
        }

        public override void OnAwake()
        {
            Ins = this;
            //NotifyWindow
            Notifier.Reg(NetState.Begine, OnBegineHandler);
            Notifier.Reg<string>(NetState.Error, OnNetErrorHandler);
            Notifier.Reg(NetState.Timeout, OnNetTimeOutHandler);
            Notifier.Reg(NetState.Succeed, OnSuccessHandler);
            Notifier.Reg<string, string>(AssetDownloadCode.Error, OnAssetErrorHandler);
            Notifier.Reg<string>(AssetDownloadCode.Timeout, OnAssetTimeOutHandler);
            Notifier.Reg<string>(AssetDownloadCode.LossData, OnAssetLossDataHandler);

            //PopupWindow
            Notifier.Reg<int>(GlobalUtil.SERVERRET, ShowServerErrorCode);
            Notifier.Reg<string>(GlobalUtil.LUOJIERROR, ShowLuoJiMessage);
            Notifier.Reg<string>(GlobalUtil.LUOJISUCCESS, ShowLuoJiMessage);
            Notifier.Reg(GlobalUtil.PLAY_WINDOW_ANIMATION, PlayWindowAnimation);
            Notifier.Reg(GlobalUtil.END_PLAY_WINDOW_ANIMATION, EndPlayWindowAnimation);
            Notifier.Reg(GlobalUtil.SURE_EQUIP_INHERIT_POP, SureEquipInherit);
            ///弹窗不需要回调的 
            Notifier.Reg(GlobalUtil.CDKEY_INPUT_ERROR, NoCallBack);
            Notifier.Reg(GlobalUtil.GOLD_NOT_ENOUGH, NoCallBack);
            Notifier.Reg(GlobalUtil.CONVERT_ERROR, NoCallBack);
            Notifier.Reg(GlobalUtil.GOOD_NO_ENOUGH, NoCallBack);
            Notifier.Reg(GlobalUtil.CHECK_EQUIP_ACTION, CheckEquipAction);

        }

        public override void OnReset()
        {
            isShowNetOrAssetError = false;
            isShowLuoJiError = false;
            IsEnterRole = false;
        }

        public override void OnDestroy()
        {
            //NotifyWindow
            Notifier.Unreg(NetState.Begine, OnBegineHandler);
            Notifier.Unreg<string>(NetState.Error, OnNetErrorHandler);
            Notifier.Unreg(NetState.Timeout, OnNetTimeOutHandler);
            Notifier.Unreg(NetState.Succeed, OnSuccessHandler);
            Notifier.Unreg<string, string>(AssetDownloadCode.Error, OnAssetErrorHandler);
            Notifier.Unreg<string>(AssetDownloadCode.Timeout, OnAssetTimeOutHandler);
            Notifier.Unreg<string>(AssetDownloadCode.LossData, OnAssetLossDataHandler);

            //PopupWindow
            Notifier.Unreg<int>(GlobalUtil.SERVERRET, ShowServerErrorCode);
            Notifier.Unreg<string>(GlobalUtil.LUOJIERROR, ShowLuoJiMessage);
            Notifier.Unreg<string>(GlobalUtil.LUOJISUCCESS, ShowLuoJiMessage);
            Notifier.Unreg(GlobalUtil.PLAY_WINDOW_ANIMATION, PlayWindowAnimation);
            Notifier.Unreg(GlobalUtil.END_PLAY_WINDOW_ANIMATION, EndPlayWindowAnimation);
            Notifier.Unreg(GlobalUtil.SURE_EQUIP_INHERIT_POP, SureEquipInherit);
            //弹窗不需要回调的 
            Notifier.Unreg(GlobalUtil.CDKEY_INPUT_ERROR, NoCallBack);
            Notifier.Unreg(GlobalUtil.GOLD_NOT_ENOUGH, NoCallBack);
            Notifier.Unreg(GlobalUtil.CONVERT_ERROR, NoCallBack);
            Notifier.Unreg(GlobalUtil.GOOD_NO_ENOUGH, NoCallBack);
            Notifier.Unreg(GlobalUtil.CHECK_EQUIP_ACTION, CheckEquipAction);

        }

        #region NotifyWindow
        private NotifyWindow OpenNotifyWindow()
        {
            NotifyWindow notifywindow = WindowManager.FindWindow<NotifyWindow>();
            if (notifywindow == null || !notifywindow.IsOpen)
            {
                notifywindow = WindowManager.OpenWindow<NotifyWindow>();
            }
            isShowNetOrAssetError = true;
            return notifywindow;
        }
        private bool OnBegineHandler(object key, params object[] args)
        {
            OpenNotifyWindow().OnBegineHandler(key, args);
            return false;
        }
        private bool OnNetErrorHandler(object key, string error)
        {
            OpenNotifyWindow().OnNetErrorHandler(key, error);
            return false;
        }
        private bool OnNetTimeOutHandler(object key, params object[] args)
        {
            OpenNotifyWindow().OnNetTimeOutHandler(key, args);
            return false;
        }
        private bool OnSuccessHandler(object key, params object[] args)
        {
            OpenNotifyWindow().OnSuccessHandler(key, args);
            return false;
        }
        private bool OnAssetErrorHandler(object key, string name, string error)
        {
            OpenNotifyWindow().OnAssetErrorHandler(key, name, error);
            return false;
        }
        private bool OnAssetTimeOutHandler(object key, string name)
        {
            OpenNotifyWindow().OnAssetTimeOutHandler(key, name);
            return false;
        }
        private bool OnAssetLossDataHandler(object key, string name)
        {
            OpenNotifyWindow().OnAssetLossDataHandler(key, name);
            return false;
        }
        #endregion

        #region PopupWindow
        private PopupWindow OpenPopupWindow()
        {
            PopupWindow popupwindow = WindowManager.FindWindow<PopupWindow>();
            if (popupwindow == null || !popupwindow.IsOpen)
            {
                popupwindow = WindowManager.OpenWindow<PopupWindow>();
            }
            isShowLuoJiError = true;
            if (isShowNetOrAssetError)
            {
                WindowManager.OpenWindow<NotifyWindow>();
            }
            return popupwindow;
        }
        private bool ShowServerErrorCode(object code, int ret)
        {
            OpenPopupWindow().ShowServerErrorCode(code, ret);
            return false;
        }
        private bool ShowLuoJiMessage(object code, string info)
        {
            OpenPopupWindow().ShowLuoJiMessage(code, info);
            return false;
        }
        private bool SureEquipInherit(object code, params object[] args)
        {
            OpenPopupWindow().SureEquipInherit(code, args);
            return false;
        }
        private bool NoCallBack(object code, params object[] args)
        {
            OpenPopupWindow().NoCallBack(code, args);
            return false;
        }
        private bool CheckEquipAction(object code, params object[] args)
        {
            OpenPopupWindow().CheckEquipAction(code, args);
            return false;
        }
        public void ShowPopupWindowBtnSure(string info, UIEventListener.VoidDelegate btn_sure, UIEventListener.VoidDelegate btn_close)
        {
            OpenPopupWindow().ShowPopupWindowBtnSure(info, btn_sure, btn_close);
        }

        public void ShowPopupWindowCanceSure(string info, UIEventListener.VoidDelegate btn_sure, UIEventListener.VoidDelegate btn_cancel)
        {
            OpenPopupWindow().ShowPopupWindowBtnCancelSure(info, btn_sure, btn_cancel);
        }
        #endregion

        #region 界面动画遮罩
        private bool PlayWindowAnimation(object code, params object[] args)
        {
            WindowManager.OpenWindow<UIParticleWindow>();
            return false;
        }
        private bool EndPlayWindowAnimation(object code, params object[] args)
        {
            WindowManager.CloseWindow<UIParticleWindow>();
            return false;
        }
        #endregion

        #region ToastWindow
        private ToastWindow OpenToastWindow()
        {
            ToastWindow toastwindow = WindowManager.FindWindow<ToastWindow>();
            if (toastwindow == null || !toastwindow.IsOpen)
            {
                toastwindow = WindowManager.OpenWindow<ToastWindow>();
            }
            return toastwindow;
        }
        public void Show(ToastHelp toast)
        {
            OpenToastWindow().Show(toast);
        }
        public void ShowToast(string text)
        {
            OpenToastWindow().ShowToast(text);
        }
        #endregion

        #region GetRewardWindow
        /// <summary>
        /// 显示获得的物品信息
        /// </summary>
        public void ShowGetReward()
        {
            List<BaseData> listdata = DataManager.GetOtherList();
            List<BaseData> listshowdata = new List<BaseData>();
            if (listdata != null && listdata.Count > 0)
            {
                List<BaseData> listmoney = new List<BaseData>();
                for (int i = 0; i < listdata.Count; i++)
                {
                    if (listdata[i] is RoleSvrData)
                    {
                        RoleSvrData rolesvrdata = listdata[i] as RoleSvrData;
                        //积分
                        if (rolesvrdata.score > 0)
                        {
                            GoodsSvrData goodssvrdata = new GoodsSvrData();
                            goodssvrdata.id = 10118 * 100;
                            goodssvrdata.goods = 10118;
                            goodssvrdata.num = rolesvrdata.score;
                            listmoney.Add(goodssvrdata);
                        }
                        //武魂
                        if (rolesvrdata.spirit > 0)
                        {
                            GoodsSvrData goodssvrdata = new GoodsSvrData();
                            goodssvrdata.id = 10119 * 100;
                            goodssvrdata.goods = 10119;
                            goodssvrdata.num = rolesvrdata.spirit;
                            listmoney.Add(goodssvrdata);
                        }
                        //点券
                        if (rolesvrdata.coupons > 0)
                        {
                            GoodsSvrData goodssvrdata = new GoodsSvrData();
                            goodssvrdata.id = 10123 * 100;
                            goodssvrdata.goods = 10123;
                            goodssvrdata.num = rolesvrdata.coupons;
                            listmoney.Add(goodssvrdata);
                        }
                        //声望
                        if (rolesvrdata.prestige > 0)
                        {
                            GoodsSvrData goodssvrdata = new GoodsSvrData();
                            goodssvrdata.id = 10116 * 100;
                            goodssvrdata.goods = 10116;
                            goodssvrdata.num = rolesvrdata.prestige;
                            listmoney.Add(goodssvrdata);
                        }
                        //经验
                        if (rolesvrdata.exp > 0)
                        {
                            GoodsSvrData goodssvrdata = new GoodsSvrData();
                            goodssvrdata.id = 10101 * 100;
                            goodssvrdata.goods = 10126;
                            goodssvrdata.num = rolesvrdata.exp;
                            listmoney.Add(goodssvrdata);
                        }
                        //金币
                        if (rolesvrdata.coin > 0)
                        {
                            GoodsSvrData goodssvrdata = new GoodsSvrData();
                            goodssvrdata.id = 10104 * 100;
                            goodssvrdata.goods = 10104;
                            goodssvrdata.num = rolesvrdata.coin;
                            listmoney.Add(goodssvrdata);
                        }
                        //元宝
                        if (rolesvrdata.gold > 0)
                        {
                            GoodsSvrData goodssvrdata = new GoodsSvrData();
                            goodssvrdata.id = 10103 * 100;
                            goodssvrdata.goods = 10103;
                            goodssvrdata.num = rolesvrdata.gold;
                            listmoney.Add(goodssvrdata);
                        }
                        //体力
                        if (rolesvrdata.ap > 0)
                        {
                            GoodsSvrData goodssvrdata = new GoodsSvrData();
                            goodssvrdata.id = 10127 * 100;
                            goodssvrdata.goods = 10127;
                            goodssvrdata.num = rolesvrdata.ap;
                            listmoney.Add(goodssvrdata);
                        }
                    }
                    else if ((listdata[i] is GoodsSvrData && (listdata[i] as GoodsSvrData).num > 0) || (listdata[i] is DiamondSvrData && (listdata[i] as DiamondSvrData).num > 0)
                        || (listdata[i] is TreasureSvrData && (listdata[i] as TreasureSvrData).num > 0)
                        || listdata[i] is PlayerSvrData || listdata[i] is EquipSvrData)
                    {
                        listshowdata.Add(listdata[i]);
                    }
                }
                for (int i = 0; i < listmoney.Count; i++)
                {
                    listshowdata.Insert(0, listmoney[i]);
                }
                GetRewardWindow getrewardwindow = WindowManager.FindWindow<GetRewardWindow>();
                if (getrewardwindow != null && getrewardwindow.IsOpen)
                {
                    getrewardwindow.ReOpen(listshowdata);
                }
                else
                {
                    WindowManager.OpenWindow<GetRewardWindow>(listshowdata);
                }
                SoundManager.GetSePlayer().Play("se_GetGoods");
            }
            else
            {
                Logger.Log("没有获得的物品信息");
            }
        }
        #endregion

        #region 打开多条件的窗口（比如盟会-需判断是否有盟会，有则打开GuildWindow,无则打开GuildleftWindow）
        /// <summary>
        /// 进入盟会系统 需判断是否有盟会，有则打开GuildWindow,无则打开GuildleftWindow
        /// </summary>
        public void OpenGuild()
        {
            if (DataAPI.IsHadGuild())
            {
                WindowManager.OpenWindow<GuildWindow>();
            }
            else
            {
                WindowManager.OpenWindow<GuildleftWindow>((int)eGuildWinType.AllList);
            }
        }
        public void ResetFriendListTime()
        {
            RelationListWindow relationlistwindow = WindowManager.FindWindow<RelationListWindow>();
            if (relationlistwindow != null)
            {
                relationlistwindow.ResetFriendList();
            }
        }
        #endregion

        public override void OnUpdate()
        {
            nexttime += Time.deltaTime;
            if (nexttime > 1)
            {
                nexttime = 0f;
                if (IsEnterRole)
                {
                    intenval += 1;
                    if (intenval >= marqueeTime && BattleLauncher.Inst == null)
                    {
                        GetMarquee();
                    }
                }
            }
        }

        public void GetMarquee()
        {
            intenval = 0;
            NetAPI.AppMarquee(MarqueeShow);
        }

        /// <summary>
        /// 跑马灯
        /// </summary>
        private void MarqueeShow()
        {
            List<MarqueeSvrData> listMarqueeSvrData = DataManager.GetList<MarqueeSvrData>();
            if (listMarqueeSvrData != null)
            {
                for (int i = 0; i < listMarqueeSvrData.Count; i++)
                {
                    if (listMarqueeSvrData[i] != null && !string.IsNullOrEmpty(listMarqueeSvrData[i].startTime) && !string.IsNullOrEmpty(listMarqueeSvrData[i].expireTime))
                    {
                        DateTime start = StringUtil.TransStringToDateTimeSecond(listMarqueeSvrData[i].startTime);
                        DateTime end = StringUtil.TransStringToDateTimeSecond(listMarqueeSvrData[i].expireTime);
                        if (AlarmManager.GetCurrentDateTime() >= start && AlarmManager.GetCurrentDateTime() < end)
                        {
                            WindowManager.OpenWindow<MarqueeshowWindow>();
                            break;
                        }
                    }
                }
            }
        }
    }
     */
}
