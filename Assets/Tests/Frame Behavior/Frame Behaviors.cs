using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Frame Behaviors", menuName = "FrameBehaviors/Frame Behaviors")]
public class FrameBehaviors : ScriptableObject {
  [SerializeReference]
  public List<FrameBehavior> Behaviors;
  [Min(1)]
  public int EndFrame = 60;
  public MonoBehaviour PreviewPrefab;

  public FrameBehaviors() {
    Behaviors = new();
  }

  public FrameBehaviors(FrameBehaviors from) {
    Behaviors = from.Behaviors.Select(FrameBehavior.Clone).ToList();
    EndFrame = from.EndFrame;
    PreviewPrefab = from.PreviewPrefab;
  }
}