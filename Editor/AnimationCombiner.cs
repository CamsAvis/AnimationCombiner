using PlasticPipe.PlasticProtocol.Messages;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class AnimationCombiner : EditorWindow
{
    const string GENERATED_ASSETS_FOLDER = "!Cam/Scripts/Generated Assets";

    List<AnimationClip> clips;
    Vector2 scrollPos;

    [MenuItem("Cam/Animation Combiner")]
    public static void ShowWindow() => EditorWindow.GetWindow<AnimationCombiner>().Show();

    public void OnEnable() => clips = new List<AnimationClip>();

    public void OnGUI()
    {
        float screenWidth = EditorGUIUtility.currentViewWidth - 12.5f;

        GUILayout.Label("Clips", EditorStyles.whiteLargeLabel);
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Add Selected", GUILayout.Width(screenWidth / 2f)))
                AddSelectedClips();
            if (GUILayout.Button("Clear", GUILayout.Width(screenWidth / 2f)))
                clips = new List<AnimationClip>();
        }
        scrollPos = GUILayout.BeginScrollView(scrollPos, "box");
        {
            for (int i = 0; i < clips.Count; i++)
            {
                bool removed = DrawClip(i);
                if (removed) return;
            }
        }
        GUILayout.EndScrollView();

        GUILayout.Space(10);

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Merge All In List", GUILayout.Width(screenWidth / 2f)))
                MergeClips();

            if (GUILayout.Button("Merge Selected", GUILayout.Width(screenWidth / 2f)))
                MergeSelectedClips();
        }

        GUILayout.Space(10);
    }

    bool DrawClip(int index)
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUIUtility.labelWidth = 25;
            GUILayout.Label((index + 1).ToString(), GUILayout.Width(25));
            GUI.enabled = false;
            EditorGUILayout.ObjectField(clips[index], typeof(AnimationClip), true);
            GUI.enabled = true;

            if (GUILayout.Button("Remove", GUILayout.Width(75)))
            {
                clips.Remove(clips[index]);
                return true;
            }
        }

        return false;
    }

    void GenerateDirectory()
    {
        string dirAbsPath = $"{Application.dataPath}/{GENERATED_ASSETS_FOLDER}";
        if (!System.IO.Directory.Exists(dirAbsPath))
            System.IO.Directory.CreateDirectory(dirAbsPath);
    }

    void MergeSelectedClips()
    {
        List<AnimationClip> oldClips = clips;
        clips = new List<AnimationClip>();
        AddSelectedClips();
        MergeClips();
        clips = oldClips;
    }

    void MergeClips()
    {
        GenerateDirectory();

        AnimationClip newClip = new AnimationClip();
        EditorUtility.SetDirty(newClip);

        foreach(AnimationClip clip in clips)
        {
            EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(clip);
            foreach(EditorCurveBinding binding in bindings) {
                AnimationUtility.SetEditorCurve(
                    newClip, binding, AnimationUtility.GetEditorCurve(clip, binding)
                );
            }
        }

        string path = AssetDatabase.GenerateUniqueAssetPath($"Assets/{GENERATED_ASSETS_FOLDER}/CombinedAnimation.anim");
        AssetDatabase.CreateAsset(newClip, path);
        EditorGUIUtility.PingObject(newClip);
    }

    void AddSelectedClips()
    {
        clips = clips ?? new List<AnimationClip>();

        Debug.Log("Getting Selected Clips");
        List<AnimationClip> selectedClips = Selection.objects
            .Where(o => o.GetType().Equals(typeof(AnimationClip)))
            .Cast<AnimationClip>()
            .ToList();

        foreach (AnimationClip clip in selectedClips) {
            Debug.Log($"Clip: {clip.name}");
        }

        clips = clips.Concat(selectedClips).ToList();
    }
}
