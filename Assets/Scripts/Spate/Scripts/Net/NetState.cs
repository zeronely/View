using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spate
{
    public enum NetState
    {
        // 开始发起网络连接
        Begine,
        // 进度变化
        Progress,
        // 网络传输出错
        Error,
        // 网络传输超时
        Timeout,
        // 网络传输成功
        Succeed
    }
}
