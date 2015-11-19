using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
[CustomEditor(typeof(URect))]
public class URectEditor : Editor
{
    private URect mRect;

    void OnEnable()
    {
        mRect = target as URect;
    }

    void OnDisable()
    {
        mRect = null;
    }

    public override void OnInspectorGUI()
    {
        if (mRect == null) return;
        serializedObject.Update();
        GUI.changed = false;
        GUILayout.BeginHorizontal();
        int val = EditorGUILayout.IntField("Size", mRect.width, GUILayout.MinWidth(30f));
        if (GUI.changed)
        {
            NGUIEditorTools.RegisterUndo("Dimensions Change", mRect);
            mRect.width = val;
        }
        NGUIEditorTools.SetLabelWidth(12f);
        GUI.changed = false;
        val = EditorGUILayout.IntField("x", mRect.height, GUILayout.MinWidth(30f));
        if (GUI.changed)
        {
            NGUIEditorTools.RegisterUndo("Dimensions Change", mRect);
            mRect.height = val;
        }
    }
}