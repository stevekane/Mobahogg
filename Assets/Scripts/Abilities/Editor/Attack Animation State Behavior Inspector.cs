using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AttackAnimationStateBehavior))]
public class AttackAnimationStateBehaviorInspector : Editor {
  PreviewRenderUtility previewRenderUtility;

  Vector2 previewRotation = new(45, 60);
  float previewZoom = 5.0f;
  int previewFrame;

  void OnEnable() {
    previewRenderUtility = new PreviewRenderUtility();
    previewRenderUtility.camera.nearClipPlane = 0.1f;
    previewRenderUtility.camera.farClipPlane = 1000f;
    previewRenderUtility.camera.fieldOfView = 45f;
    previewRenderUtility.camera.clearFlags = CameraClearFlags.SolidColor;
    previewRenderUtility.camera.backgroundColor = Color.gray;
  }

  private void OnDisable() {
    previewRenderUtility.Cleanup();
  }

  public override void OnInspectorGUI() {
    AttackAnimationStateBehavior behavior = (AttackAnimationStateBehavior)target;
    behavior.Clip = (AnimationClip)EditorGUILayout.ObjectField("Clip", behavior.Clip, typeof(AnimationClip), false);
    behavior.ActiveFrame = EditorGUILayout.IntField("Active Frame", behavior.ActiveFrame);
    behavior.RecoveryFrame = EditorGUILayout.IntField("Recovery Frame", behavior.RecoveryFrame);
    var validPreviewObject = Selection.activeGameObject
      && Selection.activeGameObject.TryGetComponent(out Animator animator)
      && animator.runtimeAnimatorController;

    if (!EditorApplication.isPlaying && validPreviewObject) {
      var previewInstance = Instantiate(Selection.activeGameObject, Vector3.zero, default);
      previewRenderUtility.AddSingleGO(previewInstance);
      // Initialize the avatar attacher
      if (previewInstance.TryGetComponent(out AvatarAttacher avatarAttacher)) {
        avatarAttacher.Attach();
      }
      // Set valid Standard RP shader on all Renderers found in the heirarchy... gross
      var materials = new List<Material> { new Material(Shader.Find("Standard")) };
      foreach (var renderer in previewInstance.GetComponentsInChildren<Renderer>()) {
        renderer.SetSharedMaterials(materials);
      }
      // Turn on root motion to make the preview window track its motion
      previewInstance.GetComponent<Animator>().applyRootMotion = true;
      // If there is a clip, set the frame on the animated character
      if (behavior.Clip) {
        var duration = behavior.Clip.length;
        var frameRate = behavior.Clip.frameRate;
        var totalFrames = Mathf.FloorToInt(duration * frameRate);
        var normalizedTime = (float)previewFrame / totalFrames;
        behavior.Clip.SampleAnimation(previewInstance, normalizedTime * duration);
        previewFrame = EditorGUILayout.IntSlider("Preview Frame", previewFrame, 0, totalFrames);
      }
      GUILayout.BeginVertical("box");
      Rect previewRect = GUILayoutUtility.GetRect(256, 256, GUILayout.ExpandWidth(true));
      previewRenderUtility.BeginPreview(previewRect, GUIStyle.none);
      ProcessEvents(previewRect);
      var cameraOffset = Quaternion.Euler(previewRotation.x, previewRotation.y, 0) * Vector3.back * previewZoom;
      previewRenderUtility.camera.transform.position = previewInstance.transform.position + Vector3.up + cameraOffset;
      previewRenderUtility.camera.transform.LookAt(previewInstance.transform.position + Vector3.up);
      previewRenderUtility.Render();
      Texture previewTexture = previewRenderUtility.EndPreview();
      GUI.DrawTexture(previewRect, previewTexture, ScaleMode.StretchToFill, false);
      GUILayout.EndVertical();
      materials.ForEach(DestroyImmediate);
      DestroyImmediate(previewInstance);
    }
    if (GUI.changed) {
      EditorUtility.SetDirty(target);
    }
  }

  void ProcessEvents(Rect previewRect) {
    Event evt = Event.current;

    // Zoom with the mouse wheel
    if (evt.type == EventType.ScrollWheel && previewRect.Contains(evt.mousePosition)) {
        previewZoom = Mathf.Clamp(previewZoom - evt.delta.y * 0.1f, 1.0f, 10.0f);
        evt.Use();
    }

    // Rotate the camera on right-click drag
    if (evt.type == EventType.MouseDrag && evt.button == 1 && previewRect.Contains(evt.mousePosition)) {
        previewRotation.x = Mathf.Clamp(previewRotation.x + evt.delta.y * 0.5f, -90f, 90f); // Clamp vertical rotation
        previewRotation.y += evt.delta.x * 0.5f; // Horizontal rotation
        evt.Use();
    }
  }
}