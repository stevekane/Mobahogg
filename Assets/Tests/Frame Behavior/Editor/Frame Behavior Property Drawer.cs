using System.Linq.Expressions;
using Unity.Properties;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(FrameBehavior), true)]
public class FrameBehaviorDrawer : PropertyDrawer
{
  public override VisualElement CreatePropertyGUI(SerializedProperty property)
  {
    var behaviorRoot = new FrameBehaviorRoot();
    behaviorRoot.BindProperty(property);
    return behaviorRoot;
  }
}

public interface IPropertyBinder {
  public void BindProperty(SerializedProperty property);
}

public class FrameBehaviorRoot : VisualElement, IBindable, IPropertyBinder {
  SerializedProperty Property;
  Label prefix;
  VisualElement timelineContainer;
  VisualElement topContainer;
  VisualElement midContainer;
  VisualElement bottomContainer;
  VisualElement clip;
  VisualElement playhead;
  Label topRightLabel;
  Label bottomLeftLabel;
  Label bottomRightLabel;
  VisualElement detailContainer;

  public int Frame { get; private set; } = 0;
  public int MaxFrame { get; private set; } = 60;

  public string bindingPath { get; set; }
  public IBinding binding { get; set; }

  public FrameBehaviorRoot() {
    style.flexDirection = FlexDirection.Column;
    BuildUI();
    RegisterCallbacks();
  }

  public void BindProperty(SerializedProperty property)
  {
    Property = property;
    // Seems to complain that binding to reference object is not supported... not sure what it means
    // bindingPath = property.propertyPath;
    this.Bind(property.serializedObject);
    var detailPanel = NoFoldoutInspectorUtility.CreateNoFoldoutInspector(property);
    detailContainer.Clear();
    detailContainer.Add(detailPanel);
    UpdateVisual();
  }

  void BuildUI()
  {
    // Build the header row containing the prefix and the timeline container.
    var trackRow = new VisualElement();
    trackRow.style.flexDirection = FlexDirection.Row;
    trackRow.style.alignItems = Align.Center;
    prefix = new Label("label");
    prefix.style.width = 180;
    prefix.style.unityFontStyleAndWeight = FontStyle.Bold;
    timelineContainer = new VisualElement();
    timelineContainer.style.flexGrow = 1;
    timelineContainer.style.flexDirection = FlexDirection.Column;
    trackRow.Add(prefix);
    trackRow.Add(timelineContainer);
    Add(trackRow);

    // Top section: extent labels ("0" and MaxFrame)
    topContainer = new VisualElement();
    topContainer.style.flexDirection = FlexDirection.Row;
    topContainer.style.height = 16;
    var topLeftLabel = new Label("0");
    topLeftLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
    topLeftLabel.style.fontSize = 10;
    topLeftLabel.style.flexGrow = 1;
    topRightLabel = new Label(MaxFrame.ToString());
    topRightLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
    topRightLabel.style.fontSize = 10;
    topRightLabel.style.unityTextAlign = TextAnchor.MiddleRight;
    topContainer.Add(topLeftLabel);
    topContainer.Add(topRightLabel);

    // Middle section: timeline track with clip and playhead.
    midContainer = new VisualElement();
    midContainer.style.flexGrow = 1;
    midContainer.style.height = 10;
    midContainer.style.position = Position.Relative;
    midContainer.style.backgroundColor = new Color(0.1f, 0.1f, 0.1f);
    clip = new VisualElement();
    clip.style.position = Position.Absolute;
    clip.style.backgroundColor = new Color(0.75f, 0.75f, 0.75f);
    midContainer.Add(clip);
    playhead = new VisualElement();
    playhead.style.position = Position.Absolute;
    playhead.style.width = 5;
    playhead.style.backgroundColor = Color.red;
    playhead.style.top = 0;
    playhead.style.bottom = 0;
    midContainer.Add(playhead);

    // Bottom section: labels for StartFrame and EndFrame.
    bottomContainer = new VisualElement();
    bottomContainer.style.position = Position.Relative;
    bottomContainer.style.height = 16;
    bottomLeftLabel = new Label();
    bottomLeftLabel.style.position = Position.Absolute;
    bottomLeftLabel.style.fontSize = 10;
    bottomLeftLabel.style.unityTextAlign = TextAnchor.UpperLeft;
    bottomLeftLabel.style.width = 20;
    bottomRightLabel = new Label();
    bottomRightLabel.style.position = Position.Absolute;
    bottomRightLabel.style.fontSize = 10;
    bottomRightLabel.style.unityTextAlign = TextAnchor.UpperRight;
    bottomRightLabel.style.width = 20;
    bottomContainer.Add(bottomLeftLabel);
    bottomContainer.Add(bottomRightLabel);

    // Assemble timeline container.
    timelineContainer.Add(topContainer);
    timelineContainer.Add(midContainer);
    timelineContainer.Add(bottomContainer);

    // Create the detail field inspector.
    detailContainer = new VisualElement();
    detailContainer.style.marginTop = 8;
    detailContainer.style.display = DisplayStyle.None;
    Add(detailContainer);
  }

  void RegisterCallbacks()
  {
    // Toggle detail view on clicking the prefix label.
    prefix.RegisterCallback<PointerDownEvent>(e =>
    {
      detailContainer.style.display = detailContainer.style.display == DisplayStyle.Flex
        ? DisplayStyle.None
        : DisplayStyle.Flex;
      e.StopPropagation();
    });

    float dragOffset = 0;
    // Dragging on the clip adjusts StartFrame and EndFrame.
    clip.RegisterCallback<PointerDownEvent>(e =>
    {
      if (e.button != 0)
        return;
      clip.CapturePointer(e.pointerId);
      var localX = e.originalMousePosition.x;
      var w = midContainer.resolvedStyle.width;
      var startProp = Property.FindPropertyRelative("StartFrame");
      var currentOffset = startProp.intValue / (float)MaxFrame * w;
      dragOffset = localX - currentOffset;
      e.StopPropagation();
    });
    clip.RegisterCallback<PointerMoveEvent>(e =>
    {
      if (!clip.HasPointerCapture(e.pointerId))
        return;
      Property.serializedObject.Update();
      var w = midContainer.resolvedStyle.width;
      var startProp = Property.FindPropertyRelative("StartFrame");
      var endProp = Property.FindPropertyRelative("EndFrame");
      int length = endProp.intValue - startProp.intValue;
      var localX = e.originalMousePosition.x - dragOffset;
      var newStart = localX / w * MaxFrame;
      newStart = Mathf.Clamp(newStart, 0, MaxFrame - length);
      startProp.intValue = Mathf.RoundToInt(newStart);
      endProp.intValue = startProp.intValue + length;
      Property.serializedObject.ApplyModifiedProperties();
      UpdateVisual();
      e.StopPropagation();
    });
    clip.RegisterCallback<PointerUpEvent>(e =>
    {
      if (!clip.HasPointerCapture(e.pointerId))
        return;
      clip.ReleasePointer(e.pointerId);
      e.StopPropagation();
    });

    RegisterCallback<GeometryChangedEvent>(e => UpdateVisual());
    RegisterCallback<SerializedPropertyChangeEvent>(evt => UpdateVisual());
  }

  public void SetFrame(int frame)
  {
    Frame = frame;
    UpdateVisual();
  }

  public void SetMaxFrame(int maxFrame)
  {
    MaxFrame = maxFrame;
    UpdateVisual();
  }

  void UpdateVisual() {
    Property.serializedObject.Update();
    var midWidth = midContainer.resolvedStyle.width;
    var startProp = Property.FindPropertyRelative("StartFrame");
    var endProp = Property.FindPropertyRelative("EndFrame");
    int s = startProp.intValue;
    int d = endProp.intValue;
    float offset = s / (float)MaxFrame * midWidth;
    float cw = (d - s) / (float)MaxFrame * midWidth;
    clip.style.left = offset;
    clip.style.width = cw;
    clip.style.height = midContainer.resolvedStyle.height;
    if (topRightLabel != null)
      topRightLabel.text = MaxFrame.ToString();
    bottomLeftLabel.text = s.ToString();
    bottomRightLabel.text = d.ToString();
    bottomLeftLabel.style.left = offset;
    bottomRightLabel.style.left = offset + cw - bottomRightLabel.resolvedStyle.width;
    float playheadX = Frame / (float)MaxFrame * midWidth;
    playhead.style.left = playheadX;
    prefix.text = Property.GetTargetObjectOfProperty().GetType().HumanName();
  }
}