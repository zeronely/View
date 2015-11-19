﻿using UnityEngine;
using UnityEditor;
using Spate;
using System.Collections.Generic;


//[ExecuteInEditMode]
//[CustomEditor(typeof(Window),true)]
//public class WindowEditor : Editor
//{
//    private SerializedProperty spOpenMotion;
//    private SerializedProperty spCloseMotion;
//    private SerializedProperty spLayer;
//    private SerializedProperty spModal;
//    private SerializedProperty spCloseOnBlur;
//    private SerializedProperty spWidgets;
//    private SerializedProperty spFsm;

//    void OnEnable()
//    {
//        spOpenMotion = serializedObject.FindProperty("_openMotion");
//        spCloseMotion = serializedObject.FindProperty("_closeMotion");
//        spLayer = serializedObject.FindProperty("_layer");
//        spModal = serializedObject.FindProperty("_isModal");
//        spCloseOnBlur = serializedObject.FindProperty("_isCloseOnBlur");
//        spWidgets = serializedObject.FindProperty("_widgets");
//        spFsm = serializedObject.FindProperty("_fsm");
//    }

//    void OnDisable()
//    {
//        spOpenMotion = null;
//        spCloseMotion = null;
//        spLayer = null;
//        spModal = null;
//        spCloseOnBlur = null;
//        spWidgets = null;
//        spFsm = null;
//    }


//    public override void OnInspectorGUI()
//    {
//        serializedObject.Update();
//        EditorGUILayout.PropertyField(spOpenMotion);
//        EditorGUILayout.PropertyField(spCloseMotion);
//        EditorGUILayout.PropertyField(spLayer);
//        EditorGUILayout.PropertyField(spModal);
//        EditorGUILayout.PropertyField(spCloseOnBlur);
//        EditorGUILayout.PropertyField(spWidgets);
//        EditorGUILayout.PropertyField(spFsm);
//        serializedObject.ApplyModifiedProperties();
//    }

//}