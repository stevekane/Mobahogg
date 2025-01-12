#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class RootMotionCalculator : EditorWindow {
    public static void InspectCurves(AnimationClip clip) {
        if (clip == null) {
            Debug.LogError("Please select an AnimationClip.");
            return;
        }

        // List all curves in the animation clip
        EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(clip);
        foreach (var binding in bindings) {
            Debug.Log($"Path: {binding.path}, Type: {binding.type}, Property: {binding.propertyName}");
        }
    }

    public static Vector3 IntegrateRootMotion(AnimationClip clip, int sampleRate = 1000) {
        // Retrieve root motion curves
        AnimationCurve curveX = AnimationUtility.GetEditorCurve(clip, new EditorCurveBinding {
            path = "root",
            type = typeof(Transform),
            propertyName = "m_LocalPosition.x"
        });

        AnimationCurve curveY = AnimationUtility.GetEditorCurve(clip, new EditorCurveBinding {
            path = "root",
            type = typeof(Transform),
            propertyName = "m_LocalPosition.y"
        });

        AnimationCurve curveZ = AnimationUtility.GetEditorCurve(clip, new EditorCurveBinding {
            path = "root",
            type = typeof(Transform),
            propertyName = "m_LocalPosition.z"
        });

        if (curveX == null || curveY == null || curveZ == null) {
            Debug.LogError("Root motion curves not found.");
            return Vector3.zero;
        }

        // Duration of the clip
        float duration = clip.length;

        // Numerical integration using the trapezoidal rule
        float timeStep = duration / sampleRate;
        Vector3 totalDisplacement = Vector3.zero;

        for (int i = 0; i < sampleRate; i++) {
            float t1 = i * timeStep;
            float t2 = (i + 1) * timeStep;

            // Evaluate positions at t1 and t2
            Vector3 pos1 = new Vector3(curveX.Evaluate(t1), curveY.Evaluate(t1), curveZ.Evaluate(t1));
            Vector3 pos2 = new Vector3(curveX.Evaluate(t2), curveY.Evaluate(t2), curveZ.Evaluate(t2));

            // Add the trapezoidal contribution
            totalDisplacement += (pos1 + pos2) * 0.5f * timeStep;
        }
        return totalDisplacement;
    }

    private float FPS = 60;
    private AnimationClip animationClip;
    private GameObject model;

    private float clipLength;
    private Vector3 distancePerCycle;
    private Vector3 distancePerSecond;

    [MenuItem("Tools/Root Motion Calculator")]
    private static void ShowWindow() {
        GetWindow<RootMotionCalculator>("Root Motion Calculator");
    }

    private void OnGUI() {
        GUILayout.Label("Root Motion Calculator", EditorStyles.boldLabel);

        FPS = EditorGUILayout.FloatField("FPS", FPS);
        animationClip = (AnimationClip)EditorGUILayout.ObjectField("Animation Clip", animationClip, typeof(AnimationClip), false);
        model = (GameObject)EditorGUILayout.ObjectField("Model", model, typeof(GameObject), false);

        if (GUILayout.Button("Calculate Root Motion") && animationClip != null && model != null) {
            CalculateRootMotion();
        }

        if (animationClip != null) {
            // InspectCurves(animationClip);
            GUILayout.Space(10);
            GUILayout.Label($"Clip Length: {clipLength:F2} seconds", EditorStyles.label);
            GUILayout.Label($"Total Forward Distance: {distancePerCycle.z:F5} units", EditorStyles.label);
            GUILayout.Label($"Average Clip Forward Velocity: {animationClip.averageSpeed.z:F5} units/s", EditorStyles.label);
            GUILayout.Label($"Calculated Forward Velocity: {IntegrateRootMotion(animationClip, 6400).z/animationClip.length:F5} units/s", EditorStyles.label);
            GUILayout.Label($"Expected Total Distance: {animationClip.averageSpeed.z * animationClip.length:F5} units", EditorStyles.label);
            GUILayout.Label($"Distance Per Cycle: {distancePerCycle.z:F5}", EditorStyles.label);
            GUILayout.Label($"Distance Per Second: {distancePerSecond.z:F5}", EditorStyles.label);
            GUILayout.Label($"Distance from apparent and humanScale: {animationClip.apparentSpeed * animationClip.length * model.GetComponent<Animator>().humanScale}", EditorStyles.label);
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
        clipPlayable.SetDuration(animationClip.length);
        output.SetSourcePlayable(clipPlayable);

        graph.Play();

        // Measure root motion frame by frame
        Vector3 totalRootMotion = Vector3.zero;
        Vector3 previousPosition = tempModel.transform.position;

        clipLength = animationClip.length;
        float timePerFrame = (float)((double)clipLength/(double)FPS);
        float time = 0;

        clipPlayable.SetTime(0);
        while (true) {
          var timeRemaining = clipLength-time;
          if (timeRemaining < timePerFrame) {
            graph.Evaluate(timeRemaining);
            Vector3 currentPosition = tempModel.transform.position;
            totalRootMotion += currentPosition - previousPosition;
            previousPosition = currentPosition;
            break;
          } else {
            graph.Evaluate(timePerFrame);
            time += timePerFrame;
            Vector3 currentPosition = tempModel.transform.position;
            totalRootMotion += currentPosition - previousPosition;
            previousPosition = currentPosition;
          }
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
