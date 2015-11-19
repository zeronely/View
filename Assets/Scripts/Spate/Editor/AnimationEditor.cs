using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Animation))]
public sealed class AnimationEditor : Editor
{
    public Animation targetAnimation;

    private AnimationClip[] animationClips = null;

    void OnEnable()
    {
        targetAnimation = (Animation)target;
        animationClips = AnimationUtility.GetAnimationClips(targetAnimation.gameObject);
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        Separator();

        GUI.color = Color.yellow;
        GUILayout.Label("Expansion", "GUIEditor.BreadcrumbLeft");
        GUI.color = Color.white;
        targetAnimation.wrapMode = (WrapMode)EditorGUILayout.EnumPopup("Wrap Mode", targetAnimation.wrapMode);
        EditorGUILayout.Space();
        for (int i = 0; i < animationClips.Length; i++)
        {
            if (animationClips[i] == null)
                return;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.ObjectField(animationClips[i], typeof(AnimationClip), false);
            if (GUILayout.Button("Preview"))
            {
                if (!Application.isPlaying)
                {
                    EditorApplication.ExecuteMenuItem("Edit/Play");
                }
                targetAnimation.Play(animationClips[i].name);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Time:  " + animationClips[i].length, GUILayout.Width(100));
            GUILayout.Label("Frame:  " + animationClips[i].length * animationClips[i].frameRate);
            EditorGUILayout.EndHorizontal();
            if (targetAnimation[animationClips[i].name].normalizedTime > 0)
            {
                EditorGUI.ProgressBar(GUILayoutUtility.GetRect(Screen.width, 30), targetAnimation[animationClips[i].name].time / animationClips[i].length, "Curret time:" + targetAnimation[animationClips[i].name].time.ToString("0.0000"));
            }
            EditorGUILayout.Space();
        }

    }
    void Separator()
    {
        GUI.color = new Color(1, 1, 1, 0.25f);
        GUILayout.Box("", "HorizontalSlider", GUILayout.Height(16));
        GUI.color = Color.white;
    }
}
