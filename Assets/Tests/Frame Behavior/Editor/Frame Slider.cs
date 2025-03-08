using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class FrameSlider : VisualElement {
  int m_Frame = 0;
  int m_EndFrame = 100;
  Label prefixLabel;
  Label frameLabel;
  VisualElement frameSelector;
  VisualElement indicator;

  public int Frame {
    get => m_Frame;
    set {
      int newVal = Mathf.Clamp(value, 0, EndFrame);
      if(m_Frame == newVal)
        return;
      int oldVal = m_Frame;
      m_Frame = newVal;
      UpdateIndicator();
      ChangeEvent<int> evt = ChangeEvent<int>.GetPooled(oldVal, m_Frame);
      evt.target = this;
      SendEvent(evt);
    }
  }

  public int EndFrame {
    get => m_EndFrame;
    set {
      int newVal = Mathf.Max(1, value);
      if(m_EndFrame == newVal)
        return;
      m_EndFrame = newVal;
      UpdateIndicator();
    }
  }

  public FrameSlider() {
    style.flexDirection = FlexDirection.Row;
    prefixLabel = new Label("Frame:");
    prefixLabel.style.width = 180;
    Add(prefixLabel);
    frameSelector = new VisualElement();
    frameSelector.style.flexGrow = 1;
    frameSelector.style.position = Position.Relative;
    frameSelector.style.backgroundColor = new StyleColor(new Color(0.2f, 0.2f, 0.2f));
    Add(frameSelector);
    indicator = new VisualElement();
    indicator.style.position = Position.Absolute;
    indicator.style.width = 5;
    indicator.style.height = new Length(100, LengthUnit.Percent);
    indicator.style.backgroundColor = new StyleColor(Color.red);
    frameLabel = new Label(Frame.ToString());
    frameLabel.style.paddingLeft = 8;
    frameLabel.style.color = Color.red;
    frameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
    indicator.Add(frameLabel);
    frameSelector.Add(indicator);
    frameSelector.RegisterCallback<PointerDownEvent>(OnPointerDown);
    frameSelector.RegisterCallback<PointerMoveEvent>(OnPointerMove);
    frameSelector.RegisterCallback<PointerUpEvent>(OnPointerUp);
    frameSelector.RegisterCallback<GeometryChangedEvent>(evt => UpdateIndicator());
    var rightBlock = new VisualElement();
    rightBlock.style.width = 60;
    rightBlock.style.flexGrow = 0;
    rightBlock.style.flexShrink = 0;
    Add(rightBlock);
  }

  void OnPointerDown(PointerDownEvent evt) {
    Vector2 local = evt.localPosition;
    float width = frameSelector.layout.width;
    int newFrame = Mathf.RoundToInt(local.x / width * EndFrame);
    Frame = newFrame;
    frameSelector.CapturePointer(evt.pointerId);
    evt.StopPropagation();
  }

  void OnPointerMove(PointerMoveEvent evt) {
    if (frameSelector.HasPointerCapture(evt.pointerId)) {
      Vector2 local = evt.localPosition;
      float width = frameSelector.layout.width;
      int newFrame = Mathf.RoundToInt(local.x / width * EndFrame);
      Frame = newFrame;
      evt.StopPropagation();
    }
  }

  void OnPointerUp(PointerUpEvent evt) {
    frameSelector.ReleasePointer(evt.pointerId);
    evt.StopPropagation();
  }

  void UpdateIndicator() {
    float width = frameSelector.layout.width;
    if(width > 0 && EndFrame > 0) {
      float pos = Frame / (float)EndFrame * width;
      indicator.style.left = pos;
      frameLabel.text = Frame.ToString();
    }
  }
}