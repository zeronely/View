using UnityEngine;
using UnityEditor;
using Spate;

[CustomPropertyDrawer(typeof(UnityDictionary), true)]
public class UnityDictionaryDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return 0f;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty sp = property.FindPropertyRelative("mItems");
        for (int i = 0, len = sp.arraySize; i < len; i++)
        {
            SerializedProperty spItem = sp.GetArrayElementAtIndex(i);
            SerializedProperty spItemKey = spItem.FindPropertyRelative("key");
            if (GUILayout.Button(spItemKey.stringValue, EditorStyles.textField))
            {
                EditorGUIUtility.PingObject(spItem.FindPropertyRelative("value").objectReferenceValue);
            }
        }
    }
}