using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;


namespace Spate
{
    public delegate object ThreadHandler(object arg);
    public delegate void ThreadCallback(object arg);

    /// <summary>
    /// 异步工作者,提供协同+线程支持
    /// </summary>
    public sealed class AsyncManager : BaseManager
    {
        static AsyncManager()
        {
            // 考虑到日志线程常驻,所以设为2
            MAX_PROC = SystemInfo.processorCount;
            if (MAX_PROC < 2) MAX_PROC = 2;
        }


        #region 协同部分,考虑是否要加上协同并发控制
        /// <summary>
        /// 启动协同
        /// </summary>
        public static Coroutine StartCoroutine(IEnumerator routine)
        {
            if (Engine.Inst == null) return null;
            return Engine.Inst.StartCoroutine(routine);
        }
        /// <summary>
        /// 关闭协同
        /// </summary>
        public static void StopCoroutine(IEnumerator routine)
        {
            if (Engine.Inst != null)
                Engine.Inst.StopCoroutine(routine);
        }
        /// <summary>
        /// 关闭协同
        /// </summary>
        public static void StopCoroutine(Coroutine routine)
        {
            if (Engine.Inst != null)
                Engine.Inst.StopCoroutine(routine);
        }
        /// <summary>
        /// 关闭所有协同
        /// </summary>
        public static void StopAllCoroutines()
        {
            if (Engine.Inst != null)
                Engine.Inst.StopAllCoroutines();
        }
        #endregion


        #region 线程部分
        private static readonly int MAX_PROC;//最大允许并发数
        private static int cur_proc = 0;//当前的并发线程数
        private static LinkedList<ThreadTask> listTask = new LinkedList<ThreadTask>();

        public static void StartThread(ThreadHandler handler, object arg)
        {
            StartThread(handler, null, arg);
        }

        public static void StartThread(ThreadHandler handler, ThreadCallback callback, object arg)
        {
            // 考虑到对LinkedList的资源竞争,所以必须在主线程中调用
            if (!Engine.IsMainContext())
                throw new Exception("仅允许在主逻辑线程中调用此方法");
            if (handler == null)
                throw new ArgumentNullException("执行体不能为null");
            // 构建Job对象并进行排队,由于做了并发控制,所以必须要进行排队
            listTask.AddLast(new ThreadTask(handler, callback, arg));
        }

        private class ThreadTask
        {
            private Thread mThread;

            private ThreadHandler mHandler;
            private object mArg;

            private ThreadCallback mCallback;
            private object mOutArg;
            private Exception mOutExp;

            public ThreadTask(ThreadHandler handler, ThreadCallback callback, object arg)
            {
                mHandler = handler;
                mCallback = callback;
                mArg = arg;
                mOutArg = null;
                mOutExp = null;
                // 自行构建线程,但不开始
                mThread = new Thread(new ParameterizedThreadStart(ThreadStart));
            }

            // 任务是否就绪完毕
            public bool IsUnstarted()
            {
                return mThread.ThreadState == ThreadState.Unstarted;
            }

            // 任务是否执行完毕
            public bool IsDead()
            {
                return !mThread.IsAlive;
            }

            // 启动线程
            public void Start()
            {
                mThread.Start(this);
            }

            // 完成,引发回调,将变量赋值为null
            public void Done()
            {
                if (mCallback != null && mOutExp == null)
                {
                    mCallback(mOutArg);
                }
                if (mOutExp != null)
                {
                    // 想办法自动抛出去
                    Logger.LogException(mOutExp);
                }
                mThread = null;
                mHandler = null;
                mArg = null;
                mCallback = null;
                mOutArg = null;
                mOutExp = null;
            }

            // 为了能支持线程的方法有返回值,所以再嵌套一层调用
            private void ThreadStart(object job)
            {
                // 在线程体中再执行用户绑定的Handler,这样就可以获得Handler处理的结果
                ThreadTask task = job as ThreadTask;
                try
                {
                    task.mOutArg = task.mHandler(task.mArg);
                }
                catch (Exception ex)
                {
                    task.mOutExp = ex;
                }
            }
        }

        #endregion

        public override void OnUpdate()
        {
            // 尝试处理线程
            if (listTask.Count > 0)
            {
                LinkedListNode<ThreadTask> node = listTask.First;
                while (node != null && node.Value != null)
                {
                    ThreadTask task = node.Value;
                    if (task.IsUnstarted())
                    {
                        // 检测当前是否还有空余的并发
                        if (cur_proc < MAX_PROC)
                        {
                            cur_proc++;
                            task.Start();
                        }
                        node = node.Next;
                    }
                    else if (task.IsDead())
                    {
                        cur_proc--;
                        // 从jobList中拆掉
                        LinkedListNode<ThreadTask> nextNode = node.Next;
                        listTask.Remove(node);
                        node = nextNode;
                        // 完成任务
                        task.Done();
                    }
                    else
                    {
                        node = node.Next;
                    }
                }
            }
        }
    }
}
