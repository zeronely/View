using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Spate
{
    /// <summary>
    /// 资源加载请求
    /// </summary>
    public class AssetRequest
    {
        public readonly string key;
        public readonly bool isStatic;
        private readonly Func<string, byte[], byte[]> handle;
        private readonly Action<string> callback;

        public AssetRequest(string _key, bool _isStatic, Func<string, byte[], byte[]> _handle, Action<string> _callback)
        {
            key = _key;
            isStatic = _isStatic;
            handle = _handle;
            callback = _callback;
        }

        public byte[] Handle(byte[] raw)
        {
            if (handle != null)
                return handle(key, raw);
            return raw;
        }

        public void Callback()
        {
            if (callback != null)
                callback(key);
        }
    }
}
