using System.Buffers;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(FrameBehavior), true)]
public class FrameBehaviorDrawer : PropertyDrawer {
  public override VisualElement CreatePropertyGUI(SerializedProperty property) {
    var root = new VisualElement();
    root.style.flexDirection = FlexDirection.Column;
    root.style.marginBottom = 8;

    var attrs = fieldInfo.GetCustomAttributes(typeof(MaxFramesAttribute), true) as MaxFramesAttribute[];
    SerializedProperty frameProperty = null;
    SerializedProperty maxFramesProperty = null;
    if (attrs != null && attrs.Length > 0) {
      frameProperty = property.serializedObject.FindProperty(attrs[0].FramePropertyName);
      maxFramesProperty = property.serializedObject.FindProperty(attrs[0].MaxFramePropertyName);
    }
    int frame = frameProperty != null ? frameProperty.intValue : 0;
    int maxFrames = maxFramesProperty != null ? maxFramesProperty.intValue : 60;

    var trackRow = new VisualElement();
    trackRow.style.flexDirection = FlexDirection.Row;
    trackRow.style.alignItems = Align.Center;
    var prefix = new Label(property.displayName);
    prefix.style.width = 180;
    prefix.style.unityFontStyleAndWeight = FontStyle.Bold;
    var timelineContainer = new VisualElement();
    timelineContainer.style.flexGrow = 1;
    timelineContainer.style.flexDirection = FlexDirection.Column;
    trackRow.Add(prefix);
    trackRow.Add(timelineContainer);
    root.Add(trackRow);

    var topContainer = new VisualElement();
    topContainer.style.flexDirection = FlexDirection.Row;
    topContainer.style.height = 16;
    var topLeftLabel = new Label("0");
    topLeftLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
    topLeftLabel.style.fontSize = 10;
    topLeftLabel.style.flexGrow = 1;
    var topRightLabel = new Label(maxFrames.ToString());
    topRightLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
    topRightLabel.style.fontSize = 10;
    topRightLabel.style.unityTextAlign = TextAnchor.MiddleRight;
    topContainer.Add(topLeftLabel);
    topContainer.Add(topRightLabel);

    var midContainer = new VisualElement();
    midContainer.style.flexGrow = 1;
    midContainer.style.height = 10;
    midContainer.style.position = Position.Relative;
    midContainer.style.backgroundColor = new Color(0.1f, 0.1f, 0.1f);
    var clip = new VisualElement();
    clip.style.position = Position.Absolute;
    clip.style.backgroundColor = new Color(0.75f, 0.75f, 0.75f);
    midContainer.Add(clip);
    var playhead = new VisualElement();
    playhead.style.position = Position.Absolute;
    playhead.style.width = 2;
    playhead.style.backgroundColor = Color.red;
    playhead.style.top = 0;
    playhead.style.bottom = 0;
    midContainer.Add(playhead);

    var bottomContainer = new VisualElement();
    bottomContainer.style.position = Position.Relative;
    bottomContainer.style.height = 16;
    var bottomLeftLabel = new Label();
    bottomLeftLabel.style.position = Position.Absolute;
    bottomLeftLabel.style.fontSize = 10;
    bottomLeftLabel.style.unityTextAlign = TextAnchor.UpperLeft;
    bottomLeftLabel.style.width = 20;
    var bottomRightLabel = new Label();
    bottomRightLabel.style.position = Position.Absolute;
    bottomRightLabel.style.fontSize = 10;
    bottomRightLabel.style.unityTextAlign = TextAnchor.UpperRight;
    bottomRightLabel.style.width = 20;
    bottomContainer.Add(bottomLeftLabel);
    bottomContainer.Add(bottomRightLabel);

    timelineContainer.Add(topContainer);
    timelineContainer.Add(midContainer);
    timelineContainer.Add(bottomContainer);
    trackRow.Add(timelineContainer);

    var detailField = NoFoldoutInspectorUtility.CreateNoFoldoutInspector(property);
    detailField.style.marginTop = 8;
    detailField.style.display = DisplayStyle.None;
    root.Add(detailField);

    prefix.RegisterCallback<PointerDownEvent>(e => {
      detailField.style.display = detailField.style.display == DisplayStyle.Flex
        ? DisplayStyle.None
        : DisplayStyle.Flex;
      e.StopPropagation();
    });

    void UpdatePlayHead(float x, float w) {
      var nextFrame = Mathf.RoundToInt(Mathf.Clamp(x, 0, w) / w * maxFrames);
      if (frameProperty.intValue != nextFrame) {
        frameProperty.intValue = nextFrame;
        property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
      }
    }

    timelineContainer.RegisterCallback<PointerDownEvent>(e => {
      if (e.button != 1) return;
      timelineContainer.CapturePointer(e.pointerId);
      UpdatePlayHead(e.localPosition.x, timelineContainer.resolvedStyle.width);
      e.StopPropagation();
    });

    timelineContainer.RegisterCallback<PointerMoveEvent>(e => {
      if (!timelineContainer.HasPointerCapture(e.pointerId)) return;
      UpdatePlayHead(e.localPosition.x, timelineContainer.resolvedStyle.width);
      e.StopPropagation();
    });

    timelineContainer.RegisterCallback<PointerUpEvent>(e => {
      if (!timelineContainer.HasPointerCapture(e.pointerId)) return;
      if (e.button != 1) return;
      UpdatePlayHead(e.localPosition.x, timelineContainer.resolvedStyle.width);
      timelineContainer.ReleasePointer(e.pointerId);
      e.StopPropagation();
    });

    float dragOffset = 0;
    clip.RegisterCallback<PointerDownEvent>(e => {
      if (e.button != 0) return;
      clip.CapturePointer(e.pointerId);
      var localX = e.originalMousePosition.x;
      var w = midContainer.resolvedStyle.width;
      var startProp = property.FindPropertyRelative("StartFrame");
      var currentOffset = startProp.intValue / (float)maxFrames * w;
      dragOffset = localX - currentOffset;
      e.StopPropagation();
    });
    clip.RegisterCallback<PointerMoveEvent>(e => {
      if (!clip.HasPointerCapture(e.pointerId)) return;
      var w = midContainer.resolvedStyle.width;
      var startProp = property.FindPropertyRelative("StartFrame");
      var endProp = property.FindPropertyRelative("EndFrame");
      int length = endProp.intValue - startProp.intValue;
      var localX = e.originalMousePosition.x - dragOffset;
      var newStart = localX / w * maxFrames;
      newStart = Mathf.Clamp(newStart, 0, maxFrames - length);
      startProp.intValue = Mathf.RoundToInt(newStart);
      endProp.intValue = startProp.intValue + length;
      property.serializedObject.ApplyModifiedProperties();
      UpdateVisual();
      e.StopPropagation();
    });
    clip.RegisterCallback<PointerUpEvent>(e => {
      if (!clip.HasPointerCapture(e.pointerId)) return;
      clip.ReleasePointer(e.pointerId);
      e.StopPropagation();
    });

    root.schedule.Execute(() => {
      if (frame != frameProperty.intValue || maxFrames != maxFramesProperty.intValue) {
        frame = frameProperty.intValue;
        maxFrames = maxFramesProperty.intValue;
        UpdateVisual();
      }
    }).Every(16);

    root.RegisterCallback<GeometryChangedEvent>(e => UpdateVisual());
    detailField.RegisterCallback<SerializedPropertyChangeEvent>(e => UpdateVisual());

    void UpdateVisual() {
      property.serializedObject.Update();
      var midWidth = midContainer.resolvedStyle.width;
      var startProp = property.FindPropertyRelative("StartFrame");
      var endProp = property.FindPropertyRelative("EndFrame");
      int s = startProp.intValue;
      int d = endProp.intValue;
      float offset = s / (float)maxFrames * midWidth;
      float cw = (d - s) / (float)maxFrames * midWidth;
      clip.style.left = offset;
      clip.style.width = cw;
      clip.style.height = midContainer.resolvedStyle.height;
      topRightLabel.text = maxFrames.ToString();
      bottomLeftLabel.text = s.ToString();
      bottomRightLabel.text = d.ToString();
      bottomLeftLabel.style.left = offset;
      bottomRightLabel.style.left = offset + cw - bottomRightLabel.resolvedStyle.width;
      float playheadX = frame / (float)maxFrames * midWidth;
      playhead.style.left = playheadX;
    }
    return root;
  }
}