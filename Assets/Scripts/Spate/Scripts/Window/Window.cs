using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spate
{
    public abstract class Window : BaseBehaviour
    {
        private View _View
        {
            get { return CachedGameObject.GetComponent<View>(); }
        }

        public eWindowLayer _Layer
        {
            get { return _View.Layer; }
        }

        /// <summary>
        /// 找自己的孩子
        /// </summary>
        public GameObject FindSelfChild(string name)
        {
            return _View.FindSelfChild(name);
        }

        /// <summary>
        /// 找自己的孩子的组件
        /// </summary>
        public T GetSelfComponent<T>(string name) where T : Component
        {
            return _View.GetSelfComponent<T>(name);
        }

        /// <summary>
        /// 找子孩子的孩子
        /// </summary>
        public GameObject FindChildViewChild(object alias, string name)
        {
            return _View.FindChildViewChild(alias, name);
        }

        /// <summary>
        /// 找子孩子的孩子的组件
        /// </summary>
        public T GetChildViewComponent<T>(object alias, string name) where T : Component
        {
            return _View.GetChildViewComponent<T>(alias, name);
        }

        /// <summary>
        /// 找子孩子的孩子的组件
        /// </summary>
        public T GetChildViewComponent<T>(string name) where T : Component
        {
            return _View.GetChildViewComponent<T>(name);
        }

        /// <summary>
        /// 找子孩子的孩子
        /// </summary>
        public GameObject FindChildViewChild(string name)
        {
            return _View.FindChildViewChild(name);
        }

        /// <summary>
        /// 找所有View中第一个名字为name的孩子(从自己开始找起)
        /// </summary>
        public GameObject FindChild(string name)
        {
            return _View.FindChild(name);
        }

        /// <summary>
        /// 设置子视图的别名
        /// </summary>
        public void SetChildViewAlias(object obj, View view)
        {
            _View.SetChildViewAlias(obj, view);
        }

        public View AddView(GameObject parentObj, GameObject child)
        {
            return _View.AddView(parentObj, child);
        }

        public View AddView(string parentName, GameObject child)
        {
            return _View.AddView(parentName, child);
        }

        public View AddView(string parentName, string path)
        {
            return _View.AddView(parentName, path);
        }

        public void RemoveChildView(object alias)
        {
            _View.RemoveChildView(alias);
        }

        public void RemoveChildView(View view)
        {
            _View.RemoveChildView(view);
        }

        public void SetWindowDepth(int sort)
        {
            OnDepthChanged(_View.Depth, sort);
            _View.Depth = sort;
        }

        public void SetAlias(object alias)
        {
            _View.Alias = alias;
        }

        protected object[] _Args
        {
            get;
            private set;
        }

        public bool IsOpen
        {
            get;
            private set;
        }

        public bool _IsFirstOpen
        {
            get;
            private set;
        }

        void Awake()
        {
            _IsFirstOpen = true;
        }

        /// <summary>
        /// 初始化控件
        /// </summary>
        public void Init(params object[] args)
        {
            _Args = args;
            OnCreate(_Args);
        }

        protected virtual void OnCreate(params object[] args)
        {

        }

        /// <summary>
        /// 播放动画之前 
        /// </summary>
        protected virtual void OnOpen(bool isReOpen, params object[] args)
        {

        }

        /// <summary>
        /// 播放动画后
        /// </summary>
        protected virtual void OnPlayAnimAfferOpen()
        {

        }

        /// <summary>
        /// 播放动画之前
        /// </summary>
        protected virtual void OnPlayAnimBefroeOpen()
        {

        }

        protected virtual void OnStart()
        {

        }

        void Start()
        {
            OnStart();
        }

        public void Open(bool isPlayforOpen, params object[] args)
        {
            _Args = args;
            OnOpen(IsOpen, _Args);
            _IsFirstOpen = false;
            IsOpen = true;
            CachedGameObject.SetActive(true);
            PlayOpenAnim(isPlayforOpen);
        }

        private void PlayOpenAnim(bool isPlayforOpen)
        {
            OnPlayAnimBefroeOpen();
            if (isPlayforOpen)
                PlayWindowAnim(_View.OpenAnimType, true, OnPlayAnimAfferOpen);
            else
                OnPlayAnimAfferOpen();
        }
        private void PlayCloseAnim()
        {
            Action act = () =>
            {
                IsOpen = false;
                CachedGameObject.SetActive(false);
                OnPlayAnimAffterClose();
            };
            PlayWindowAnim(_View.CloseAnimType, false, act);
        }

        protected virtual void OnClose()
        {

        }

        private void PlayWindowAnim(eWindowAnimType type, bool isOpen, Action callback = null)
        {
            switch (type)
            {
                case eWindowAnimType.None:
                    if (callback != null)
                        callback();
                    break;
                case eWindowAnimType.Scale:
                    TweenScale tweenscale = CachedGameObject.GetComponent<TweenScale>();
                    if (tweenscale == null)
                        tweenscale = CachedGameObject.AddComponent<TweenScale>();
                    if (isOpen)
                    {
                        tweenscale.from = Vector2.zero;
                        tweenscale.to = Vector2.one;
                    }
                    else
                    {
                        tweenscale.from = Vector2.one;
                        tweenscale.to = Vector2.zero;
                    }
                    tweenscale.duration = 0.4f;
                    tweenscale.style = UITweener.Style.Once;
                    tweenscale.onFinished.Clear();
                    tweenscale.onFinished.Add(new EventDelegate(() =>
                    {
                        if (callback != null)
                            callback();
                    }));
                    tweenscale.ResetToBeginning();
                    break;
                case eWindowAnimType.Alpha:
                    TweenAlpha tweenAlpha = CachedGameObject.GetComponent<TweenAlpha>();
                    if (tweenAlpha == null)
                        tweenAlpha = CachedGameObject.AddComponent<TweenAlpha>();
                    if (isOpen)
                    {
                        tweenAlpha.from = 0f;
                        tweenAlpha.to = 1f;
                    }
                    else
                    {
                        tweenAlpha.from = 1f;
                        tweenAlpha.to = 0f;
                    }
                    tweenAlpha.duration = 0.4f;
                    tweenAlpha.style = UITweener.Style.Once;
                    tweenAlpha.onFinished.Clear();
                    tweenAlpha.onFinished.Add(new EventDelegate(() =>
                    {
                        if (callback != null)
                            callback();
                    }));
                    tweenAlpha.ResetToBeginning();
                    break;
                case eWindowAnimType.UpToDown:
                case eWindowAnimType.DownToUp:
                case eWindowAnimType.LiftToRight:
                case eWindowAnimType.RightToLeft:
                    TweenPosition tweenPosition = CachedGameObject.GetComponent<TweenPosition>();
                    if (tweenPosition == null)
                        tweenPosition = CachedGameObject.AddComponent<TweenPosition>();
                    Vector2 formVt = Vector2.zero;
                    Vector2 toVt = Vector2.zero;
                    if (isOpen)
                    {
                        switch (type)
                        {
                            case eWindowAnimType.UpToDown:
                                formVt = new Vector2(0, 400);
                                toVt = Vector2.zero;
                                break;
                            case eWindowAnimType.DownToUp:
                                formVt = new Vector2(0, -400);
                                toVt = Vector2.zero;
                                break;
                            case eWindowAnimType.LiftToRight:
                                formVt = new Vector2(-600, 0);
                                toVt = Vector2.zero;
                                break;
                            case eWindowAnimType.RightToLeft:
                                formVt = new Vector2(600, 0);
                                toVt = Vector2.zero;
                                break;
                        }
                    }
                    else
                    {
                        switch (type)
                        {
                            case eWindowAnimType.UpToDown:
                                formVt = Vector2.zero;
                                toVt = new Vector2(0, -400);
                                break;
                            case eWindowAnimType.DownToUp:
                                formVt = Vector2.zero;
                                toVt = new Vector2(0, 400);
                                break;
                            case eWindowAnimType.LiftToRight:
                                formVt = Vector2.zero;
                                toVt = new Vector2(600, 0);
                                break;
                            case eWindowAnimType.RightToLeft:
                                formVt = Vector2.zero;
                                toVt = new Vector2(-600, 0);
                                break;
                        }
                    }
                    tweenPosition.from = formVt;
                    tweenPosition.to = toVt;
                    tweenPosition.duration = 0.4f;
                    tweenPosition.style = UITweener.Style.Once;
                    tweenPosition.onFinished.Clear();
                    tweenPosition.onFinished.Add(new EventDelegate(() =>
                    {
                        if (callback != null)
                            callback();
                    }));
                    tweenPosition.ResetToBeginning();
                    break;
            }
        }

        protected virtual void OnDepthChanged(int oldValue, int netValue)
        {

        }

        protected virtual void OnPlayAnimBeforeClose()
        {

        }

        protected virtual void OnPlayAnimAffterClose()
        {

        }

        protected virtual void OnDestroy()
        {
            IsOpen = false;
        }

        void Destroy()
        {
            OnDestroy();
        }

        protected virtual void OnUpdate()
        {

        }

        void Update()
        {
            OnUpdate();
        }

        public void DoClose(bool destroy = true)
        {
            OnClose();
            OnPlayAnimBeforeClose();
            PlayCloseAnim();
            //
        }

        public void ClickClose(GameObject go = null)
        {
            DoClose();
        }


    }
}

