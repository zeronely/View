﻿using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// 数据捆,Dictionary的序列化实现
/// </summary>
[Serializable]
public sealed class DataBundle
{
    [SerializeField]
    private List<Entity> _list;
    [SerializeField]
    private string _type;

    public DataBundle() :
        this(10)
    {
    }

    public DataBundle(System.Type t)
        : this(10, t)
    {
    }

    public DataBundle(int capacity)
        : this(capacity, typeof(UnityEngine.Object))
    {
    }

    public DataBundle(int capacity, System.Type t)
    {
        if (capacity < 0) throw new ArgumentException("capacity < 0");
        if (t == null) throw new System.ArgumentException("t is null");
        _list = new List<Entity>(capacity);
        _type = t.AssemblyQualifiedName;
    }

    public int Count
    {
        get { return _list.Count; }
    }

    public Type RealType
    {
        get { return Type.GetType(_type); }
    }

    /// <summary>
    /// 检查是否存在指定的键
    /// </summary>
    public bool ContainsKey(string name)
    {
        bool ret = false;
        for (int i = 0, len = _list.Count; i < len; i++)
        {
            if (_list[i] != null && string.Equals(_list[i].key, name))
            {
                ret = true;
                break;
            }
        }
        return ret;
    }

    /// <summary>
    /// 增加keyvalue对，如果key存在就覆盖
    /// </summary>
    public void Put(string key, UnityEngine.Object value)
    {
        bool ret = false;
        for (int i = 0, len = _list.Count; i < len; i++)
        {
            if (_list[i] != null && string.Equals(_list[i].key, key))
            {
                ret = true;
                // 修改
                _list[i].value = value;
                break;
            }
        }
        if (!ret)
        {
            // 增加
            Entity entity = new Entity() { key = key, value = value };
            _list.Add(entity);
        }
    }

    /// <summary>
    /// 查找指定键的值,如果不存在返回null
    /// </summary>
    public UnityEngine.Object FindValue(string key)
    {
        UnityEngine.Object result = null;
        for (int i = 0, len = _list.Count; i < len; i++)
        {
            if (_list[i] != null && string.Equals(_list[i].key, key))
            {
                result = _list[i].value;
                break;
            }
        }
        return result;
    }

    /// <summary>
    /// 查找指定键的值,如果不存在返回null
    /// </summary>
    public T FindValue<T>(string key) where T : UnityEngine.Object
    {
        UnityEngine.Object result = FindValue(key);
        return result as T;
    }


    [System.Serializable]
    private class Entity
    {
        [SerializeField]
        public string key;
        [SerializeField]
        public UnityEngine.Object value;

        public override bool Equals(object obj)
        {
            Entity entity = obj as Entity;
            if (entity == null) return false;
            return object.Equals(key, entity.key) && UnityEngine.Object.Equals(value, entity.value);
        }

        public override int GetHashCode()
        {
            return key.GetHashCode() + value.GetHashCode();
        }
    }
}