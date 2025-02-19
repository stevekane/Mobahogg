using UnityEngine;

public class MaxFramesAttribute : PropertyAttribute {
  public string MaxFramePropertyName { get; private set; }

  public MaxFramesAttribute(string propertyName) {
    MaxFramePropertyName = propertyName;
  }
}