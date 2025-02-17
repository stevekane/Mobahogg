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
  double CurrentTime = 0;
  bool IsPlaying;

  PlayableGraph Graph;
  ScriptPlayableOutput ScriptPlayableOutput;
  AnimationPlayableOutput AnimationPlayableOutput;
  TurntablePreviewGUIElement PreviewRenderUtility;
  Animator PreviewAnimatorInstance;
  ScriptPlayable<AnimationMontagePlayableBehavior> AnimationMontagePlayable;

  bool Initialized => Graph.IsValid() && PreviewRenderUtility != null;

  void OnDisable() {
    if (Graph.IsValid()) {
      Graph.Destroy();
    }
    if (PreviewAnimatorInstance) {
      DestroyImmediate(PreviewAnimatorInstance.gameObject);
    }
    if (PreviewRenderUtility != null) {
      PreviewRenderUtility.Cleanup();
      PreviewRenderUtility = null;
    }
    EditorApplication.update -= Repaint;
  }

  void InitializeClock() {
    PreviousClockTime = EditorApplication.timeSinceStartup;
    CurrentClockTime = EditorApplication.timeSinceStartup;
  }

  void TickClock() {
    PreviousClockTime = CurrentClockTime;
    CurrentClockTime = EditorApplication.timeSinceStartup;
  }

  public override void OnInspectorGUI() {
    AnimationMontage montage = (AnimationMontage)target;
    // Steve - Do not fuck with this lazy-initialization. OnEnable/OnDisable are worthless in Editors
    if (!Initialized) {
      PreviewRenderUtility = new();
      Graph = PlayableGraph.Create("Animation Montage Inspector");
      ScriptPlayableOutput = ScriptPlayableOutput.Create(Graph, "Script Output");
      AnimationPlayableOutput = AnimationPlayableOutput.Create(Graph, "Animation Output", null);
      InitializeClock();
      SetupMontagePlayable(montage);
      SetupPreviewAnimatorInstance(montage.AnimatorPrefab);
      Graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
      Graph.Play();
      Graph.Evaluate();
      EditorApplication.update -= Repaint;
      EditorApplication.update += Repaint;
    }

    serializedObject.Update();
    TickClock();
    DrawPlayStateButtons();
    DrawTimeline(montage); // Draw before stuff that affects Current Frame
    UpdatePreview(montage);
    DrawPreview();
    EditorGUI.BeginChangeCheck();
    EditorGUILayout.PropertyField(serializedObject.FindProperty("AnimatorPrefab"));
    if (EditorGUI.EndChangeCheck()) {
      var newAnimator = (Animator)serializedObject.FindProperty("AnimatorPrefab").objectReferenceValue;
      SetupPreviewAnimatorInstance(newAnimator);
    }
    EditorGUILayout.PropertyField(serializedObject.FindProperty("Clips"));
    EditorGUILayout.PropertyField(serializedObject.FindProperty("Notifies"));
    if (serializedObject.hasModifiedProperties) {
      SetupMontagePlayable(montage);
      serializedObject.ApplyModifiedProperties();
    }
  }

  void SetupPreviewAnimatorInstance(Animator animatorPrefab) {
    if (PreviewAnimatorInstance) {
      DestroyImmediate(PreviewAnimatorInstance.gameObject);
      PreviewRenderUtility.SetSubject(null);
    }
    if (animatorPrefab) {
      PreviewAnimatorInstance = Instantiate(animatorPrefab);
      PreviewAnimatorInstance.GetComponent<Animator>().applyRootMotion = false;
      PreviewRenderUtility.SetSubject(PreviewAnimatorInstance.gameObject);
      AnimationPlayableOutput.SetTarget(PreviewAnimatorInstance);
      if (PreviewAnimatorInstance.TryGetComponent(out AvatarAttacher avatarAttacher)) {
        avatarAttacher.Attach();
      }
    }
  }

  void SetupMontagePlayable(AnimationMontage montage) {
    AnimationPlayableOutput.SetSourcePlayable(AnimationMontagePlayable, 0);
    if (AnimationMontagePlayable.IsValid()) {
      AnimationMontagePlayable.Destroy();
    }
    AnimationMontagePlayable = montage.CreateScriptPlayable(Graph);
    AnimationMontagePlayable.SetOutputCount(2);
    AnimationPlayableOutput.SetSourcePlayable(AnimationMontagePlayable, 0);
    ScriptPlayableOutput.SetSourcePlayable(AnimationMontagePlayable, 1);
  }

  void UpdatePreview(AnimationMontage montage) {
    if (IsPlaying) {
      CurrentTime += DeltaTime;
      CurrentTime = CurrentTime % AnimationMontagePlayable.GetDuration();
      CurrentFrame = Mathf.FloorToInt((float)CurrentTime/FrameDuration)%montage.FrameDuration;
      AnimationMontagePlayable.SetTime(CurrentTime);
      Graph.Evaluate((float)DeltaTime);
    } else {
      AnimationMontagePlayable.SetTime(CurrentTime);
      Graph.Evaluate();
    }
  }

  void DrawPreview() {
    GUILayout.BeginVertical("box");
    Rect previewRect = GUILayoutUtility.GetRect(512, 512, GUILayout.ExpandWidth(true));
    PreviewRenderUtility.Update(previewRect, Event.current);
    GUILayout.EndVertical();
  }

  void DrawPlayStateButtons() {
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
  }

  void DrawTimeline(AnimationMontage montage) {
    var clips = serializedObject.FindProperty("Clips");
    var notifies = serializedObject.FindProperty("Notifies");
    var clipCount = clips.arraySize;
    var notifyCount = notifies.arraySize;
    GUILayout.Space(10);
    Rect header = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, 50);
    EditorGUI.DrawRect(header, new Color(0.05f, 0.05f, 0.05f));
    Rect rect = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, clipCount * (TrackHeight + TrackSpacing) + notifyCount * (TrackHeight + TrackSpacing) + 100);
    EditorGUI.DrawRect(rect, new Color(0.15f, 0.15f, 0.15f));

    HandleTimelineScrolling(rect);
    DrawFrames(rect);
    float yOffset = rect.y + 50;
    for (var i = 0; i < clipCount; i++) {
      DrawClipRow(rect, montage.Clips[i], clips.GetArrayElementAtIndex(i), yOffset);
      yOffset += TrackHeight + TrackSpacing;
    }
    yOffset += 20; // Space between clips and notifies
    for (var i = 0; i < notifyCount; i++) {
      DrawNotifyRow(rect, montage.Notifies[i], notifies.GetArrayElementAtIndex(i), yOffset);
      yOffset += TrackHeight + TrackSpacing;
    }
    if (!IsPlaying) {
      HandlePlayheadDragging(header);
    }
    DrawPlayhead(rect);
    GUILayout.Space(10);
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
      CurrentTime = CurrentFrame / 60d;
      e.Use();
    }
  }

  void DrawClipRow(Rect rect, AnimationMontageClip montageClip, SerializedProperty serializedProperty, float yOffset) {
    float PixelX(float frameX) => rect.x + PaddingLeft + (frameX / (float)VisibleFrames) * (rect.width - PaddingLeft - PaddingRight);
    float TrackX(int frame) => Mathf.Clamp(frame - FrameOffset, 0, FrameOffset + VisibleFrames);
    int startFrame = serializedProperty.FindPropertyRelative("StartFrame").intValue;
    int fadeInFrames = serializedProperty.FindPropertyRelative("FadeInFrames").intValue;
    int fadeOutFrames = serializedProperty.FindPropertyRelative("FadeOutFrames").intValue;
    int endFrame = montageClip.EndFrame;
    AnimationClip animationClip = montageClip.AnimationClip;

    Rect trackRect;
    Rect clipRect;
    {
      trackRect = new Rect(rect.x + PaddingLeft, yOffset, rect.width - PaddingLeft - PaddingRight, TrackHeight);
      EditorGUI.DrawRect(trackRect, new Color(0.2f, 0.2f, 0.2f));
    }
    {
      float x0 = TrackX(startFrame);
      float x1 = TrackX(endFrame);
      float startX = PixelX(x0);
      float endX = PixelX(x1);
      clipRect = new Rect(startX, yOffset, Mathf.Max(2, endX - startX), TrackHeight);
      EditorGUI.DrawRect(clipRect, new Color(0.5f, 0.7f, 0.9f));
    }
    if (fadeInFrames > 0) {
      float x0 = TrackX(startFrame);
      float x1 = TrackX(startFrame+fadeInFrames);
      float startX = PixelX(x0);
      float endX = PixelX(x1);
      Rect fadeInRect = new Rect(startX, yOffset, Mathf.Max(2, endX-startX), 2);
      EditorGUI.DrawRect(fadeInRect, Color.white);
    }
    if (fadeOutFrames > 0) {
      float x0 = TrackX(endFrame-fadeOutFrames);
      float x1 = TrackX(endFrame);
      float startX = PixelX(x0);
      float endX = PixelX(x1);
      Rect fadeOutRect = new Rect(startX, yOffset, Mathf.Max(2, endX-startX), 2);
      EditorGUI.DrawRect(fadeOutRect, Color.white);
    }

    GUIStyle labelStyle = new GUIStyle(EditorStyles.whiteLabel) { normal = { textColor = Color.black } };
    GUI.Label(clipRect, animationClip ? $"{animationClip.name}({montageClip.Speed}x)": "No Clip", labelStyle);
    HandleMouseInput(trackRect, serializedProperty, false);
  }

  void DrawNotifyRow(Rect rect, AnimationNotify notify, SerializedProperty serializedProperty, float yOffset) {
    string name = serializedProperty.FindPropertyRelative("Name").stringValue;
    int startFrame = serializedProperty.FindPropertyRelative("StartFrame").intValue;
    int endFrame = notify.EndFrame;
    Rect rowRect = new Rect(rect.x + PaddingLeft, yOffset, rect.width - PaddingLeft - PaddingRight, TrackHeight);
    EditorGUI.DrawRect(rowRect, new Color(0.25f, 0.25f, 0.25f));

    float x0 = Mathf.Clamp(startFrame - FrameOffset, 0, FrameOffset + VisibleFrames);
    float x1 = Mathf.Clamp(endFrame - FrameOffset, 0, FrameOffset + VisibleFrames);
    float startX = rect.x + PaddingLeft + (x0 / (float)VisibleFrames) * (rect.width - PaddingLeft - PaddingRight);
    float endX = rect.x + PaddingLeft + (x1 / (float)VisibleFrames) * (rect.width - PaddingLeft - PaddingRight);
    Rect trackRect = new Rect(startX, yOffset, Mathf.Max(2, endX - startX), TrackHeight);
    EditorGUI.DrawRect(trackRect, new Color(0.8f, 0.5f, 0.5f));

    GUIStyle labelStyle = new GUIStyle(EditorStyles.whiteLabel) { normal = { textColor = Color.black } };
    GUI.Label(trackRect, name, labelStyle);
    HandleMouseInput(rowRect, serializedProperty, true);
  }

  void HandleMouseInput(Rect trackRect, SerializedProperty property, bool isNotify) {
    var startFrame = property.FindPropertyRelative("StartFrame");
    // Technically, this is dubious.
    // EndFrame is only a property on notifies and is a getter on clips ( and therefore not serialized )
    // Duration is a getter on clips only and therefore is not serialized
    // It happens to be the case that this isn't used here in a way that throws errors but it could
    // lead to unexpected behavior or confusion to a future reader
    var endFrame = property.FindPropertyRelative("EndFrame");
    var duration = property.FindPropertyRelative("Duration");
    Event e = Event.current;
    if (trackRect.Contains(e.mousePosition)) {
      if (e.type == EventType.MouseDown) {
        float clickedFrame = Mathf.Clamp(
          FrameOffset + Mathf.RoundToInt((e.mousePosition.x - trackRect.x) / trackRect.width * VisibleFrames),
          0, int.MaxValue
        );

        if (!isNotify) {
          if (e.button == 0) {
            startFrame.intValue = (int)clickedFrame;
          }
          if (e.button == 1) {
            startFrame.intValue = Mathf.Max(0, (int)clickedFrame - duration.intValue);
          }
        } else {
          if (e.button == 0) {
            startFrame.intValue = (int)clickedFrame;
            endFrame.intValue = Mathf.Max(startFrame.intValue + 1, endFrame.intValue);
          }
          if (e.button == 1) {
            endFrame.intValue = (int)clickedFrame;
            startFrame.intValue = Mathf.Min(endFrame.intValue - 1, startFrame.intValue);
          }
        }

        e.Use();
      }
    }
  }
}