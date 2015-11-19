﻿using UnityEngine;
using UnityEditor;

using System.Collections.Generic;

[ExecuteInEditMode]
[CustomEditor(typeof(Element),true)]
public class ElementEditor : Editor
{
    private SerializedProperty spWidgets;

    void OnEnable()
    {
        spWidgets = serializedObject.FindProperty("_widgets");
    }

    void OnDisable()
    {
        spWidgets = null;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(spWidgets);
        serializedObject.ApplyModifiedProperties();
    }
}
