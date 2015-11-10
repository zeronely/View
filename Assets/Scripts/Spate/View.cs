using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class View : MonoBehaviour
{
    [SerializeField]
    [HideInInspector]
    public Eobject NeedObject = null;
    public Dictionary<object, View> childeViews;
    //
    private const string appointSuffix = "View";
    private const string appointPrefix = "#";

    Dictionary<string, UnityEngine.Object> needObj = new Dictionary<string, UnityEngine.Object>();
    public object Alias
    {
        set
        {
            if (Alias != value)
                Alias = value;
        }
        get { return Alias; }
    }
    void OnValidate()
    {
        FindNeedChildrens();
        SaveView();
    }

    void Awake()
    {
        Alias = this.GetInstanceID();
        childeViews = new Dictionary<object, View>();
    }

    void SaveView()
    {
        if (NeedObject == null)
            NeedObject = new Eobject();
        else
            NeedObject.Clear();
        using (Dictionary<string, UnityEngine.Object>.KeyCollection.Enumerator erator = needObj.Keys.GetEnumerator())
        {
            while (erator.MoveNext())
            {
                string key = erator.Current as string;
                UnityEngine.Object obj = needObj[key];
                NeedObject.Put(key, obj);
            }
        }
    }

    void FindNeedChildrens()
    {
        needObj.Clear();
        foreach (Transform tf in gameObject.transform)
        {
            string name = EcodAppointPrefix(tf.gameObject.name);
            if (tf.gameObject.name.StartsWith(appointPrefix))
            {
                if (!needObj.ContainsKey(name))
                    needObj.Add(name, tf.gameObject);
                else
                    NGUIDebug.Log(tf.gameObject.name + "is constant !");
            }
        }
    }

    bool IsParentNode(GameObject node)
    {
        string nodeName = node.name;
        return appointSuffix.Equals(nodeName.Substring((nodeName.Length - appointSuffix.Length), appointSuffix.Length));
    }
    string EcodAppointPrefix(string name)
    {
        return name.Substring(1, name.Length - 1);
    }

    /// <summary>
    /// 找自己的孩子
    /// </summary>
    public UnityEngine.Object FindSelfChild(string name)
    {
        if (NeedObject == null)
            return null;
        return NeedObject.FindValue(name);
    }

    /// <summary>
    /// 找自己的孩子的组件
    /// </summary>
    public T GetSelfComponent<T>(string name) where T : Component
    {
        GameObject obj = FindSelfChild(name) as GameObject;
        return obj.GetComponent<T>();
    }

    /// <summary>
    /// 找子孩子的孩子
    /// </summary>
    public UnityEngine.Object FindChildViewChild(object alias, string name)
    {
        if (childeViews == null || !childeViews.ContainsKey(alias))
            return null;
        return childeViews[alias].FindSelfChild(name);
    }

    /// <summary>
    /// 找子孩子的孩子的组件
    /// </summary>
    public T GetChildViewComponent<T>(object alias, string name) where T : Component
    {
        if (childeViews == null || !childeViews.ContainsKey(alias))
            return null;
        return childeViews[alias].GetSelfComponent<T>(name);
    }

    /// <summary>
    /// 找子孩子的孩子的组件
    /// </summary>
    public T GetChildViewComponent<T>(string name) where T : Component
    {
        if (childeViews == null)
            return null;
        T goalObj = null;
        using (Dictionary<object, View>.ValueCollection.Enumerator erator = childeViews.Values.GetEnumerator())
        {
            while (erator.MoveNext())
            {
                View view = erator.Current as View;
                T obj = view.GetSelfComponent<T>(name);
                if (obj != null)
                {
                    goalObj = obj;
                    break;
                }
            }
        }
        return goalObj;
    }

    /// <summary>
    /// 找子孩子的孩子
    /// </summary>
    public UnityEngine.Object FindChildViewChild(string name)
    {
        if (childeViews == null)
            return null;
        UnityEngine.Object goalObj = null;
        using (Dictionary<object, View>.ValueCollection.Enumerator erator = childeViews.Values.GetEnumerator())
        {
            while (erator.MoveNext())
            {
                View view = erator.Current as View;
                UnityEngine.Object obj = view.FindSelfChild(name);
                if (obj != null)
                {
                    goalObj = obj;
                    break;
                }
            }
        }
        return goalObj;
    }

    /// <summary>
    /// 找所有View中第一个名字为name的孩子(从自己开始找起)
    /// </summary>
    public UnityEngine.Object FindChild(string name)
    {
        UnityEngine.Object goalObj = FindSelfChild(name);
        if (goalObj != null)
            return goalObj;
        return FindChildViewChild(name);
    }

    /// <summary>
    /// 找所有View中第一个名字为name的孩子的组件(从自己开始找起)
    /// </summary>
    public T GetComponent<T>(string name) where T : Component
    {
        T goalCom = GetSelfComponent<T>(name);
        if (goalCom != null)
            return goalCom;
        return GetChildViewComponent<T>(name);
    }

    /// <summary>
    /// 设置子视图的别名
    /// </summary>
    public void SetChildViewAlias(object obj, View view)
    {
        if (childeViews.ContainsValue(view))
        {
            if (childeViews.ContainsKey(obj))
            {
                Debug.LogError("您指定的别名已被其他视图占用");
            }
            else
            {
                View aliasView = view;
                childeViews.Remove(view.Alias);
                aliasView.Alias = obj;
                childeViews.Add(aliasView.Alias, aliasView);
            }
        }
        else
            Debug.LogError("没有找到您要设置别名的对象！");
    }


    public View AddView(GameObject parentObj, GameObject child)
    {
        if (child == null)
        {
            Debug.LogError("要挂载的物体为空！");
            return null;
        }
        if (parentObj == null)
        {
            Debug.LogError("您指定的挂载点不存在！");
            return null;
        }

        View childView = child.GetComponent<View>();
        if (childView == null)
        {
            Debug.LogError("您挂载的物体不存在View组件");
            return null;
        }
        //
        child.transform.parent = parentObj.transform;
        child.transform.localPosition = Vector3.zero;
        child.transform.localScale = Vector3.one;
        childeViews.Add(childView.Alias, childView);
        return childView;
    }

    public View AddView(string parentName, GameObject child)
    {
        UnityEngine.Object obj = FindSelfChild(parentName);
        if (obj == null)
        {
            Debug.LogError("您指定的挂载点不存在！");
            return null;
        }
        else
        {
            return AddView(obj as GameObject, child);
        }
    }

    public View AddView(string parentName, string path)
    {
        GameObject obj = Resources.Load(path) as GameObject;
        if (obj == null)
        {
            Debug.LogError(path + "指定的路径不存在！");
            return null;
        }
        else
            return AddView(parentName, obj);
    }


}
