using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Frame Behaviors", menuName = "FrameBehaviors/Frame Behaviors")]
public class FrameBehaviors : ScriptableObject {
  [SerializeReference]
  public List<FrameBehavior> Behaviors;
  [Min(1)]
  public int EndFrame = 60;
}