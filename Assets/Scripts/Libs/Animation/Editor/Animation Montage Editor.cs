using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

[CustomEditor(typeof(AnimationMontage))]
public class AnimationMontageInspector : Editor {
  const float TrackHeight = 20f;
  const float TrackSpacing = 10f;
  const float PaddingLeft = 60f;
  const float PaddingRight = 20f;
  const float FrameDuration = 1f/60;
  const int MinVisibleFrames = 60;
  const int MaxVisibleFrames = 60*10;

  double CurrentClockTime;
  double PreviousClockTime;
  double DeltaTime => CurrentClockTime-PreviousClockTime;

  int VisibleFrames = 60;
  int FrameOffset = 0;
  int CurrentFrame = 0;
  bool IsPlaying;
  bool SubscribedToEditorUpdate;

  PlayableGraph Graph;
  ScriptPlayableOutput ScriptPlayableOutput;
  AnimationPlayableOutput AnimationPlayableOutput;
  TurntablePreviewGUIElement PreviewRenderUtility;
  Animator PreviewAnimatorInstance;
  ScriptPlayable<AnimationMontagePlayableBehavior> AnimationMontagePlayable;

  void OnDisable() {
    if (Graph.IsValid()) {
      Graph.Destroy();
    }
    if (PreviewRenderUtility != null) {
      PreviewRenderUtility.Cleanup();
      PreviewRenderUtility = null;
    }
    if (SubscribedToEditorUpdate) {
      SubscribedToEditorUpdate = false;
      EditorApplication.update -= Repaint;
    }
  }

  // Any time play mode is entered/exited tear this thing down to prevent garbage
  // from occurring in the preview scene ( Unity does not understand what this scene is )
  // TODO: Is it possible that using their Preview facility would circumvent this issue?
  // It might and that might be preferable overall if so.
  void OnPlayModeStateChange(PlayModeStateChange stateChange) {
    OnDisable();
  }

  // Lazy initialization because OnEnable seems like fucking bullshit in Unity for ScriptableObjects
  public override void OnInspectorGUI() {
    AnimationMontage montage = (AnimationMontage)target;

    // Early out with default editor
    if (EditorApplication.isPlaying) {
      DrawDefaultInspector();
      return;
    }

    serializedObject.Update();

    if (!SubscribedToEditorUpdate) {
      PreviousClockTime = EditorApplication.timeSinceStartup;
      CurrentClockTime = EditorApplication.timeSinceStartup;
      SubscribedToEditorUpdate = true;
      EditorApplication.update += Repaint;
      EditorApplication.playModeStateChanged += OnPlayModeStateChange;
    } else {
      PreviousClockTime = CurrentClockTime;
      CurrentClockTime = EditorApplication.timeSinceStartup;
    }

    if (!Graph.IsValid()) {
      Graph = PlayableGraph.Create("Animation Montage Inspector");
      Graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
      Graph.Play();
      Graph.Evaluate(0);
      ScriptPlayableOutput = ScriptPlayableOutput.Create(Graph, "Script Output");
      AnimationPlayableOutput = AnimationPlayableOutput.Create(Graph, "Animation Output", null);
    }

    if (PreviewRenderUtility == null) {
      PreviewRenderUtility = new();
      SetupPreviewAnimatorInstance(montage.AnimatorPrefab);
      SetupMontagePlayable(montage);
    }

    GUILayout.BeginHorizontal();
    if (IsPlaying) {
      EditorGUILayout.LabelField("Frame", CurrentFrame.ToString());
      if (GUILayout.Button("Pause")) {
        IsPlaying = false;
      }
    } else {
      CurrentFrame = EditorGUILayout.IntField("Frame", CurrentFrame);
      if (GUILayout.Button("Play")) {
        CurrentFrame = 0;
        AnimationMontagePlayable.SetTime(0);
        IsPlaying = true;
      }
    }
    GUILayout.EndHorizontal();

    // Steve - Currently, we have to render timeline first because CurrentFrame potentially changes
    // from its interactions. This is kinda weird though possibly?
    // This should run before the graph is evaluated or you get jitter in the preview window
    GUILayout.Space(10);
    DrawTimeline(montage);
    GUILayout.Space(10);

    if (IsPlaying) {
      Graph.Evaluate((float)DeltaTime);
      var currentMontageTime = (float)AnimationMontagePlayable.GetTime();
      var duration = (float)AnimationMontagePlayable.GetDuration();
      if (currentMontageTime >= duration) {
        var loopingCurrentMontageTime = currentMontageTime % duration;
        AnimationMontagePlayable.SetTime(0);
      }
      CurrentFrame = Mathf.FloorToInt((float)AnimationMontagePlayable.GetTime()/FrameDuration)%montage.FrameDuration;
    } else {
      AnimationMontagePlayable.SetTime(CurrentFrame * FrameDuration);
      Graph.Evaluate(0);
    }

    DrawPreview();
    GUILayout.Space(10);

    EditorGUI.BeginChangeCheck();
    montage.AnimatorPrefab = (Animator)EditorGUILayout.ObjectField("AnimatorPrefab", montage.AnimatorPrefab, typeof(Animator), false);
    if (EditorGUI.EndChangeCheck()) {
      SetupPreviewAnimatorInstance(montage.AnimatorPrefab);
      SetupMontagePlayable(montage);
    }
    EditorGUI.BeginChangeCheck();
    EditorGUILayout.PropertyField(serializedObject.FindProperty("Clips"), true);
    EditorGUILayout.PropertyField(serializedObject.FindProperty("Notifies"), true);
    if (EditorGUI.EndChangeCheck()) {
      SetupMontagePlayable(montage);
    }
    serializedObject.ApplyModifiedProperties();
  }

  void SetupPreviewAnimatorInstance(Animator animatorPrefab) {
    if (PreviewAnimatorInstance) {
      DestroyImmediate(PreviewAnimatorInstance.gameObject);
    }
    if (animatorPrefab) {
      PreviewAnimatorInstance = Instantiate(animatorPrefab);
      PreviewRenderUtility.SetSubject(PreviewAnimatorInstance.gameObject);
      // TODO: Maybe some other object?
      ScriptPlayableOutput.SetUserData(PreviewAnimatorInstance.gameObject);
      AnimationPlayableOutput.SetTarget(PreviewAnimatorInstance);
      PreviewAnimatorInstance.GetComponent<Animator>().applyRootMotion = false;
      if (PreviewAnimatorInstance.TryGetComponent(out AvatarAttacher avatarAttacher)) {
        avatarAttacher.Attach();
      }
    }
  }

  void SetupMontagePlayable(AnimationMontage montage) {
    if (!AnimationMontagePlayable.IsNull()) {
      AnimationMontagePlayable.Destroy();
    }
    AnimationMontagePlayable = montage.CreateScriptPlayable(Graph);
    AnimationMontagePlayable.SetOutputCount(2);
    AnimationPlayableOutput.SetSourcePlayable(AnimationMontagePlayable, 0);
    ScriptPlayableOutput.SetSourcePlayable(AnimationMontagePlayable, 1);
  }

  void DrawPreview() {
    GUILayout.BeginVertical("box");
    Rect previewRect = GUILayoutUtility.GetRect(512, 512, GUILayout.ExpandWidth(true));
    PreviewRenderUtility.Update(previewRect, Event.current);
    GUILayout.EndVertical();
  }

  void DrawTimeline(AnimationMontage montage) {
    Rect header = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, 50);
    EditorGUI.DrawRect(header, new Color(0.05f, 0.05f, 0.05f));
    Rect rect = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, montage.Clips.Count * (TrackHeight + TrackSpacing) + montage.Notifies.Count * (TrackHeight + TrackSpacing) + 100);
    EditorGUI.DrawRect(rect, new Color(0.15f, 0.15f, 0.15f));

    HandleTimelineScrolling(rect);
    DrawFrames(rect);
    float yOffset = rect.y + 50;
    foreach (var clip in montage.Clips) {
      DrawClipRow(rect, clip, yOffset);
      yOffset += TrackHeight + TrackSpacing;
    }
    yOffset += 20; // Space between clips and notifies
    foreach (var notify in montage.Notifies) {
      DrawNotifyRow(rect, notify, yOffset);
      yOffset += TrackHeight + TrackSpacing;
    }
    if (!IsPlaying) {
      HandlePlayheadDragging(header);
    }
    DrawPlayhead(rect);
  }

  void HandleTimelineScrolling(Rect rect) {
    Event e = Event.current;
    if (!rect.Contains(e.mousePosition))
      return;
    if (e.type == EventType.ScrollWheel) {
      VisibleFrames += (int)e.delta.y * 2;
      VisibleFrames = Mathf.Clamp(VisibleFrames, MinVisibleFrames, MaxVisibleFrames);
      e.Use();
    }

    if (e.type == EventType.MouseDrag && e.button == 2) {
      FrameOffset = Mathf.Max(0, FrameOffset - (int)(e.delta.x / 5));
      e.Use();
    }
  }

  void DrawFrames(Rect rect) {
    var mod = VisibleFrames / MinVisibleFrames * 5;
    for (int i = FrameOffset; i <= FrameOffset + VisibleFrames; i++) {
      if (i % mod == 0) {
        float x = rect.x + PaddingLeft + ((i - FrameOffset) / (float)VisibleFrames) * (rect.width - PaddingLeft - PaddingRight);
        Rect lineRect = new Rect(x, rect.y + 30, 1, rect.height - 30);
        EditorGUI.DrawRect(lineRect, new Color(0.3f, 0.3f, 0.3f));

        GUIStyle labelStyle = new GUIStyle(EditorStyles.whiteLabel) { fontSize = 9 };
        GUI.Label(new Rect(x - 10, rect.y + 5, 20, 15), i.ToString(), labelStyle);
      }
    }
  }

  // Playhead drawing
  void DrawPlayhead(Rect rect) {
    if (CurrentFrame >= FrameOffset && CurrentFrame <= FrameOffset + VisibleFrames) {
      float x = rect.x + PaddingLeft + ((CurrentFrame - FrameOffset) / (float)VisibleFrames) * (rect.width - PaddingLeft - PaddingRight);
      Rect playheadRect = new Rect(x - 2, rect.y + 30, 2, rect.height - 30);
      EditorGUI.DrawRect(playheadRect, Color.white);
      GUIStyle labelStyle = new GUIStyle(EditorStyles.whiteLabel) { fontSize = 16 };
      GUI.Label(new Rect(x - 10, rect.y - 20, 60, 40), CurrentFrame.ToString(), labelStyle);
    }
  }

  // Playhead dragging logic
  void HandlePlayheadDragging(Rect rect) {
    var e = Event.current;
    if (rect.Contains(e.mousePosition) && (e.type == EventType.MouseDown || e.type == EventType.MouseDrag) && (e.button == 0 || e.button == 1 || e.button == 2)) {
      float clickedFrame = FrameOffset + Mathf.RoundToInt((e.mousePosition.x - rect.x - PaddingLeft) / (rect.width - PaddingLeft - PaddingRight) * VisibleFrames);
      CurrentFrame = Mathf.Clamp((int)clickedFrame, 0, int.MaxValue);
      e.Use();
    }
  }

  void DrawClipRow(Rect rect, AnimationMontageClip clip, float yOffset) {
    float PixelX(float frameX) => rect.x + PaddingLeft + (frameX / (float)VisibleFrames) * (rect.width - PaddingLeft - PaddingRight);
    float TrackX(int frame) => Mathf.Clamp(frame - FrameOffset, 0, FrameOffset + VisibleFrames);

    Rect trackRect;
    Rect clipRect;
    {
      trackRect = new Rect(rect.x + PaddingLeft, yOffset, rect.width - PaddingLeft - PaddingRight, TrackHeight);
      EditorGUI.DrawRect(trackRect, new Color(0.2f, 0.2f, 0.2f));
    }
    {
      float x0 = TrackX(clip.StartFrame);
      float x1 = TrackX(clip.EndFrame);
      float startX = PixelX(x0);
      float endX = PixelX(x1);
      clipRect = new Rect(startX, yOffset, Mathf.Max(2, endX - startX), TrackHeight);
      EditorGUI.DrawRect(clipRect, new Color(0.5f, 0.7f, 0.9f));
    }
    if (clip.FadeInFrames > 0) {
      float x0 = TrackX(clip.StartFrame);
      float x1 = TrackX(clip.StartFrame+clip.FadeInFrames);
      float startX = PixelX(x0);
      float endX = PixelX(x1);
      Rect fadeInRect = new Rect(startX, yOffset, Mathf.Max(2, endX-startX), 2);
      EditorGUI.DrawRect(fadeInRect, Color.white);
    }
    if (clip.FadeOutFrames > 0) {
      float x0 = TrackX(clip.EndFrame-clip.FadeOutFrames);
      float x1 = TrackX(clip.EndFrame);
      float startX = PixelX(x0);
      float endX = PixelX(x1);
      Rect fadeOutRect = new Rect(startX, yOffset, Mathf.Max(2, endX-startX), 2);
      EditorGUI.DrawRect(fadeOutRect, Color.white);
    }

    GUIStyle labelStyle = new GUIStyle(EditorStyles.whiteLabel) { normal = { textColor = Color.black } };
    GUI.Label(clipRect, clip.AnimationClip ? clip.AnimationClip.name : "No Clip", labelStyle);
    HandleMouseInput(trackRect, clip, false);
  }

  void DrawNotifyRow(Rect rect, AnimationNotify notify, float yOffset) {
    Rect rowRect = new Rect(rect.x + PaddingLeft, yOffset, rect.width - PaddingLeft - PaddingRight, TrackHeight);
    EditorGUI.DrawRect(rowRect, new Color(0.25f, 0.25f, 0.25f));

    float x0 = Mathf.Clamp(notify.StartFrame - FrameOffset, 0, FrameOffset + VisibleFrames);
    float x1 = Mathf.Clamp(notify.EndFrame - FrameOffset, 0, FrameOffset + VisibleFrames);
    float startX = rect.x + PaddingLeft + (x0 / (float)VisibleFrames) * (rect.width - PaddingLeft - PaddingRight);
    float endX = rect.x + PaddingLeft + (x1 / (float)VisibleFrames) * (rect.width - PaddingLeft - PaddingRight);
    Rect trackRect = new Rect(startX, yOffset, Mathf.Max(2, endX - startX), TrackHeight);
    EditorGUI.DrawRect(trackRect, new Color(0.8f, 0.5f, 0.5f));

    GUIStyle labelStyle = new GUIStyle(EditorStyles.whiteLabel) { normal = { textColor = Color.black } };
    GUI.Label(trackRect, notify.Name, labelStyle);
    HandleMouseInput(rowRect, notify, true);
  }

  void HandleMouseInput(Rect trackRect, object item, bool isNotify) {
    Event e = Event.current;
    if (trackRect.Contains(e.mousePosition)) {
      if (e.type == EventType.MouseDown) {
        float clickedFrame = Mathf.Clamp(
          FrameOffset + Mathf.RoundToInt((e.mousePosition.x - trackRect.x) / trackRect.width * VisibleFrames),
          0, int.MaxValue
        );

        if (item is AnimationMontageClip clip) {
          if (e.button == 0) clip.StartFrame = (int)clickedFrame;
          if (e.button == 1) clip.StartFrame = Mathf.Max(0, (int)clickedFrame - clip.Duration);
        } else if (item is AnimationNotify notify) {
          if (e.button == 0) {
            notify.StartFrame = (int)clickedFrame;
            notify.EndFrame = Mathf.Max(notify.StartFrame + 1, notify.EndFrame);
          }
          if (e.button == 1) {
            notify.EndFrame = (int)clickedFrame;
            notify.StartFrame = Mathf.Min(notify.EndFrame - 1, notify.StartFrame);
          }
        }

        e.Use();
      }
    }
  }
}