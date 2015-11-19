using UnityEngine;

namespace Spate
{
    public class Asset
    {
        public Object Prefab { get; private set; }
        public bool IsStatic { get; private set; }

        public Asset(bool _isStatic, Object _asset)
        {
            IsStatic = _isStatic;
            Prefab = _asset;
        }

        public void Unload()
        {
            Prefab = null;
            IsStatic = false;
        }
    }
}
