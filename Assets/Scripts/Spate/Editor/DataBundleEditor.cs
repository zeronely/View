﻿using UnityEngine;
using UnityEditor;
using System;


[CustomPropertyDrawer(typeof(DataBundle))]
public class DataBundleEditor : PropertyDrawer
{
    private Type valueType = null;
    private string tmpKey = string.Empty;
    // 0正常,1长度不合法，2名称已存在
    private int tmpErrorCode = 0;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (valueType == null)
        {
            SerializedProperty spType = property.FindPropertyRelative("_type");
            string typeFullName = spType.stringValue;
            valueType = Type.GetType(typeFullName);
        }

        return 2f;
        // return 3 * EditorGUIUtility.singleLineHeight + 2 * EditorGUIUtility.standardVerticalSpacing;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (NGUIEditorTools.DrawHeader(label.text, false))
        {
            //SpateEditorUtil.BeginContents();
            EditorGUIUtility.labelWidth = 55f;
            SerializedProperty spList = property.FindPropertyRelative("_list");
            // 绘制已有项
            for (int i = 0, len = spList.arraySize; i < len; i++)
            {
                SerializedProperty spEntity = spList.GetArrayElementAtIndex(i);
                switch (DrawEntity(spEntity, i == 0, i == (len - 1)))
                {
                    case DrawState.Delete:
                        {
                            // 移除该元素
                            spList.DeleteArrayElementAtIndex(i);
                        }
                        break;
                    case DrawState.Up:
                        {
                            // 和上一个元素互换位置
                            spList.MoveArrayElement(i, i - 1);
                        }
                        break;
                    case DrawState.Down:
                        {
                            // 和下一个元素互换位置
                            spList.MoveArrayElement(i, i + 1);
                        }
                        break;
                    default:
                        continue;
                }
                break;
            }
            // 绘制新增项
            DrawNew(spList);
            //SpateEditorUtil.EndContents();
        }
    }

    private void DrawNew(SerializedProperty spList)
    {
        // 绘制新增项
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("New Key:", GUILayout.MinWidth(55), GUILayout.MaxWidth(110));
        GUI.changed = false;
        tmpKey = EditorGUILayout.TextField(tmpKey);
        GUILayout.EndHorizontal();
        // 一旦有变动，就要清除ErrorCode,这样可以保证出错后修改能立马清除错误
        if (GUI.changed) tmpErrorCode = 0;
        Event curEvent = Event.current;
        // Enter弹起时进行校验
        if (curEvent.type == EventType.KeyUp && (curEvent.keyCode == KeyCode.Return ||
            curEvent.keyCode == KeyCode.KeypadEnter))
        {
            if (string.IsNullOrEmpty(tmpKey) || tmpKey.Length > 40)
            {
                // 长度不合法
                tmpErrorCode = 1;
            }
            else if (ContainsKey(spList, tmpKey))
            {
                // 名称已存在
                tmpErrorCode = 2;
            }
            else
            {
                // 添加新项
                int index = spList.arraySize;
                spList.InsertArrayElementAtIndex(index);
                SerializedProperty spNewEntity = spList.GetArrayElementAtIndex(index);
                SerializedProperty spNewEntityKey = spNewEntity.FindPropertyRelative("key");
                spNewEntityKey.stringValue = tmpKey;
                // 恢复
                tmpKey = string.Empty;
                tmpErrorCode = 0;
            }
            // 切记要调用
            curEvent.Use();
        }
        switch (tmpErrorCode)
        {
            case 1:
                EditorGUILayout.HelpBox("Key Can Not Be Empty!But Less Than 40 Yet!", MessageType.Error);
                break;
            case 2:
                EditorGUILayout.HelpBox("Same Key Has Exist!", MessageType.Error);
                break;
        }
    }

    /// <summary>
    /// 绘制DataBundle.Entity,如果需要终止继续绘制则返回false
    /// </summary>
    private DrawState DrawEntity(SerializedProperty spEntity, bool isFirst, bool isLast)
    {
        DrawState state = DrawState.None;

        SerializedProperty spKey = spEntity.FindPropertyRelative("key");
        SerializedProperty spValue = spEntity.FindPropertyRelative("value");

        GUILayout.BeginHorizontal();
        // key
        EditorGUILayout.LabelField(spKey.stringValue + ":", GUILayout.Width(110f));
        // value
        spValue.objectReferenceValue = EditorGUILayout.ObjectField(spValue.objectReferenceValue, valueType, true);
        // (up)
        if (isFirst)
        {
            GUILayout.Label("\u25B2", "dragtab", GUILayout.Width(25f), GUILayout.Height(16f));
        }
        else
        {
            if (GUILayout.Button("\u25B2", "minibutton", GUILayout.Width(25f), GUILayout.Height(16f)))
            {
                state = DrawState.Up;
            }
        }
        // (down)
        if (isLast)
        {
            GUILayout.Label("\u25BC", "dragtab", GUILayout.Width(25f), GUILayout.Height(16f));
        }
        else
        {
            if (GUILayout.Button("\u25BC", "minibutton", GUILayout.Width(25f), GUILayout.Height(16f)))
            {
                state = DrawState.Down;
            }
        }
        // (delete)
        if (GUILayout.Button("\u2716", "minibutton", GUILayout.Width(25f), GUILayout.Height(16f)))
        {
            state = DrawState.Delete;
        }
        GUILayout.EndHorizontal();
        return state;
    }

    private enum DrawState
    {
        None,
        Delete,
        Up,
        Down
    }
   

    private bool ContainsKey(SerializedProperty spList, string key)
    {
        bool ret = false;
        for (int i = 0, len = spList.arraySize; i < len; i++)
        {
            SerializedProperty spKey = spList.GetArrayElementAtIndex(i).FindPropertyRelative("key");
            if (string.Equals(spKey.stringValue, key))
            {
                ret = true;
                break;
            }
        }
        return ret;
    }
}