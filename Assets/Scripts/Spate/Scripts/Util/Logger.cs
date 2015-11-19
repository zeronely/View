using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Collections.Generic;

using UnityEngine;
using Object = UnityEngine.Object;

namespace Spate
{
    /// <summary>
    /// 文件日志辅助类
    /// </summary>
    public static class Logger
    {
        // NeedStop
        private static bool mNeedStop = false;
        // Writer
        private static StreamWriter mWriter = null;
        // 消息对象队列
        private static LocklessQueue<LogMessage> mMessageQueue = new LocklessQueue<LogMessage>();

        /// <summary>
        /// 初始化日志工具
        /// </summary>
        public static void Initialize()
        {
            mNeedStop = false;
            if (mWriter == null)
            {
                // string logPath = Path.Combine(Settings.UNITY_FOLDER, "engine.log");
                string logPath = Path.Combine(Settings.UNITY_LOG_FOLDER, string.Concat(Engine.GetNowString(), ".log"));
                //if (File.Exists(logPath))
                //{
                //    // 如果存在则判定是否已经达到1MB，如果达到1MB(1048576)则移动到spate.log.bak
                //    FileInfo info = new FileInfo(logPath);
                //    if (info.Length >= 1048576L)
                //    {
                //        string logBackPath = Path.Combine(Settings.UNITY_FOLDER, "engine.log.bak");
                //        if (File.Exists(logBackPath))
                //            File.Delete(logBackPath);
                //        File.Copy(logPath, logBackPath);
                //        File.Delete(logPath);
                //    }
                //}
                mWriter = new StreamWriter(logPath, false, Encoding.UTF8);
                mWriter.AutoFlush = true;
            }
            // 每次启动的时候多拉几行空格，便于阅读
            if (mWriter.BaseStream.Position > 0L)
            {
                for (int i = 0; i < 10; i++)
                {
                    mWriter.WriteLine();
                }
            }

            Application.RegisterLogCallbackThreaded(OnLogCallback);
            if (Settings.isMobilePlatform)
                BuglyAgent.RegisterLogCallback(OnLogCallback);
            // 开辟一个线程来执行LogMessage
            AsyncManager.StartThread(LogMessageHandler, (object arg) =>
            {
                // 取消监听
                Application.RegisterLogCallbackThreaded(null);
                if (Settings.isMobilePlatform)
                    BuglyAgent.UnregisterLogCallback(OnLogCallback);
                LogMessage.Clear();
            }, null);
        }

        /// <summary>
        /// 请求释放日志工具
        /// </summary>
        public static void Dispose()
        {
            mNeedStop = true;
        }

        /// <summary>
        /// 输出Engine模块的Debug信息
        /// </summary>
        public static void LogEngine(string message)
        {
            Log("Engine", LogType.Log, message);
        }

        /// <summary>
        /// 输出Engine模块的Debug信息
        /// </summary>
        public static void LogEngine(string format, params object[] args)
        {
            LogEngine(string.Format(format, args));
        }

        /// <summary>
        /// 输出Net模块的Debug信息
        /// </summary>
        public static void LogNet(LogType type, string message)
        {
            Log("Net", type, message);
        }

        /// <summary>
        /// 输出Net模块的Debug信息
        /// </summary>
        public static void LogNet(LogType type, string format, params object[] args)
        {
            LogNet(type, string.Format(format, args));
        }

        /// <summary>
        /// 输出Asset模块的Debug信息
        /// </summary>
        public static void LogAsset(LogType type, string message)
        {
            Log("Asset", type, message);
        }

        /// <summary>
        /// 输出Asset模块的Debug信息
        /// </summary>
        public static void LogAsset(LogType type, string format, params object[] args)
        {
            LogAsset(type, string.Format(format, args));
        }

        /// <summary>
        /// 输出Battle模块的日志
        /// </summary>
        public static void LogBattle(LogType type, string message)
        {
            Log("Battle", type, message);
        }

        /// <summary>
        /// 输出Battle模块的日志
        /// </summary>
        public static void LogBattle(LogType type, string format, params object[] args)
        {
            LogBattle(type, string.Format(format, args));
        }

        /// <summary>
        /// 输出Debug信息(统称为Game模块)
        /// </summary>
        public static void Log(string message)
        {
            Log("Game", LogType.Log, message);
        }

        /// <summary>
        /// 输出Debug信息(统称为Game模块)
        /// </summary>
        public static void Log(string format, params object[] args)
        {
            Log(string.Format(format, args));
        }

        public static void Log(string tag, LogType type, object message)
        {
            // 直接用Debug.Log会造成较大的CPU消耗,所以用线程来输出日志
            string stackTrace = null;
            if (message is Exception)
                stackTrace = StackTraceUtility.ExtractStringFromException(message);
            else
                stackTrace = StackTraceUtility.ExtractStackTrace();
            mMessageQueue.Enqueue(LogMessage.Obtain(tag, type, message, stackTrace));
        }

        /// <summary>
        /// 输出Warning信息(统称为Game模块)
        /// </summary>
        public static void LogWarning(string message)
        {
            Log("Game", LogType.Warning, message);
        }

        /// <summary>
        /// 输出Warning信息(统称为Game模块)
        /// </summary>
        public static void LogWarning(string format, params object[] args)
        {
            LogWarning(string.Format(format, args));
        }

        /// <summary>
        /// 输出Eroor信息(统称为Game模块)
        /// </summary>
        public static void LogError(string message)
        {
            Log("Game", LogType.Error, message);
        }

        /// <summary>
        /// 输出Eroor信息(统称为Game模块)
        /// </summary>
        public static void LogError(string format, params object[] args)
        {
            LogError(string.Format(format, args));
        }

        /// <summary>
        /// 输出Exception信息(统称为Game模块)
        /// </summary>
        public static void LogException(Exception ex)
        {
            Log("Game", LogType.Exception, ex);
        }



        // 统一将所有的Log汇成文档
        private static void OnLogCallback(string condition, string stackTrace, LogType type)
        {
            // TODO:有一种特殊红色错误日志(目前只发现的类型),
            // condition为空字符串,但是stackTrace不为空,在console面板上看不到日志内容,想办法处理这种日志
            //if (string.IsNullOrEmpty(condition) && !string.IsNullOrEmpty(stackTrace))
            //{
            //}
            // 写入到文件中
            mWriter.WriteLine(condition);
            if (type != LogType.Log && !string.IsNullOrEmpty(stackTrace))
                mWriter.WriteLine(stackTrace);
        }

        private static object LogMessageHandler(object arg)
        {
            while (true)
            {
                LogMessage message = null;
                mMessageQueue.TryDequeue(out message);
                if (message == null)
                {
                    if (mNeedStop) break;
                    Thread.Sleep(20);
                    continue;
                }
                // 利用Debug输出日志
                switch (message.Type)
                {
                    case LogType.Assert:
                    case LogType.Log:
                        {
                            string txt = string.Format("[{0}]{1}", message.Tag, message.Message);
                            CheckLog(ref txt);
                            if (Settings.Debug)
                            {
                                Debug.Log(txt);
                            }
                            else
                                OnLogCallback(txt, message.Trace, LogType.Log);
                        }
                        break;
                    case LogType.Warning:
                        {
                            string txt = string.Format("[{0}]{1}", message.Tag, message.Message);
                            CheckLog(ref txt);
                            Debug.LogWarning(txt);
                        }
                        break;
                    case LogType.Error:
                        {
                            string txt = string.Format("[{0}]{1}", message.Tag, message.Message);
                            CheckLog(ref txt);
                            Debug.LogError(txt);
                        }
                        break;
                    case LogType.Exception:
                        {
                            if (message.Message is Exception)
                            {
                                Debug.LogException((Exception)message.Message);
                            }
                            else
                            {
                                string txt = string.Format("[{0}]{1}", message.Tag, message.Message);
                                CheckLog(ref txt);
                                Debug.LogError(txt);
                            }
                        }
                        break;
                }
                message.Recycle();
            }
            // 线程中关闭流
            try
            {
                mWriter.Close();
            }
            catch { }
            mWriter = null;

            return null;
        }

        private static void CheckLog(ref string log)
        {
            if (log.Length >= 16360)//最大值16363 = 65535/4
                log = log.Substring(0, 16360) + "...";
        }

        private sealed class LogMessage
        {
            /// <summary>
            /// 标签
            /// </summary>
            public string Tag { get; private set; }
            /// <summary>
            /// 日志类型
            /// </summary>
            public LogType Type { get; private set; }
            /// <summary>
            /// 日志内容
            /// </summary>
            public object Message { get; private set; }
            /// <summary>
            /// StackTrace
            /// </summary>
            public string Trace { get; private set; }

            // 当前是否被使用
            private bool mIsUsed;

            private LogMessage()
            {
                mIsUsed = true;
            }

            /// <summary>
            /// 清理对象池池
            /// </summary>
            public static void Clear()
            {
                foreach (LogMessage msg in mPoolList)
                {
                    msg.Recycle();
                }
                mPoolList.Clear();
            }

            /// <summary>
            /// 获取一个消息对象,为了减少对象的开辟，使用对象池
            /// </summary>
            public static LogMessage Obtain(string tag, LogType type, object message, string stackTrace)
            {
                LogMessage logMessage = null;
                // 优先尝试从对象池中查找已Recycle过的对象
                foreach (LogMessage msg in mPoolList)
                {
                    if (!msg.mIsUsed)
                    {
                        logMessage = msg;
                        logMessage.mIsUsed = true;
                        break;
                    }
                }
                if (logMessage == null)
                {
                    // 构建新对象并加入对象池
                    logMessage = new LogMessage();
                    mPoolList.Add(logMessage);
                }
                // 赋值
                logMessage.Tag = tag;
                logMessage.Type = type;
                logMessage.Message = message;
                logMessage.Trace = stackTrace;
                return logMessage;
            }

            /// <summary>
            /// 释放,让对象可以循环使用
            /// </summary>
            public void Recycle()
            {
                mIsUsed = false;
                Tag = null;
                Message = null;
                Type = LogType.Log;
            }

            // 池
            private static List<LogMessage> mPoolList = new List<LogMessage>(10);
        }
    }
}
