using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Spate
{
    /// <summary>
    /// 可序列化的字典
    /// </summary>
    [Serializable]
    public sealed class UnityDictionary
    {
        [SerializeField]
        private List<KeyValuePair> mItems;

        public UnityDictionary()
        {
            Init(0);
        }

        public UnityDictionary(int capacity)
        {
            Init(capacity);
        }

        private void Init(int capacity)
        {
            if (capacity < 0) throw new ArgumentOutOfRangeException("capacity");
            if (capacity == 0) capacity = 10;
            mItems = new List<KeyValuePair>(capacity);
        }


        public int Count
        {
            get { return mItems.Count; }
        }

        public int Capacity
        {
            get { return mItems.Capacity; }
        }

        public string[] Keys
        {
            get
            {
                String[] keyArray = new String[Count];
                for (int i = 0; i < keyArray.Length; i++)
                {
                    keyArray[i] = mItems[i].key;
                }
                return keyArray;
            }
        }

        public bool ContainsKey(string key)
        {
            if (key == null)
                throw new ArgumentNullException("key");
            KeyValuePair pair = FindKeyValuePair(key);
            return pair != null;
        }

        public void Add(string key, Object value)
        {
            if (ContainsKey(key))
                throw new ArgumentException("An element with the same key already exists in the dictionary.");
            mItems.Add(new KeyValuePair(key, value));
        }

        public Object Get(string key)
        {
            if (key == null)
                throw new ArgumentNullException("key");
            KeyValuePair pair = FindKeyValuePair(key);
            if (pair == null)
                return null;
            return pair.value;
        }

        public GameObject GetGameObject(string key)
        {
            return Get<GameObject>(key);
        }

        public T Get<T>(string key) where T : Object
        {
            Object o = Get(key);
            return o as T;
        }

        public bool Remove(string key)
        {
            if (key == null)
                throw new ArgumentNullException("key");
            int index = -1;
            FindKeyValuePair(key, ref index);
            if (index == -1) return false;
            mItems.RemoveAt(index);
            return true;
        }

        public void Clear()
        {
            for (int i = 0, len = Count; i < len; i++)
            {
                mItems[i].Dispose();
            }
            mItems.Clear();
            mItems.TrimExcess();
        }

        private KeyValuePair FindKeyValuePair(string key)
        {
            KeyValuePair result = null;
            for (int i = 0, len = mItems.Count; i < len; i++)
            {
                if (string.Equals(mItems[i].key, key))
                {
                    result = mItems[i];
                    break;
                }
            }
            return result;
        }

        private KeyValuePair FindKeyValuePair(string key, ref int index)
        {
            KeyValuePair result = null;
            for (int i = 0, len = mItems.Count; i < len; i++)
            {
                if (string.Equals(mItems[i].key, key))
                {
                    index = i;
                    result = mItems[i];
                    break;
                }
            }
            return result;
        }

        [Serializable]
        private class KeyValuePair : IDisposable
        {
            [SerializeField]
            public string key;
            [SerializeField]
            public Object value;

            public KeyValuePair(string key, Object value)
            {
                this.key = key;
                this.value = value;
            }

            public override bool Equals(object obj)
            {
                KeyValuePair pair = obj as KeyValuePair;
                if (pair == null) return false;
                return string.Equals(key, pair.key) && object.Equals(value, pair.value);
            }

            public override int GetHashCode()
            {
                int hash = key.GetHashCode() + (value == null ? 0 : value.GetHashCode());
                return hash;
            }

            public override string ToString()
            {
                return string.Concat(key, "@", value == null ? "Null" : value.ToString());
            }

            public void Dispose()
            {
                key = null;
                value = null;
            }
        }
    }
}
