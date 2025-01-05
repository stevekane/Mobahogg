using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AttackAnimationStateBehavior))]
public class AttackAnimationStateBehaviorInspector : Editor
{
    private GameObject selectedModel;
    private PreviewRenderUtility previewRenderUtility;
    private GameObject previewInstance;
    private Animator previewAnimator;
    private int currentFrame;
    private Vector2 previewPan;
    private float previewZoom = 1.0f;
    private Vector2 previewRotation;

    private void OnEnable()
    {
        previewRenderUtility = new PreviewRenderUtility();
        previewRenderUtility.camera.nearClipPlane = 0.1f;
        previewRenderUtility.camera.farClipPlane = 1000f;
        previewRenderUtility.camera.fieldOfView = 45f;
        previewRenderUtility.camera.clearFlags = CameraClearFlags.SolidColor;
        previewRenderUtility.camera.backgroundColor = Color.gray;
    }

    private void OnDisable()
    {
        if (previewInstance != null)
        {
            DestroyImmediate(previewInstance);
        }

        previewRenderUtility.Cleanup();
    }

    public override void OnInspectorGUI()
    {
        AttackAnimationStateBehavior behavior = (AttackAnimationStateBehavior)target;

        // Select a model from the project
        selectedModel = (GameObject)EditorGUILayout.ObjectField("Model", selectedModel, typeof(GameObject), false);

        behavior.Clip = (AnimationClip)EditorGUILayout.ObjectField("Clip", behavior.Clip, typeof(AnimationClip), false);
        behavior.ActiveFrame = EditorGUILayout.IntField("Active Frame", behavior.ActiveFrame);
        behavior.RecoveryFrame = EditorGUILayout.IntField("Recovery Frame", behavior.RecoveryFrame);

        if (selectedModel != null && behavior.Clip != null)
        {
            if (previewInstance == null || previewInstance.name != selectedModel.name)
            {
                if (previewInstance != null)
                {
                    DestroyImmediate(previewInstance);
                }

                previewInstance = Instantiate(selectedModel);
                previewAnimator = previewInstance.GetComponent<Animator>();
                previewRenderUtility.AddSingleGO(previewInstance);
            }

            float duration = behavior.Clip.length;
            float frameRate = behavior.Clip.frameRate;
            int totalFrames = Mathf.FloorToInt(duration * frameRate);

            // Frame control slider for the preview
            currentFrame = EditorGUILayout.IntSlider("Preview Frame", currentFrame, 0, totalFrames);

            // Preview the selected clip in the preview window
            if (previewAnimator != null && behavior.Clip != null)
            {
                float normalizedTime = (float)currentFrame / totalFrames;
                behavior.Clip.SampleAnimation(previewInstance, normalizedTime * duration);

                GUILayout.BeginVertical("box");
                Rect previewRect = GUILayoutUtility.GetRect(256, 256, GUILayout.ExpandWidth(true));

                Event evt = Event.current;
                if (evt.type == EventType.ScrollWheel && previewRect.Contains(evt.mousePosition))
                {
                    previewZoom = Mathf.Clamp(previewZoom - evt.delta.y * 0.1f, 0.2f, 5.0f);
                    evt.Use();
                }

                if (evt.type == EventType.MouseDrag && evt.button == 1 && previewRect.Contains(evt.mousePosition))
                {
                    previewRotation += evt.delta * 0.2f;
                    evt.Use();
                }

                if (evt.type == EventType.MouseDrag && evt.button == 2 && previewRect.Contains(evt.mousePosition))
                {
                    previewPan += evt.delta * 0.005f;
                    evt.Use();
                }

                previewRenderUtility.BeginPreview(previewRect, GUIStyle.none);
                Vector3 cameraPosition = new Vector3(previewPan.x, 1 + previewPan.y, -3 / previewZoom);
                Quaternion rotation = Quaternion.Euler(previewRotation.y, previewRotation.x, 0);
                previewRenderUtility.camera.transform.position = rotation * cameraPosition;
                previewRenderUtility.camera.transform.LookAt(new Vector3(0, 1, 0));
                previewRenderUtility.Render();
                Texture previewTexture = previewRenderUtility.EndPreview();
                GUI.DrawTexture(previewRect, previewTexture, ScaleMode.StretchToFill, false);
                GUILayout.EndVertical();
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Please select both an Animation Clip and a Model to preview.", MessageType.Info);
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }
}