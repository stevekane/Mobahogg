using UnityEngine;
using UnityEditor;

public class AnimationRootBoneProcessor : AssetPostprocessor {
    // Called when an animation is imported
    void OnPostprocessAnimation(GameObject gameObject, AnimationClip clip) {
        Debug.Log($"Processing animation clip: {clip.name}");

        // Specify the root bone name (customize as needed)
        string rootBoneName = "root";

        // Get all curve bindings in the clip
        var bindings = AnimationUtility.GetCurveBindings(clip);

        // Find rotation curves for the root bone
        foreach (var binding in bindings) {
            if (binding.path == rootBoneName && binding.propertyName.StartsWith("m_LocalRotation")) {
                Debug.Log($"Zeroing out rotation curve: {binding.propertyName}");

                // Replace the curve with a constant (identity quaternion for rotations)
                var newCurve = new AnimationCurve();
                newCurve.AddKey(0f, binding.propertyName == "m_LocalRotation.w" ? 1f : 0f); // w=1, x/y/z=0
                AnimationUtility.SetEditorCurve(clip, binding, newCurve);
            }
        }

        Debug.Log($"Finished processing animation clip: {clip.name}");
    }
}