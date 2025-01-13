using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AnimationMontage))]
public class AnimationMontageInspector : Editor {
  const float TrackHeight = 20f;
  const float TrackSpacing = 10f;
  const float PaddingLeft = 60f;
  const float PaddingRight = 20f;

  int VisibleFrames = 60;
  int frameOffset = 0;

  public override void OnInspectorGUI() {
    AnimationMontage montage = (AnimationMontage)target;
    serializedObject.Update();

    GUILayout.Space(10);

    // Draw preview area
    Rect previewRect = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, 100);
    EditorGUI.DrawRect(previewRect, Color.grey);
    GUI.Label(previewRect, "Preview", new GUIStyle(EditorStyles.whiteLabel) { alignment = TextAnchor.MiddleCenter });

    GUILayout.Space(10);

    DrawTimeline(montage);

    GUILayout.Space(10);

    EditorGUILayout.PropertyField(serializedObject.FindProperty("Clips"), true);
    EditorGUILayout.PropertyField(serializedObject.FindProperty("Notifies"), true);

    serializedObject.ApplyModifiedProperties();
  }

  void DrawTimeline(AnimationMontage montage) {
    Rect rect = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, montage.Clips.Count * (TrackHeight + TrackSpacing) + montage.Notifies.Count * (TrackHeight + TrackSpacing) + 100);
    EditorGUI.DrawRect(rect, new Color(0.15f, 0.15f, 0.15f));

    HandleTimelineScrolling();
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
  }

  void HandleTimelineScrolling() {
    Event e = Event.current;
    if (e.type == EventType.ScrollWheel) {
      VisibleFrames += (int)e.delta.y*2;
      VisibleFrames = Mathf.Clamp(VisibleFrames, 5, 240);
      e.Use();
    }

    if (e.type == EventType.MouseDrag && e.button == 2) {
      frameOffset = Mathf.Max(0, frameOffset - (int)(e.delta.x / 5));
      e.Use();
    }
  }

  void DrawFrames(Rect rect) {
    var mod = VisibleFrames >= 120 ? 10 : 5;
    for (int i = frameOffset; i <= frameOffset + VisibleFrames; i++) {
      if (i % mod == 0) {
        float x = rect.x + PaddingLeft + ((i - frameOffset) / (float)VisibleFrames) * (rect.width - PaddingLeft - PaddingRight);
        Rect lineRect = new Rect(x, rect.y + 30, 1, rect.height - 30);
        EditorGUI.DrawRect(lineRect, new Color(0.3f, 0.3f, 0.3f));

        GUIStyle labelStyle = new GUIStyle(EditorStyles.whiteLabel) { fontSize = 9 };
        GUI.Label(new Rect(x - 10, rect.y + 5, 20, 15), i.ToString(), labelStyle);
      }
    }
  }

  void DrawClipRow(Rect rect, AnimationMontageClip clip, float yOffset) {
    Rect rowRect = new Rect(rect.x + PaddingLeft, yOffset, rect.width - PaddingLeft - PaddingRight, TrackHeight);
    EditorGUI.DrawRect(rowRect, new Color(0.2f, 0.2f, 0.2f));

    float x0 = Mathf.Clamp(clip.StartFrame-frameOffset, 0, frameOffset+VisibleFrames);
    float x1 = Mathf.Clamp(clip.EndFrame - frameOffset, 0, frameOffset+VisibleFrames);
    float startX = rect.x + PaddingLeft + (x0 / (float)VisibleFrames) * (rect.width - PaddingLeft - PaddingRight);
    float endX = rect.x + PaddingLeft + (x1 / (float)VisibleFrames) * (rect.width - PaddingLeft - PaddingRight);
    Rect trackRect = new Rect(startX, yOffset, Mathf.Max(2, endX - startX), TrackHeight);
    EditorGUI.DrawRect(trackRect, new Color(0.5f, 0.7f, 0.9f));

    GUI.Label(trackRect, clip.AnimationClip ? clip.AnimationClip.name : "No Clip", EditorStyles.whiteLabel);
    HandleMouseInput(rowRect, clip);
  }

  void DrawNotifyRow(Rect rect, AnimationNotify notify, float yOffset) {
    Rect rowRect = new Rect(rect.x + PaddingLeft, yOffset, rect.width - PaddingLeft - PaddingRight, TrackHeight);
    EditorGUI.DrawRect(rowRect, new Color(0.25f, 0.25f, 0.25f));

    float x0 = Mathf.Clamp(notify.StartFrame-frameOffset, 0, frameOffset+VisibleFrames);
    float x1 = Mathf.Clamp(notify.EndFrame - frameOffset, 0, frameOffset+VisibleFrames);
    float startX = rect.x + PaddingLeft + (x0 / (float)VisibleFrames) * (rect.width - PaddingLeft - PaddingRight);
    float endX = rect.x + PaddingLeft + (x1 / (float)VisibleFrames) * (rect.width - PaddingLeft - PaddingRight);
    Rect trackRect = new Rect(startX, yOffset, Mathf.Max(2, endX - startX), TrackHeight);
    EditorGUI.DrawRect(trackRect, new Color(0.8f, 0.5f, 0.5f));

    GUI.Label(trackRect, notify.Name, EditorStyles.whiteLabel);
    HandleMouseInput(rowRect, notify);
  }

  void HandleMouseInput(Rect rowRect, object item) {
    Event e = Event.current;
    if (rowRect.Contains(e.mousePosition)) {
      if (e.type == EventType.MouseDown) {
        float clickedFrame = Mathf.Clamp(
          frameOffset + Mathf.RoundToInt((e.mousePosition.x - rowRect.x) / rowRect.width * VisibleFrames),
          0, int.MaxValue
        );

        if (item is AnimationMontageClip clip) {
          if (e.button == 0) clip.StartFrame = (int)clickedFrame;
          if (e.button == 1) clip.StartFrame = Mathf.Max(0, (int)clickedFrame - clip.Duration);
        } else if (item is AnimationNotify notify) {
          if (e.button == 0) {
            notify.StartFrame = (int)clickedFrame;
            notify.EndFrame = Mathf.Max(notify.StartFrame+1, notify.EndFrame);
          }
          if (e.button == 1) {
            notify.EndFrame = (int)clickedFrame;
            notify.StartFrame = Mathf.Min(notify.EndFrame-1, notify.StartFrame);
          }
        }

        e.Use();
      }
    }
  }
}
