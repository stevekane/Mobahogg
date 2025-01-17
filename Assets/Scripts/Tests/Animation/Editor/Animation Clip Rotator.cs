using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.IO;

public class AnimationClipRotator : EditorWindow {
    private AnimationClip originalClip;
    private string rootBoneName = "Root";
    private Vector3 rotationOffset;
    private string savePath = "Assets/NewAnimationClip.anim";

    [MenuItem("Tools/Animation Clip Rotator")]
    public static void ShowWindow() {
        GetWindow<AnimationClipRotator>("Animation Clip Rotator");
    }

    private void OnGUI() {
        GUILayout.Label("Animation Clip Rotator", EditorStyles.boldLabel);

        // Select the animation clip
        originalClip = (AnimationClip)EditorGUILayout.ObjectField("Animation Clip", originalClip, typeof(AnimationClip), false);

        // Input root bone name
        rootBoneName = EditorGUILayout.TextField("Root Bone Name", rootBoneName);

        // Rotation offset
        rotationOffset = EditorGUILayout.Vector3Field("Rotation Offset (Euler)", rotationOffset);

        var bindings = AnimationUtility.GetCurveBindings(originalClip);
        foreach (var binding in bindings) {
          if (binding.path == rootBoneName && binding.propertyName.StartsWith("m_LocalRotation")) {
            GUILayout.Label($"Found binding at {binding.path} {binding.propertyName}");
          }
        }
        if (GUILayout.Button("Print")) {
          foreach (var binding in bindings) {
            Debug.Log($"Path:{binding.path} | PropertyName:{binding.propertyName}");
          }
        }

        // Specify save path
        GUILayout.BeginHorizontal();
        savePath = EditorGUILayout.TextField("Save Path", savePath);
        if (GUILayout.Button("...", GUILayout.Width(30))) {
            savePath = EditorUtility.SaveFilePanelInProject("Save New Animation Clip", "NewAnimationClip", "anim", "Select where to save the new animation clip.");
        }
        GUILayout.EndHorizontal();

        // Process and Save button
        if (GUILayout.Button("Create Rotated Animation")) {
            if (originalClip == null) {
                EditorUtility.DisplayDialog("Error", "Please select a valid animation clip.", "OK");
                return;
            }

            CreateRotatedAnimation();
        }
    }

    private void CreateRotatedAnimation() {
        // Duplicate the original clip
        AnimationClip newClip = new AnimationClip();
        EditorUtility.CopySerialized(originalClip, newClip);

        // Retrieve all bindings
        var bindings = AnimationUtility.GetCurveBindings(originalClip);
        foreach (var binding in bindings) {
            if (binding.path == rootBoneName && binding.propertyName.StartsWith("m_LocalRotation")) {
                // Process rotation curves
                var curve = AnimationUtility.GetEditorCurve(originalClip, binding);
                var newCurve = new AnimationCurve();

                foreach (var keyframe in curve.keys) {
                    Quaternion originalRotation = new Quaternion();
                    if (binding.propertyName.EndsWith(".x")) originalRotation.x = keyframe.value;
                    if (binding.propertyName.EndsWith(".y")) originalRotation.y = keyframe.value;
                    if (binding.propertyName.EndsWith(".z")) originalRotation.z = keyframe.value;
                    if (binding.propertyName.EndsWith(".w")) originalRotation.w = keyframe.value;

                    Quaternion offsetRotation = Quaternion.Euler(rotationOffset);
                    Quaternion rotated = offsetRotation * originalRotation;

                    // Add keyframe with modified rotation
                    var newKeyframe = new Keyframe(keyframe.time, rotated[binding.propertyName[^1]]);
                    newKeyframe.inTangent = keyframe.inTangent;
                    newKeyframe.outTangent = keyframe.outTangent;
                    newCurve.AddKey(newKeyframe);
                }

                // Apply the modified curve to the new clip
                AnimationUtility.SetEditorCurve(newClip, binding, newCurve);
            } else {
                // Copy other curves directly
                var curve = AnimationUtility.GetEditorCurve(originalClip, binding);
                AnimationUtility.SetEditorCurve(newClip, binding, curve);
            }
        }

        // Save the new clip to the specified path
        AssetDatabase.CreateAsset(newClip, savePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Success", "New animation clip created at " + savePath, "OK");
    }
}
