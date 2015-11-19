using UnityEngine;
using System.Collections;

namespace Spate
{
    public class BBS : BaseBehaviour
    {
        public Transform target;

        void Update()
        {
            if (target != null)
                CachedTransform.LookAt(target);
        }
    }
}
