using System;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;

using UnityEngine;
using System.Collections;


namespace Spate
{
    /// <summary>
    /// 窗口管理,提供打开窗口，关闭窗口，窗口的层级、进退栈维护
    /// </summary>
    public sealed class WindowManager : BaseManager
    {
        // 所有已经打开过的窗口
        private static Dictionary<string, Window> mAllWindows = new Dictionary<string, Window>();
        // 已经打开的壁纸层窗口
        private static List<Window> mWallpaperWindows = new List<Window>();
        // 已经打开的普通UI层窗口
        private static List<Window> mNormalWindows = new List<Window>();
        // 已经打开的普通对话框(弹出框)窗口
        private static List<Window> mDialogWindows = new List<Window>();
        // 已经打开的系统警告窗口
        private static List<Window> mAlertWindows = new List<Window>();
        // 当前处于打开状态的窗口
        private static Dictionary<string, Window> mActiveWindows = new Dictionary<string, Window>();
        //
        public static Dictionary<string, int> listShouldOpenWindow = new Dictionary<string, int>();

        private const string WindowPath = "Prefabs/window/";
        private static UICamera mUICamera;
        private static GameObject mRoot;
        private static Transform mAlertRoot;
        private static Transform mDialogRoot;
        private static Transform mNormalRoot;
        private static Transform mWallpaperRoot;

        public override void OnAwake()
        {
            mRoot = null;
            GameObject[] guiArray = GameObject.FindGameObjectsWithTag("GUI");
            if (guiArray != null)
            {
                foreach (GameObject go in guiArray)
                {
                    if (mRoot == null)
                        mRoot = go;
                    else
                        go.SetActive(false);
                }
            }
            if (mRoot == null)
            {
                mRoot = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/window/GUI")) as GameObject;
                StringUtil.RemoveClone(mRoot);
            }
            // 获取组件
            GameObject.DontDestroyOnLoad(mRoot);
            mUICamera = mRoot.GetComponentInChildren<UICamera>();
            WindowManager.ChangeUICameraFlags(CameraClearFlags.Skybox);
        }

        public override void OnReset()
        {
            if (mRoot != null)
                GameObject.Destroy(mRoot);
            mRoot = null;
            mAllWindows.Clear();
            mAlertWindows.Clear();
            mDialogWindows.Clear();
            mNormalWindows.Clear();
            mWallpaperWindows.Clear();
            mActiveWindows.Clear();
        }

        /// <summary>
        /// 获取Camera
        /// </summary>
        public static Camera GetCamera()
        {
            if (mUICamera == null)
                return null;
            return mUICamera.cachedCamera;
        }
        /// <summary>
        /// 获取UICamera
        /// </summary>
        public static UICamera GetUICamera()
        {
            return mUICamera;
        }

        public static bool CheckRoot(GameObject gui)
        {
            return gui == mRoot;
        }

        public static void ChangeUICameraFlags(CameraClearFlags flags)
        {
            if (GetCamera().clearFlags == flags) return;
            GetCamera().clearFlags = flags;
        }
        // 清除UICamera的残影
        public static void ClearGhost()
        {
            AsyncManager.StartCoroutine(ClearGhostCore());
        }

        private static IEnumerator ClearGhostCore()
        {
            yield return 0;
            GetCamera().clearFlags = CameraClearFlags.Skybox;
            yield return 1;
            GetCamera().clearFlags = CameraClearFlags.Depth;
        }


        /// <summary>
        /// 打开窗口
        /// </summary>
        public static T OpenWindow<T>(params object[] args) where T : Window
        {
            Window window = OpenWindow(typeof(T).Name, args);
            return window as T;
        }
        /// <summary>
        /// 打开窗口
        /// </summary>
        public static Window OpenWindow(string name, params object[] args)
        {
            Window window = null;
            if (mActiveWindows.ContainsKey(name))
            {
                window = mActiveWindows[name];
                window.Open(true, args);
                window.CachedGameObject.SetActive(true);

                RefreshDepth(window);
            }
            else
            {
                // 检测是否被打开过
                if (mAllWindows.ContainsKey(name))
                    window = mAllWindows[name];
                else
                    window = CreateWindow(name);
                mActiveWindows.Add(name, window);//添加到激活列表
                // 显示可见,连续调用两次是解决预制件本身active为false的特殊情况
                window.Init(args);
                window.Open(true, args);
            }
            RefreshDepth(window);
            return window;
        }

        private static void RefreshDepth(Window window)
        {
            if (window == null)
                return;
            switch (window._Layer)
            {
                case eWindowLayer.Alert:
                    {
                        if (!mAlertWindows.Contains(window))
                        {
                            mAlertWindows.Add(window);
                        }
                        else
                        {
                            mAlertWindows.Remove(window);
                            mAlertWindows.Add(window);
                        }
                        for (int i = 0; i < mAlertWindows.Count; i++)
                        {
                            mAlertWindows[i].SetWindowDepth(-2000 + 20 * i);
                            mAlertWindows[i].transform.localPosition = new Vector3(0, 0, -8000 - 200 * i);
                        }
                    }
                    break;
                case eWindowLayer.Dialog:
                    {
                        if (!mDialogWindows.Contains(window))
                        {
                            mDialogWindows.Add(window);
                        }
                        else
                        {
                            mDialogWindows.Remove(window);
                            mDialogWindows.Add(window);
                        }
                        for (int i = 0; i < mDialogWindows.Count; i++)
                        {
                            //if (mDialogWindows[i].GetType() == typeof(GuideWindow))
                            //{
                            //    mDialogWindows[i].Depth = -2500;
                            //    mDialogWindows[i].transform.localPosition = new Vector3(0, 0, -2500);
                            //}
                            //else
                            //{
                            mDialogWindows[i].SetWindowDepth(-4500 + 20 * i);
                            mDialogWindows[i].transform.localPosition = new Vector3(0, 0, -2000 - 230 * i);
                            //}
                        }
                    }
                    break;
                case eWindowLayer.Normal:
                    {
                        if (!mNormalWindows.Contains(window))
                        {
                            mNormalWindows.Add(window);
                        }
                        else
                        {
                            mNormalWindows.Remove(window);
                            mNormalWindows.Add(window);
                        }
                        for (int i = 0; i < mNormalWindows.Count; i++)
                        {
                            mNormalWindows[i].SetWindowDepth(-7000 + 20 * i);
                            mNormalWindows[i].transform.localPosition = new Vector3(0, 0, 2000 - 200 * i);
                        }
                    }
                    break;
                case eWindowLayer.Wallpaper:
                    {
                        if (!mWallpaperWindows.Contains(window))
                        {
                            mWallpaperWindows.Add(window);
                        }
                        else
                        {
                            mWallpaperWindows.Remove(window);
                            mWallpaperWindows.Add(window);
                        }
                        for (int i = 0; i < mWallpaperWindows.Count; i++)
                        {
                            mWallpaperWindows[i].SetWindowDepth(-7000 + 20 * i);
                            mWallpaperWindows[i].transform.localPosition = new Vector3(0, 0, 5000 - 200 * i);
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// 关闭窗口
        /// </summary>
        public static void CloseWindow<T>(bool destroy = false) where T : Window
        {
            CloseWindow(typeof(T).Name, destroy);
        }
        /// <summary>
        /// 关闭窗口
        /// </summary>
        public static void CloseWindow(string name, bool destroy = false)
        {
            if (!mAllWindows.ContainsKey(name))
            {
                // Logger.LogError("不能关闭没有打开的窗口-" + name);
            }
            else
            {
                Window window = mAllWindows[name];
                window.DoClose();
                if (window != null && window.gameObject != null) window.gameObject.SetActive(false);
                mActiveWindows.Remove(name);
                // 从layer中移除
                switch (window._Layer)
                {
                    case eWindowLayer.Alert:
                        mAlertWindows.Remove(window);
                        break;
                    case eWindowLayer.Dialog:
                        mDialogWindows.Remove(window);
                        break;
                    case eWindowLayer.Normal:
                        mNormalWindows.Remove(window);
                        break;
                    case eWindowLayer.Wallpaper:
                        mWallpaperWindows.Remove(window);
                        break;
                }
                // destroy
                if (window != null && window.gameObject != null && destroy)
                {
                    mAllWindows.Remove(name);
                    if (window.CachedGameObject != null) GameObject.Destroy(window.CachedGameObject);
                }
            }
        }
        /// <summary>
        /// 查找窗口
        /// </summary>
        public static T FindWindow<T>() where T : Window
        {
            Window window = null;
            mAllWindows.TryGetValue(typeof(T).Name, out window);
            return window as T;
        }

        /// <summary>
        /// 跳转场景，关闭所有window
        /// </summary>
        public static void SceneLoadCloseWindows()
        {
            List<Window> list = new List<Window>();
            foreach (Window win in mActiveWindows.Values)
            {
                if (string.Equals(win.name, "LoadingWindow", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(win.name, "MarqueeshowWindow", StringComparison.OrdinalIgnoreCase))
                    continue;
                list.Add(win);
            }
            for (int i = 0; i < list.Count; i++)
            {
                list[i].DoClose();
            }
        }

        /// <summary>
        /// 将当前所有激活的窗口进行隐藏,进入战斗时需要调用
        /// </summary>
        public static void DisableAllActives()
        {
            foreach (Window win in mActiveWindows.Values)
            {
                if (string.Equals(win.name, "LoadingWindow", StringComparison.OrdinalIgnoreCase))
                    continue;
                win.Open(false);
            }
        }
        /// <summary>
        /// 将当前所有激活的窗口进行恢复显示,退出战斗时需要调用
        /// </summary>
        public static void RecoverAllActives()
        {
            foreach (string name in mActiveWindows.Keys)
            {
                if (string.Equals(name, "LoadingWindow", StringComparison.OrdinalIgnoreCase))
                    continue;
                OpenWindow(name);
            }
        }

        /// <summary>
        /// 清除新手引导不需要恢复的窗口
        /// </summary>
        public static void ClearGuideNoActives()
        {
            if (mActiveWindows.ContainsKey("StayendWindow"))
                mActiveWindows.Remove("StayendWindow");
            if (mActiveWindows.ContainsKey("DemontowerWindow"))
                mActiveWindows.Remove("DemontowerWindow");
            if (mActiveWindows.ContainsKey("BudokaiWindow"))
                mActiveWindows.Remove("BudokaiWindow");
            if (mActiveWindows.ContainsKey("DreamlandWindow"))
                mActiveWindows.Remove("DreamlandWindow");
        }

        public static bool NextGuideSubstep(GameObject go, bool isWait = false)
        {
            bool isNext = true;
            //if (!Settings.IsKipGuide && GuideWindow.Ins != null && go != null)
            //    isNext = GuideWindow.Ins.ClickBtnExcuteNext(go.name, isWait);
            return isNext;
        }

        /// <summary>
        /// 创建窗口对象
        /// </summary>
        private static Window CreateWindow(string name)
        {
            GameObject prefab = Resources.Load(WindowPath + name) as GameObject;
            if (prefab == null) return null;
            GameObject newWindow = GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity) as GameObject;
            StringUtil.RemoveClone(newWindow);
            Type t = TryFindClassType(name);
            Window newComp = newWindow.GetComponent(name) as Window;
            if (newComp == null)
                newComp = newWindow.AddComponent(t) as Window;
            newComp.CachedTransform.SetParent(FindParent(newComp._Layer));
            newComp.CachedTransform.localPosition = Vector3.zero;
            newComp.CachedTransform.localRotation = Quaternion.identity;
            newComp.CachedTransform.localScale = Vector3.one;

            mAllWindows.Add(name, newComp);
            return newComp;
        }

        /// <summary>
        /// 查找节点
        /// </summary>
        private static Transform FindParent(eWindowLayer layer)
        {
            Transform tf = null;
            switch (layer)
            {
                case eWindowLayer.Alert:
                    if (mAlertRoot == null)
                        mAlertRoot = mRoot.transform.FindChild("Anchor/Alert");
                    tf = mAlertRoot;
                    break;
                case eWindowLayer.Dialog:
                    if (mDialogRoot == null)
                        mDialogRoot = mRoot.transform.FindChild("Anchor/Dialog");
                    tf = mDialogRoot;
                    break;
                case eWindowLayer.Normal:
                    if (mNormalRoot == null)
                        mNormalRoot = mRoot.transform.FindChild("Anchor/Normal");
                    tf = mNormalRoot;
                    break;
                case eWindowLayer.Wallpaper:
                    if (mWallpaperRoot == null)
                        mWallpaperRoot = mRoot.transform.FindChild("Anchor/Wallpaper");
                    tf = mWallpaperRoot;
                    break;
            }
            return tf;
        }

        //屏幕锁屏及恢复
        public override void OnApplicationPause(bool pause)
        {
            //if (!pause && BattleManager.Inst == null)
            //{
            //    // 刷新指定的window，比如需要刷新时间的
            //    foreach (string name in mActiveWindows.Keys)
            //    {
            //        // item.Initialize(true);
            //        if (name == "TradingWindow"
            //            || name == "TaskWindow"
            //            || name == "RoleInfoWindow"
            //            || name == "RankinglistWindow"
            //                || name == "BudokaiWindow"
            //                || name == "BudokaileftWindow"
            //            || name == "DreamlandWindow"
            //            || name == "RecruitWindow")
            //        {
            //            OpenWindow(name);
            //        }
            //    }
            //}
            //if (WindowAlertExecutor.Ins.IsEnterRole)
            //{
            //    WindowAlertExecutor.Ins.intenval = 0;
            //    NetAPI.AppMarquee(NotifyTimeIsChange);
            //}
        }
        /// <summary>
        /// 通知本地时间已更新
        /// </summary>
        private void NotifyTimeIsChange()
        {
            Notifier.Notify(GlobalUtil.LOCAL_TIME_CHANGE);
        }

        /// <summary>
        /// 尝试根据类名推导出真实的Type
        /// </summary>
        private static Type TryFindClassType(string className)
        {
            Type t = null;
            // login -> LoginWindow
            // 1,首字母大写
            string inferName = className.ToUpper(0, 1);
            // 2,尾缀添加Csv
            if (!inferName.EndsWith("Window"))
                inferName = inferName + "Window";
            // 根据名称查找类型(考虑垮程序集查找的问题)
            t = Type.GetType(inferName, false, false);
            if (t == null)
            {
                string assemblyQualifiedName = string.Format("{0}, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", inferName);
                t = Type.GetType(assemblyQualifiedName);
            }
            if (t == null)
                throw new Exception(string.Format("推导出的类型{0}不存在", inferName));
            return t;
        }
    }
}