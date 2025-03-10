using UnityEngine;
using UnityEditor;

public class CustomCurveEditorWindow : EditorWindow {
    AnimationClip selectedClip;
    string curveName = "CustomCurve";
    AnimationCurve customCurve = AnimationCurve.Linear(0, 0, 1, 1);

    [MenuItem("Window/Custom Curve Editor")]
    public static void ShowWindow() {
        GetWindow<CustomCurveEditorWindow>("Custom Curve Editor");
    }

    void OnGUI()
    {
        GUILayout.Label("Add a Custom Curve to an Animation Clip", EditorStyles.boldLabel);
        selectedClip = (AnimationClip)EditorGUILayout.ObjectField("Animation Clip", selectedClip, typeof(AnimationClip), false);
        curveName = EditorGUILayout.TextField("Curve Name", curveName);
        customCurve = EditorGUILayout.CurveField("Custom Curve", customCurve);
        if (GUILayout.Button("Add Custom Curve")) {
            if (selectedClip != null) {
                // Record the change for undo functionality.
                Undo.RecordObject(selectedClip, "Add Custom Curve");
                var binding = EditorCurveBinding.FloatCurve("", typeof(Animator), curveName);
                AnimationUtility.SetEditorCurve(selectedClip, binding, customCurve);
                EditorUtility.SetDirty(selectedClip);
                Debug.Log("Added custom curve: " + curveName + " to " + selectedClip.name);
            }
            else {
                Debug.LogWarning("Please select an Animation Clip first!");
            }
        }
    }
}
