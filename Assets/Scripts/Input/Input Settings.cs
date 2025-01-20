using UnityEngine;

[CreateAssetMenu(fileName = "Input Settings", menuName = "Settings/Input")]
public class InputSettings : ScriptableObject {
  public int InputBufferFrameWindow = 12;
}