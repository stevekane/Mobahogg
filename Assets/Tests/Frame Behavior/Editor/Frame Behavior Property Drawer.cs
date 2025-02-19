using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(FrameBehavior), true)]
public class FrameBehaviorDrawer : PropertyDrawer {
  public override VisualElement CreatePropertyGUI(SerializedProperty property) {
    var root = new VisualElement();
    var maxFrames = 60;
    var attrs = fieldInfo.GetCustomAttributes(typeof(MaxFramesAttribute), true) as MaxFramesAttribute[];
    var maxFramesProperty = attrs != null && attrs.Length > 0
      ? property.serializedObject.FindProperty(attrs[0].MaxFramePropertyName)
      : null;
    root.style.marginBottom = 8;
    root.style.flexDirection = FlexDirection.Column;
    var row = new VisualElement();
    row.style.flexDirection = FlexDirection.Row;
    row.style.alignItems = Align.Center;
    var prefix = new Label(property.displayName);
    prefix.style.width = 180;
    var timelineContainer = new VisualElement();
    timelineContainer.style.flexGrow = 1;
    timelineContainer.style.position = Position.Relative;
    timelineContainer.style.height = 20;
    timelineContainer.style.backgroundColor = new Color(0.1f, 0.1f, 0.1f);
    var clip = new VisualElement();
    clip.style.position = Position.Absolute;
    clip.style.backgroundColor = new Color(0.75f, 0.75f, 0.75f);
    var startProp = property.FindPropertyRelative("StartFrame");
    var endProp = property.FindPropertyRelative("EndFrame");
    timelineContainer.Add(clip);
    row.Add(prefix);
    row.Add(timelineContainer);
    root.Add(row);
    var detailField = NoFoldoutInspectorUtility.CreateNoFoldoutInspector(property);
    detailField.style.display = DisplayStyle.None;
    root.Add(detailField);
    bool dragging = false;
    float dragOffset = 0;
    prefix.RegisterCallback<PointerDownEvent>(e => {
      detailField.style.display = detailField.style.display == DisplayStyle.Flex
        ? DisplayStyle.None
        : DisplayStyle.Flex;
      e.StopPropagation();
    });
    clip.RegisterCallback<PointerDownEvent>(e => {
      dragging = true;
      clip.CapturePointer(e.pointerId);
      var localX = e.originalMousePosition.x;
      var w = timelineContainer.resolvedStyle.width;
      var currentOffset = (startProp.intValue / (float)maxFrames) * w;
      dragOffset = localX - currentOffset;
      e.StopPropagation();
    });
    clip.RegisterCallback<PointerMoveEvent>(e => {
      if (!dragging) return;
      var w = timelineContainer.resolvedStyle.width;
      var length = endProp.intValue - startProp.intValue;
      var localX = e.originalMousePosition.x - dragOffset;
      var newStart = (localX / w) * maxFrames;
      newStart = Mathf.Clamp(newStart, 0, maxFrames - length);
      startProp.intValue = Mathf.RoundToInt(newStart);
      endProp.intValue = startProp.intValue + length;
      property.serializedObject.ApplyModifiedProperties();
      UpdateVisual();
      e.StopPropagation();
    });
    clip.RegisterCallback<PointerUpEvent>(e => {
      dragging = false;
      clip.ReleasePointer(e.pointerId);
      e.StopPropagation();
    });
    root.RegisterCallback<GeometryChangedEvent>(e => UpdateVisual());
    detailField.RegisterCallback<SerializedPropertyChangeEvent>(e => UpdateVisual());
    // Steve
    // poll maxFrames prop to resize timeline... kind of stupid we have to poll but not sure how
    // else to do it atm
    if (maxFramesProperty != null) {
      var poll = root.schedule.Execute(() => {
        if (maxFrames != maxFramesProperty.intValue) {
          maxFrames = maxFramesProperty.intValue;
          UpdateVisual();
        }
      }).Every(16);
    }
    void UpdateVisual() {
      property.serializedObject.Update();
      var w = timelineContainer.resolvedStyle.width;
      var h = timelineContainer.resolvedStyle.height;
      var s = startProp.intValue;
      var d = endProp.intValue;
      var offset = (s / (float)maxFrames) * w;
      var cw = ((d - s) / (float)maxFrames) * w;
      clip.style.left = offset;
      clip.style.width = cw;
      clip.style.height = h;
    }
    return root;
  }
}
