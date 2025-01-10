#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class RootMotionCalculator : EditorWindow {
    private AnimationClip animationClip;
    private GameObject model;
    private bool isLooping;

    private float clipLength;
    private Vector3 distancePerCycle;
    private Vector3 distancePerSecond;

    [MenuItem("Tools/Root Motion Calculator")]
    private static void ShowWindow() {
        GetWindow<RootMotionCalculator>("Root Motion Calculator");
    }

    private void OnGUI() {
        GUILayout.Label("Root Motion Calculator", EditorStyles.boldLabel);

        animationClip = (AnimationClip)EditorGUILayout.ObjectField("Animation Clip", animationClip, typeof(AnimationClip), false);
        model = (GameObject)EditorGUILayout.ObjectField("Model", model, typeof(GameObject), false);

        if (animationClip != null) {
            isLooping = EditorGUILayout.Toggle("Is Looping", animationClip.isLooping);
        }

        if (GUILayout.Button("Calculate Root Motion") && animationClip != null && model != null) {
            CalculateRootMotion();
        }

        if (animationClip != null) {
            GUILayout.Space(10);
            GUILayout.Label($"Clip Length: {clipLength:F2} seconds", EditorStyles.label);
            GUILayout.Label($"Distance Per Cycle: {distancePerCycle}", EditorStyles.label);
            GUILayout.Label($"Distance Per Second: {distancePerSecond}", EditorStyles.label);
        }
    }

    private void CalculateRootMotion() {
        // Create a temporary GameObject to play the animation
        GameObject tempModel = Instantiate(model);
        tempModel.hideFlags = HideFlags.HideAndDontSave;

        Animator animator = tempModel.GetComponent<Animator>();
        if (animator == null) {
            Debug.LogError("The selected model does not have an Animator component.");
            DestroyImmediate(tempModel);
            return;
        }

        // Set up the PlayableGraph
        PlayableGraph graph = PlayableGraph.Create("RootMotionGraph");
        AnimationPlayableOutput output = AnimationPlayableOutput.Create(graph, "Output", animator);
        AnimationClipPlayable clipPlayable = AnimationClipPlayable.Create(graph, animationClip);
        output.SetSourcePlayable(clipPlayable);

        graph.Play();

        // Measure root motion frame by frame
        Vector3 totalRootMotion = Vector3.zero;
        Vector3 previousPosition = tempModel.transform.position;

        clipLength = animationClip.length;
        float frameRate = animationClip.frameRate;
        int totalFrames = Mathf.CeilToInt(clipLength * frameRate);

        if (isLooping) {
            totalFrames -= 1; // Adjust for looping
        }

        for (int i = 0; i < totalFrames; i++) {
            float normalizedTime = (float)i / totalFrames;
            clipPlayable.SetTime(normalizedTime * clipLength);
            graph.Evaluate(1f / frameRate);

            Vector3 currentPosition = tempModel.transform.position;
            totalRootMotion += currentPosition - previousPosition;
            previousPosition = currentPosition;
        }

        // Calculate distances
        distancePerCycle = totalRootMotion;
        distancePerSecond = totalRootMotion / clipLength;

        // Cleanup
        graph.Destroy();
        DestroyImmediate(tempModel);
    }
}
#endif
