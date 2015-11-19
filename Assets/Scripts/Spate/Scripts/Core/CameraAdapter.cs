using UnityEngine;
using System.Collections;


namespace Spate
{
    [RequireComponent(typeof(Camera))]
    public sealed class CameraAdapter : MonoBehaviour
    {
        public float baseWidth = 1136f;
        public float baseHeight = 640f;

        private Camera mCamera;
        private float baseAspect;
        private float curAspect;
        private float oldAspect;

        private float scale;
        private float value;
        private Rect rect;

        void Start()
        {
            mCamera = camera;
            if (mCamera == null)
            {
                Destroy(this);
                return;
            }
            baseAspect = baseWidth / baseHeight;
            // mCamera.aspect = baseAspect;
            if (Settings.isMobilePlatform)
            {
                Update();
                Destroy(this);
                return;
            }
        }

        void Update()
        {
            curAspect = Screen.width * 1.0f / Screen.height;
            if (curAspect != oldAspect)
            {
                oldAspect = curAspect;
                if (curAspect > baseAspect)
                {
                    // 高度压缩
                    scale = Screen.height / baseHeight;//高度缩放比
                    value = scale * baseWidth;
                    rect = mCamera.rect;
                    rect.x = (Screen.width - value) * 0.5f / Screen.width;
                    rect.y = 0;
                    rect.width = value / Screen.width;
                    rect.height = 1f;
                    mCamera.rect = rect;
                }
                else if (curAspect < baseAspect)
                {
                    // 宽度压缩
                    scale = Screen.width / baseWidth;//宽度缩放比
                    value = scale * baseHeight;
                    rect = mCamera.rect;
                    rect.x = 0;
                    rect.y = (Screen.height - value) * 0.5f / Screen.height;
                    rect.width = 1f;
                    rect.height = value / Screen.height;
                    mCamera.rect = rect;
                }
            }
        }

        void OnDestroy()
        {
            WindowManager.ClearGhost();
        }
    }
}
