using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

[CustomEditor(typeof(FrameBehaviors))]
public class FrameBehaviorsEditor : Editor {
  IntegerField FrameField;
  PropertyField EndFrameField;
  PolymorphicList<FrameBehavior, FrameBehaviorRoot> FrameBehaviors;
  int Frame;
  int EndFrame;

  public override VisualElement CreateInspectorGUI() {
    var container = new VisualElement();
    FrameField = new IntegerField("Frame");
    FrameField.RegisterCallback<ChangeEvent<int>>(OnFrameChange);
    EndFrameField = new PropertyField(serializedObject.FindProperty("EndFrame"));
    EndFrameField.RegisterCallback<SerializedPropertyChangeEvent>(OnEndFrameChange);
    FrameBehaviors = new PolymorphicList<FrameBehavior, FrameBehaviorRoot>();
    FrameBehaviors.BindProperty(serializedObject.FindProperty("Behaviors"));
    // I don't love this... because technically you would prefer to remove this
    // to prevent any kind of lingering reference...
    FrameBehaviors.OnChange.Listen(OnListChange);
    container.Add(FrameField);
    container.Add(EndFrameField);
    container.Add(FrameBehaviors);
    return container;
  }

  void OnListChange() {
    foreach (var behavior in FrameBehaviors.Elements) {
      SetFrame(behavior, Frame);
      SetEndFrame(behavior, EndFrame);
    }
  }

  void OnFrameChange(ChangeEvent<int> changeEvent) {
    Frame = changeEvent.newValue;
    foreach (var behavior in FrameBehaviors.Elements) {
      SetFrame(behavior, changeEvent.newValue);
    }
  }

  void OnEndFrameChange(SerializedPropertyChangeEvent evt) {
    EndFrame = evt.changedProperty.intValue;
    foreach (var behavior in FrameBehaviors.Elements) {
      SetEndFrame(behavior, evt.changedProperty.intValue);
    }
  }

  void SetFrame(FrameBehaviorRoot frameBehavior, int frame) {
    frameBehavior.SetFrame(frame);
  }

  void SetEndFrame(FrameBehaviorRoot frameBehavior, int endFrame) {
    frameBehavior.SetMaxFrame(endFrame);
  }
}