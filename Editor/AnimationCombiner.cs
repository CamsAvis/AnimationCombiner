using PlasticPipe.PlasticProtocol.Messages;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class AnimationCombiner : EditorWindow
{
    List<AnimationClip> clips;
    string generatedAssetsDir = "!Cam/Scripts/Generated Assets";

    [MenuItem("Cam/Animation Combiner")]
    public static void ShowWindow() => EditorWindow.GetWindow<AnimationCombiner>().Show();

    public void OnEnable() => clips = new List<AnimationClip>();

    public void OnGUI()
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Add Selected"))
                AddSelectedClips();
            if (GUILayout.Button("Clear"))
                clips = new List<AnimationClip>();
        }

        if (GUILayout.Button("Merge"))
            MergeClips();

        if (GUILayout.Button("Merge Selected"))
            MergeSelectedClips();

        GUILayout.Label("Clips");
        GUI.enabled = false;
        EditorGUIUtility.labelWidth = 25;
        for(int i = 0; i < clips.Count; i++) {
            EditorGUILayout.ObjectField((i+1).ToString(), clips[i], typeof(AnimationClip), true);
        }
        GUI.enabled = true;
    }

    void GenerateDirectory()
    {
        string dirAbsPath = $"{Application.dataPath}/{generatedAssetsDir}";
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

        string path = AssetDatabase.GenerateUniqueAssetPath($"Assets/{generatedAssetsDir}/CombinedAnimation.anim");
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
