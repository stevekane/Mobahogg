using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;

[CustomEditor(typeof(FrameBehaviors))]
public class FrameBehaviorsEditor : Editor {
  FrameBehaviorsPreviewScene PreviewScene;
  PropertyField PreviewPrefabField;
  FrameSlider FrameSlider;
  PropertyField EndFrameField;
  PolymorphicList<FrameBehavior, FrameBehaviorRoot> FrameBehaviors;
  int Frame;
  int EndFrame;

  MonoBehaviour PreviewProvider =>
    ((FrameBehaviors)target).PreviewPrefab;

  IEnumerable<FrameBehavior> Behaviors =>
    ((FrameBehaviors)target).Behaviors;

  public override VisualElement CreateInspectorGUI() {
    var container = new VisualElement();
    PreviewPrefabField = new PropertyField(serializedObject.FindProperty("PreviewPrefab"));
    PreviewPrefabField.RegisterCallback<SerializedPropertyChangeEvent>(OnPrefabChange);
    EndFrameField = new PropertyField(serializedObject.FindProperty("EndFrame"));
    EndFrameField.RegisterCallback<SerializedPropertyChangeEvent>(OnEndFrameChange);
    FrameSlider = new FrameSlider();
    FrameSlider.RegisterCallback<ChangeEvent<int>>(OnFrameChange);
    FrameSlider.Frame = Frame;
    FrameSlider.EndFrame = EndFrame;
    FrameBehaviors = new PolymorphicList<FrameBehavior, FrameBehaviorRoot>();
    FrameBehaviors.BindProperty(serializedObject.FindProperty("Behaviors"));
    FrameBehaviors.OnChange.Listen(OnListChange);
    PreviewScene = new FrameBehaviorsPreviewScene();
    PreviewScene.SetProvider(PreviewProvider);
    PreviewScene.SetFrameBehaviors(Behaviors);
    PreviewScene.Seek(Frame);
    container.TrackSerializedObjectValue(serializedObject, OnAnyBehaviorChange);
    container.Add(PreviewPrefabField);
    container.Add(PreviewScene);
    container.Add(EndFrameField);
    container.Add(FrameSlider);
    container.Add(FrameBehaviors);
    container.Bind(serializedObject);
    return container;
  }

  void OnListChange() {
    foreach (var behavior in FrameBehaviors.Elements) {
      SetFrame(behavior, Frame);
      SetEndFrame(behavior, EndFrame);
    }
  }

  void OnAnyBehaviorChange(SerializedObject so) {
    PreviewScene.SetFrameBehaviors(Behaviors);
  }

  void OnFrameChange(ChangeEvent<int> changeEvent) {
    Frame = Mathf.Clamp(changeEvent.newValue, 0, EndFrame);
    FrameSlider.Frame = Frame;
    foreach (var behavior in FrameBehaviors.Elements) {
      SetFrame(behavior, changeEvent.newValue);
    }
    PreviewScene.Seek(Frame);
  }

  void OnPrefabChange(SerializedPropertyChangeEvent evt) {
    PreviewScene.SetProvider(PreviewProvider);
  }

  void OnEndFrameChange(SerializedPropertyChangeEvent evt) {
    EndFrame = evt.changedProperty.intValue;
    Frame = Mathf.Clamp(Frame, 0, EndFrame);
    FrameSlider.EndFrame = EndFrame;
    foreach (var behavior in FrameBehaviors.Elements) {
      SetEndFrame(behavior, evt.changedProperty.intValue);
    }
    PreviewScene.Seek(Frame);
  }

  void SetFrame(FrameBehaviorRoot frameBehavior, int frame) {
    frameBehavior.SetFrame(frame);
  }

  void SetEndFrame(FrameBehaviorRoot frameBehavior, int endFrame) {
    frameBehavior.SetMaxFrame(endFrame);
  }
}