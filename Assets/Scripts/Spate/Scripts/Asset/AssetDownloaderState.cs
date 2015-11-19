using System;

namespace Spate
{
    public enum AssetDownloaderState
    {
        /// <summary>
        /// 待命
        /// </summary>
        Idle,
        /// <summary>
        /// 工作中
        /// </summary>
        Work,
        /// <summary>
        /// 成功
        /// </summary>
        Succeed,
        /// <summary>
        /// 出错
        /// </summary>
        Error,
        /// <summary>
        /// 超时
        /// </summary>
        Timeout,
        /// <summary>
        /// 丢包
        /// </summary>
        LossData,
    }
}
