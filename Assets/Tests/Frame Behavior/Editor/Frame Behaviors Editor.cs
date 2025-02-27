using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;

[CustomEditor(typeof(FrameBehaviors))]
public class FrameBehaviorsEditor : Editor {
  FrameBehaviorsPreviewScene PreviewScene;
  PropertyField PreviewPrefabField;
  IntegerField FrameField;
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
    PreviewScene = new FrameBehaviorsPreviewScene();
    FrameField = new IntegerField("Frame");
    FrameField.RegisterCallback<ChangeEvent<int>>(OnFrameChange);
    EndFrameField = new PropertyField(serializedObject.FindProperty("EndFrame"));
    EndFrameField.RegisterCallback<SerializedPropertyChangeEvent>(OnEndFrameChange);
    FrameBehaviors = new PolymorphicList<FrameBehavior, FrameBehaviorRoot>();
    FrameBehaviors.BindProperty(serializedObject.FindProperty("Behaviors"));
    FrameBehaviors.OnChange.Listen(OnListChange);
    PreviewScene.Seek(Frame, Behaviors, PreviewProvider);
    container.TrackSerializedObjectValue(serializedObject, (so) => {
      PreviewScene.Seek(Frame, Behaviors, PreviewProvider);
    });
    container.Add(PreviewPrefabField);
    container.Add(PreviewScene);
    container.Add(FrameField);
    container.Add(EndFrameField);
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

  void OnFrameChange(ChangeEvent<int> changeEvent) {
    Frame = Mathf.Clamp(changeEvent.newValue, 0, EndFrame);
    FrameField.value = Frame;
    foreach (var behavior in FrameBehaviors.Elements) {
      SetFrame(behavior, changeEvent.newValue);
    }
    PreviewScene.Seek(Frame, Behaviors, PreviewProvider);
  }

  void OnEndFrameChange(SerializedPropertyChangeEvent evt) {
    EndFrame = evt.changedProperty.intValue;
    Frame = Mathf.Clamp(Frame, 0, EndFrame);
    FrameField.value = Frame;
    foreach (var behavior in FrameBehaviors.Elements) {
      SetEndFrame(behavior, evt.changedProperty.intValue);
    }
    PreviewScene.Seek(Frame, Behaviors, PreviewProvider);
  }

  void SetFrame(FrameBehaviorRoot frameBehavior, int frame) {
    frameBehavior.SetFrame(frame);
  }

  void SetEndFrame(FrameBehaviorRoot frameBehavior, int endFrame) {
    frameBehavior.SetMaxFrame(endFrame);
  }
}