using UnityEngine;
using System.Collections.Generic;

namespace Spate
{
    public class View : BaseBehaviour
    {
        [SerializeField]
        [HideInInspector]
        public UnityDictionary NeedObject = null;
        private Dictionary<object, View> childeViews;
        //
        [SerializeField]
        private eWindowLayer _Layer = eWindowLayer.Normal;
        [SerializeField]
        private eWindowAnimType _OpenAnimType = eWindowAnimType.None;
        [SerializeField]
        private eWindowAnimType _CloseAnimType = eWindowAnimType.None;
        [SerializeField]
        private bool _isModal = true;

        private const string appointSuffix = "View";
        private const string appointPrefix = "#";

        Dictionary<string, UnityEngine.Object> needObj = new Dictionary<string, UnityEngine.Object>();
        List<UIPanel> subUIPanels;

        /// <summary>
        /// 窗口所在层
        /// </summary>
        public eWindowLayer Layer
        {
            get { return _Layer; }
        }

        public eWindowAnimType OpenAnimType
        {
            get { return _OpenAnimType; }
        }

        public eWindowAnimType CloseAnimType
        {
            get { return _CloseAnimType; }
        }

        public bool IsModel
        {
            get { return _isModal; }
        }


        public object Alias { get; set; }
        private UIPanel Panel
        {
            get
            {
                UIPanel panel = CachedGameObject.GetComponent<UIPanel>();
                return panel;
            }
        }

        private int _depth = 0;
        public int Depth
        {
            get
            {
                if (Panel != null)
                    _depth = Panel.depth;
                return _depth;
            }
            set
            {
                if (_depth != value)
                {
                    OnDepthChanged(value - _depth);
                    _depth = value;
                    if (Panel != null)
                    {
                        Panel.depth = value;
                        Panel.sortingOrder = value;
                    }
                }
            }
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
            subUIPanels = new List<UIPanel>();
        }

        void Start()
        {
            InitDepth();
        }

        void SaveView()
        {
            if (NeedObject == null)
                NeedObject = new UnityDictionary();
            else
                NeedObject.Clear();
            using (Dictionary<string, UnityEngine.Object>.KeyCollection.Enumerator erator = needObj.Keys.GetEnumerator())
            {
                while (erator.MoveNext())
                {
                    string key = erator.Current as string;
                    UnityEngine.Object obj = needObj[key];
                    //NGUIDebug.Log(key);
                    NeedObject.Add(key, obj);
                }
            }
        }

        void FindNeedChildrens()
        {
            needObj.Clear();
            Transform[] tfs = CachedTransform.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < tfs.Length; i++)
            {
                Transform tf = tfs[i];
                if (tf.gameObject.name.StartsWith(appointPrefix))
                {
                    if (!needObj.ContainsKey(tf.gameObject.name))
                        needObj.Add(tf.gameObject.name, tf.gameObject);
                    else
                        Debug.LogError(tf.gameObject.name + "is constant !");
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
        public GameObject FindSelfChild(string name)
        {
            if (NeedObject == null)
                return null;
            return NeedObject.Get(name) as GameObject;
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
        public GameObject FindChildViewChild(object alias, string name)
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
        public GameObject FindChildViewChild(string name)
        {
            if (childeViews == null)
                return null;
            GameObject goalObj = null;
            using (Dictionary<object, View>.ValueCollection.Enumerator erator = childeViews.Values.GetEnumerator())
            {
                while (erator.MoveNext())
                {
                    View view = erator.Current as View;
                    GameObject obj = view.FindSelfChild(name);
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
        public GameObject FindChild(string name)
        {
            GameObject goalObj = FindSelfChild(name);
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

        public void RemoveChildView(object alias)
        {
            if (alias == null)
                return;
            if (childeViews.ContainsKey(alias))
            {
                Destroy(childeViews[alias].gameObject);
                childeViews.Remove(alias);
            }
        }

        public void RemoveChildView(View view)
        {
            if (view == null)
                return;
            if (childeViews.ContainsValue(view))
            {
                object key = null;
                using (Dictionary<object, View>.KeyCollection.Enumerator eartor = childeViews.Keys.GetEnumerator())
                {
                    while (eartor.MoveNext())
                    {
                        object cur = eartor.Current as object;
                        if (childeViews[cur] == view)
                        {
                            key = cur;
                            break;
                        }
                    }
                }
                Destroy(view.gameObject);
                childeViews.Remove(key);
            }
        }

        private void OnDepthChanged(int changeDepth)
        {
            for (int i = 0; i < subUIPanels.Count; i++)
            {
                UIPanel panel = subUIPanels[i];
                panel.depth = panel.depth + changeDepth;
                panel.sortingOrder = panel.sortingOrder + changeDepth;
            }
        }

        private void InitDepth()
        {
            List<int> sorts = new List<int>();
            UIPanel[] panels = CachedTransform.GetComponentsInChildren<UIPanel>(true);
            for (int i = 0; i < panels.Length; i++)
            {
                if (Panel != panels[i])
                {
                    subUIPanels.Add(panels[i]);
                    if (!sorts.Contains(panels[i].depth))
                        sorts.Add(panels[i].depth);
                }
            }
            sorts.Sort(CompareDepth);

           
            for (int i = 0; i < subUIPanels.Count; i++)
            {
                UIPanel panel = subUIPanels[i];
                //每一层空出一层给粒子系统
                int count = 0;
                for (int j = 0; j < sorts.Count;j++)
                {
                    if(panel.depth == sorts[j])
                        count = j + 1;
                }
                panel.depth = panel.depth + count;
                panel.sortingOrder = panel.sortingOrder + count;
            }
        }

        private int CompareDepth(int a, int b)
        {
            if (a > b)
                return 1;
            else if (a < b)
                return -1;
            else
                return 0;
        }
    }
}
