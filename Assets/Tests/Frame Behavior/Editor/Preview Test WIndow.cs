using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Playables;
using UnityEngine.Animations;
using UnityEngine.VFX;
using UnityEditor.Search;

public class PreviewTestWindow : EditorWindow {
  VisualEffectAsset vfxAsset;
  Animator targetAnimator;
  AnimationClip animClip;
  int currentFrame;
  PreviewRenderUtility previewUtility;
  PlayableGraph playableGraph;
  AnimationClipPlayable clipPlayable;
  VisualEffect previewVFX;
  GameObject previewGO;
  IMGUIContainer container;

  [MenuItem("Window/PreviewTestWindow")]
  public static void ShowWindow() => GetWindow<PreviewTestWindow>();

  void OnEnable() {
    previewUtility = new PreviewRenderUtility();
    previewUtility.camera.fieldOfView = 30;
    previewUtility.camera.clearFlags = CameraClearFlags.SolidColor;
    previewUtility.camera.backgroundColor = Color.gray;
    previewUtility.camera.nearClipPlane = 0.1f;
    previewUtility.camera.farClipPlane = 1000f;
    previewUtility.camera.transform.position = new Vector3(0, 5, 5);
    previewUtility.camera.transform.LookAt(Vector3.zero);
    previewUtility.camera.cameraType = CameraType.SceneView;

    var VFXAsset = new ObjectField();
    container = new IMGUIContainer(DrawIMGUI);
    rootVisualElement.Add(container);
    EditorApplication.update += UpdateWindow;
  }

  void OnDisable() {
    EditorApplication.update -= UpdateWindow;
    if (playableGraph.IsValid()) playableGraph.Destroy();
    previewUtility.Cleanup();
    if (previewGO != null) DestroyImmediate(previewGO);
  }

  void DrawIMGUI() {
    EditorGUILayout.BeginVertical();
    vfxAsset = (VisualEffectAsset)EditorGUILayout.ObjectField("VFX Asset", vfxAsset, typeof(VisualEffectAsset), false);
    targetAnimator = (Animator)EditorGUILayout.ObjectField("Animator", targetAnimator, typeof(Animator), true);
    animClip = (AnimationClip)EditorGUILayout.ObjectField("Anim Clip", animClip, typeof(AnimationClip), false);
    currentFrame = EditorGUILayout.IntSlider("Frame", currentFrame, 0, 120);
    EditorGUILayout.EndVertical();
    Rect r = new Rect(0, 100, position.width, position.height - 100);
    previewUtility.BeginPreview(r, GUIStyle.none);
    previewUtility.Render();
    Texture t = previewUtility.EndPreview();
    GUI.DrawTexture(r, t, ScaleMode.StretchToFill);
    UpdateSystems();
  }

  void UpdateSystems() {
    if (targetAnimator != null && animClip != null) {
      if (!playableGraph.IsValid()) {
        playableGraph = PlayableGraph.Create("PreviewGraph");
        var output = AnimationPlayableOutput.Create(playableGraph, "Animation", targetAnimator);
        clipPlayable = AnimationClipPlayable.Create(playableGraph, animClip);
        output.SetSourcePlayable(clipPlayable);
        playableGraph.Play();
      } else {
        clipPlayable.SetTime(currentFrame / 60f);
        playableGraph.Evaluate();
      }
    }
    if (vfxAsset != null) {
      previewVFX.visualEffectAsset = vfxAsset;
      previewVFX.Reinit();
      previewVFX.Simulate(1f / 60f, (uint)currentFrame);
    }
    rootVisualElement.MarkDirtyRepaint();
  }

  void UpdateWindow() {
    container.MarkDirtyRepaint();
  }
}
