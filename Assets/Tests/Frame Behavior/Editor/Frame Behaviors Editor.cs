using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System;
using System.Linq;
using System.Collections.Generic;

[CustomEditor(typeof(FrameBehaviors))]
public class FrameBehaviorsEditor : Editor {
  VisualElement Root;
  VisualElement Timeline;
  Button AddNewButton;
  SerializedProperty BehaviorsProperty;

  IEnumerable<FrameBehaviorRoot> FrameBehaviorRoots =>
    Timeline
    .Children()
    .Select(child => child as TimelineRow)
    .Select(row => row.FrameBehaviorRoot);

  public override VisualElement CreateInspectorGUI() {
    BehaviorsProperty = serializedObject.FindProperty("Behaviors");
    Root = new VisualElement();
    Timeline = new VisualElement();
    Timeline.name = "TimelineContainer";
    AddNewButton = new Button(AddNewBehavior);
    AddNewButton.text = "Add New Behavior";
    var endFrameProperty = serializedObject.FindProperty("EndFrame");
    var endFrameField = new PropertyField(endFrameProperty);
    var frameField = new IntegerField("Frame");
    BuildTimeline();
    Root.Add(endFrameField);
    Root.Add(frameField);
    Root.Add(Timeline);
    Root.Add(AddNewButton);
    FrameBehaviorRoots.ForEach(r => r.SetFrame(0));
    FrameBehaviorRoots.ForEach(r => r.SetMaxFrame(endFrameProperty.intValue));
    frameField.RegisterValueChangedCallback(UpdateFrame);
    endFrameField.RegisterValueChangeCallback(UpdateMaxFrame);
    return Root;
  }

  void BuildTimeline() {
    Timeline.Clear();
    serializedObject.Update();
    for (int i = 0; i < BehaviorsProperty.arraySize; i++) {
      SerializedProperty behaviorProp = BehaviorsProperty.GetArrayElementAtIndex(i);
      TimelineRow row = new TimelineRow(behaviorProp, DeleteBehavior);
      Timeline.Add(row);
    }
  }

  void UpdateMaxFrame(SerializedPropertyChangeEvent evt) {
    FrameBehaviorRoots.ForEach(r => r.SetMaxFrame(evt.changedProperty.intValue));
  }

  void UpdateFrame(ChangeEvent<int> evt) {
    FrameBehaviorRoots.ForEach(r => r.SetFrame(evt.newValue));
  }

  void DeleteBehavior(int index) {
    serializedObject.Update();
    if (index < 0 || index >= BehaviorsProperty.arraySize)
      return;
    BehaviorsProperty.DeleteArrayElementAtIndex(index);
    Timeline.RemoveAt(index);
    serializedObject.ApplyModifiedProperties();
    serializedObject.Update();
    BehaviorsProperty = serializedObject.FindProperty("Behaviors");
    for (int i = index; i < Timeline.childCount; i++) {
      var row = Timeline.ElementAt(i) as TimelineRow;
      var serializedProperty = BehaviorsProperty.GetArrayElementAtIndex(i);
      Debug.Log($"Trying to bind row {i} to property {serializedProperty.propertyPath}");
      row.FrameBehaviorRoot.BindProperty(serializedProperty);
    }
  }

  void AddNewBehavior() {
    serializedObject.Update();
    AnimatorControllerCrossFadeBehavior newBehavior = new AnimatorControllerCrossFadeBehavior();
    int newIndex = BehaviorsProperty.arraySize;
    BehaviorsProperty.AddManagedReferenceToList(newBehavior, newIndex);
    SerializedProperty newProp = BehaviorsProperty.GetArrayElementAtIndex(newIndex);
    TimelineRow newRow = new TimelineRow(newProp, DeleteBehavior);
    Timeline.Add(newRow);
    serializedObject.ApplyModifiedProperties();
  }
}

class TimelineRow : VisualElement {
  public FrameBehaviorRoot FrameBehaviorRoot;
  public Button DeleteButton;

  public TimelineRow(SerializedProperty property, Action<int> onDelete) {
    style.flexDirection = FlexDirection.Row;
    style.alignItems = Align.Center;
    FrameBehaviorRoot = new FrameBehaviorRoot();
    FrameBehaviorRoot.BindProperty(property);
    FrameBehaviorRoot.style.flexGrow = 1;
    DeleteButton = new Button(() => {
      int index = parent.IndexOf(this);
      onDelete?.Invoke(index);
    });
    DeleteButton.text = "X";
    DeleteButton.style.flexShrink = 0;
    Add(FrameBehaviorRoot);
    Add(DeleteButton);
  }
}