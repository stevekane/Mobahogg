using UnityEngine;

public class MaxFramesAttribute : PropertyAttribute {
  public string FramePropertyName { get; private set; }
  public string MaxFramePropertyName { get; private set; }

  public MaxFramesAttribute(string framePropertyName, string maxFramePropertyName) {
    FramePropertyName = framePropertyName;
    MaxFramePropertyName = maxFramePropertyName;
  }
}