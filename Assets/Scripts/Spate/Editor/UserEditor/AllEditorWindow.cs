using UnityEngine;
using UnityEditor;

using System;
using System.Reflection;
using System.Collections.Generic;


public class AllEditorWindow : EditorWindow
{
    // 记录左边栏的ScrollView的滚动位置
    private const string KEY_VEC_SV_POS = "KEY_VEC_SV_POS";
    // 记录左边栏的选择项
    private const string KEY_STR_CU_EDT = "KEY_STR_CU_EDT";

    private Vector2 mLeftSVPos = Vector2.zero;
    // 存储所有的BaseEditorWindow
    private List<EditorEntity> listEditor = null;
    private Color mColor;

    void OnEnable()
    {
        // 读取数据
        string leftSVPosText = EditorPrefs.GetString(KEY_VEC_SV_POS, "0,0");
        string[] array = leftSVPosText.Split(',');
        mLeftSVPos.x = float.Parse(array[0]);
        mLeftSVPos.y = float.Parse(array[1]);

        // 通过反射查找该dll下所有的BaseEditorWindow
        listEditor = new List<EditorEntity>(20);
        Type[] allTypes = Assembly.GetExecutingAssembly().GetTypes();
        foreach (Type t in allTypes)
        {
            if (t.IsSubclassOf(typeof(BaseEditorWindow)))
            {
                EditorEntity entity = new EditorEntity();
                entity.editor = (BaseEditorWindow)Activator.CreateInstance(t, true);
                entity.editor.aaaaa = this;
                entity.isOpenOnce = false;
                entity.isActive = false;
                listEditor.Add(entity);
            }
        }
        listEditor.TrimExcess();

        // 还原上次选择的窗口
        if (listEditor.Count > 0)
        {
            string editorName = EditorPrefs.GetString(KEY_STR_CU_EDT);
            if (!string.IsNullOrEmpty(editorName))
            {
                foreach (EditorEntity entity in listEditor)
                {
                    if (string.Equals(editorName, entity.editor.Name))
                    {
                        SwitchEditor(entity);
                        break;
                    }
                }
            }
        }
    }

    public static void OpenWindow(BaseEditorWindow win)
    {
        EditorPrefs.SetString(KEY_STR_CU_EDT, win.Name);
    }

    void OnDisable()
    {
        // 保存数据
        string leftSVPosText = string.Format("{0},{1}", mLeftSVPos.x, mLeftSVPos.y);
        EditorPrefs.SetString(KEY_VEC_SV_POS, leftSVPosText);

        // 保存当前项
        foreach (EditorEntity entity in listEditor)
        {
            string editorName = entity.editor.Name;
            if (!entity.isDestroyed)
            {
                entity.editor.OnDestroy();
                entity.isDestroyed = true;
            }
            if (entity.isActive)
            {
                EditorPrefs.SetString(KEY_STR_CU_EDT, editorName);
            }
        }
        listEditor.Clear();
        listEditor = null;
    }

    void OnGUI()
    {
        if (listEditor.Count == 0)
        {
            OnGUI_Empty();
        }
        else
        {
            GUILayout.Space(5);
            // 当前待渲染到右边的Editor
            BaseEditorWindow curEditor = null;

            EditorGUILayout.BeginHorizontal();
            // 绘制左边选择列表
            GUILayout.Space(5);
            mColor = GUI.backgroundColor;
            GUI.backgroundColor = Color.cyan;
            EditorGUILayout.BeginVertical(GUILayout.Width(120));
            EditorGUILayout.BeginScrollView(mLeftSVPos);
            foreach (EditorEntity entity in listEditor)
            {
                if (entity.isActive)
                    curEditor = entity.editor;
                BaseEditorWindow editor = entity.editor;
                if (GUILayout.Button(editor.Name, entity.isActive ? "LargeButtonMid" : "LargeButtonLeft"))
                {
                    if (!entity.isActive)
                    {
                        SwitchEditor(entity);
                        // 刷新,不需要继续渲染了
                        break;
                    }
                }
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
            // 绘制右边对应的窗口
            GUILayout.Space(10);
            GUI.backgroundColor = mColor;
            if (curEditor != null)
            {
                EditorGUILayout.BeginVertical();
                curEditor.OnGUI();
                EditorGUILayout.EndVertical();
            }
            GUILayout.Space(5);
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(5);
        }
    }

    void OnGUI_Empty()
    {
        EditorGUILayout.LabelField("没有Editor", "Big");
    }

    private void SwitchEditor(EditorEntity newEntity)
    {
        EditorEntity oldEntity = FindOldActiveEntity();
        if (oldEntity != null)
        {
            //隐藏
            oldEntity.isActive = false;
            oldEntity.editor.OnHide();
            // 自动销毁
            if (oldEntity.editor.AutoDestroy && !oldEntity.isDestroyed)
            {
                oldEntity.editor.OnDestroy();
                oldEntity.isDestroyed = true;
                oldEntity.isOpenOnce = false;
            }
        }
        // 打开新的Editor
        if (!newEntity.isOpenOnce)
        {
            newEntity.isDestroyed = false;
            newEntity.editor.OnCreate();
        }
        newEntity.editor.OnOpen();
        newEntity.isOpenOnce = true;
        newEntity.isActive = true;
    }

    private EditorEntity FindOldActiveEntity()
    {
        if (listEditor == null || listEditor.Count == 0)
            return null;
        EditorEntity result = null;
        foreach (EditorEntity entity in listEditor)
        {
            if (entity.isActive)
            {
                result = entity;
                break;
            }
        }
        return result;
    }

    private class EditorEntity
    {
        public BaseEditorWindow editor;
        // 是否曾经打开过
        public bool isOpenOnce;
        // 当前是否处于激活状态
        public bool isActive;
        // 是否已经被销毁过了
        public bool isDestroyed;
    }
}
