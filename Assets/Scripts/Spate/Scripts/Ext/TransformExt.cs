using System;
using System.Collections;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Spate
{
    public static class TransformExt
    {
        public static Transform SearchChild(this Transform tf, string name)
        {
            Transform result = null;
            for (int i = 0; i != tf.childCount; i++)
            {
                Transform _tf = tf.GetChild(i);
                if (string.Equals(name, _tf.name)) result = _tf;
                else result = _tf.SearchChild(name);
                if (result != null) break;
            }
            return result;
        }

        public static void SetIdentity(this Transform tf, Transform parent)
        {
            tf.SetParent(parent);
            tf.SetIdentity();
        }

        public static void SetIdentity(this Transform tf)
        {
            tf.localPosition = Vector3.zero;
            tf.localRotation = Quaternion.identity;
            tf.localScale = Vector3.one;
        }

        public static T FindChild<T>(this Transform tf, string name) where T : Component
        {
            Transform child = tf.FindChild(name);
            if (child == null) return null;
            return child.GetComponent<T>();
        }

        public static Transform FindChildIterative(this Transform tf, string name)
        {
            Transform result = null;
            Transform[] allTrans = tf.GetComponentsInChildren<Transform>();
            for (int i = 0; i != allTrans.Length; i++)
            {
                if (string.Equals(allTrans[i].name, name))
                {
                    result = allTrans[i];
                    break;
                }
            }
            return result;
        }

        public static T FindChildIterative<T>(this Transform tf, string name) where T : Component
        {
            Transform result = tf.FindChildIterative(name);
            if (result == null) return null;
            return result.GetComponent<T>();
        }
    }
}
