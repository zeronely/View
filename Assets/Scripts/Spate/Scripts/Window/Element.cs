﻿using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using Spate;

public class Element : BaseBehaviour
{

    [Tooltip("扩展数据,Widget映射")]
    [HideInInspector]
    [SerializeField]
    private DataBundle _widgets = new DataBundle(typeof(GameObject));

    public Element()
    {

    }

    /// <summary>
    /// 从Widgets配置中查找指定键的GameObject
    /// </summary>
    public GameObject FindWidget(string key)
    {
        return _widgets.FindValue<GameObject>(key);
    }

    // 当前窗口的GameObject对象
    public GameObject _go;
    // 当前窗口的Transform对象
    private Transform _myTrans
    {
        get { return _go.transform; }
    }

    /// <summary>
    /// 由Window通过反射来调用，该方法用于开启Element的生命周期
    /// </summary>
    public void Initialize(params object[] args)
    {
        OnCreate(args);
    }

    /// <summary>
    /// 创建时回调,在这个回调中可以进行：
    /// <para>变量初始化</para>
    /// <para>设定视图路径</para>
    /// </summary>
    protected virtual void OnCreate(params object[] args)
    {
        if (!_go.activeInHierarchy)
        {
            _go.SetActive(true);
        }
    }

    protected void DoClose()
    {
        if(_go != null)
        {
            _go.SetActive(false);
        }
    }

    public virtual void ShowData(BaseData data)
    {

    }

    //实例化组件
    protected GameObject InstantChildElement(Transform parent, GameObject prefab, ref Element element)
    {
        GameObject go = null;
        if (parent != null && prefab != null)
        {
            go = GameObject.Instantiate(prefab) as GameObject;
            go.transform.parent = parent;
            go.transform.localRotation = new Quaternion(0, 0, 0, 0);
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = new Vector3(1, 1, 1);
            element = go.GetComponent(prefab.name) as Element;
            element._go = go;
            element.Initialize();
        }
        return go;
    }

    //实例化组件
    protected void InstantChildElement(Transform parent, GameObject prefab, int count, ref List<GameObject> listgo, ref List<Element> listElement, bool needsort = true,
        float cellHeight = 200f, float cellWidth = 200f, UIGrid.Arrangement arrangement = UIGrid.Arrangement.Horizontal, int maxperline = 9999)
    {
        if (parent != null && prefab != null && count != 0)
        {
            listgo = new List<GameObject>();
            listElement = new List<Element>();
            for (int i = 0; i < count; i++)
            {
                GameObject go = GameObject.Instantiate(prefab) as GameObject;
                go.transform.parent = parent;
                go.transform.localRotation = new Quaternion(0, 0, 0, 0);
                go.transform.localPosition = Vector3.zero;
                go.transform.localScale = new Vector3(1, 1, 1);
                go.name = prefab.name + i;
                listgo.Add(go);
                Element element = go.GetComponent(prefab.name) as Element;
                listElement.Add(element);
            }
            if (needsort)
            {
                UIGrid uiGrid = parent.GetComponent<UIGrid>();
                if (uiGrid == null)
                {
                    uiGrid = parent.gameObject.AddComponent<UIGrid>();
                    uiGrid.cellHeight = cellHeight;
                    uiGrid.cellWidth = cellWidth;
                    uiGrid.arrangement = arrangement;
                    uiGrid.maxPerLine = maxperline;
                }
                uiGrid.Reposition();
            }
        }
    }

    void OnEnable()
    {
        DoEnable();
    }

    void OnDisable()
    {
        DoDisable();
    }

    void OnDestroy()
    {
        DoDestroy();
    }

    void Update()
    {
        OnUpdate();
    }

    protected virtual void OnUpdate()
    {

    }

    protected virtual void DoEnable()
    {

    }

    protected virtual void DoDisable()
    {

    }

    /// <summary>
    /// 销毁窗口
    /// </summary>
    protected virtual void DoDestroy()
    {
        UnityEngine.Object.Destroy(_go);
        _go = null;
    }

    #region 视图辅助
    /// <summary>
    /// 自动查找,优先从Widgets中查找GameObject
    /// </summary>
    protected GameObject FindView(string keyOrPath)
    {
        // 先从缓存中查找
        GameObject go = FindViewFromWidgets(keyOrPath);
        // 再实时查找
        if (go == null) go = FindViewFromTree(keyOrPath);
        return go;
    }
    /// <summary>
    /// 自动查找组件,优先从Widgets中查找
    /// </summary>
    protected T FindView<T>(string keyOrPath, bool autoAddIfNotExists = false) where T : Component
    {
        GameObject go = FindView(keyOrPath);
        if (go == null) return null;
        T comp = go.GetComponent<T>();
        if (autoAddIfNotExists && comp == null) comp = go.AddComponent<T>();
        return comp;
    }
    /// <summary>
    /// 从Widgets配置中查找GameObject
    /// </summary>
    protected GameObject FindViewFromWidgets(string key)
    {
        return FindWidget(key);
    }
    /// <summary>
    /// 从Widgets配置中查找组件
    /// </summary>
    protected T FindViewFromWidgets<T>(string key, bool autoAddIfNotExists = false) where T : Component
    {
        GameObject go = FindViewFromWidgets(key);
        if (go == null) return null;
        T comp = go.GetComponent<T>();
        if (autoAddIfNotExists && comp == null) comp = go.AddComponent<T>();
        return comp;
    }
    /// <summary>
    /// 从当前窗口下按路径查找
    /// </summary>
    protected GameObject FindViewFromTree(string path)
    {
        Transform t = _myTrans.FindChild(path);
        if (t == null) return null;
        return t.gameObject;
    }
    /// <summary>
    /// 从当前窗口下按路径查找组件
    /// </summary>
    protected T FindViewFromTree<T>(string path, bool autoAddIfNotExists = false) where T : Component
    {
        GameObject go = FindViewFromTree(path);
        if (go == null) return null;
        T comp = go.GetComponent<T>();
        if (autoAddIfNotExists && comp == null) comp = go.AddComponent<T>();
        return comp;
    }
    #endregion
}
