using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Frame Behaviors", menuName = "FrameBehaviors/Frame Behaviors")]
public class FrameBehaviors : ScriptableObject {
  public static FrameBehaviors CreateDeepInstance(FrameBehaviors from) {
    var fbs = FrameBehaviors.CreateInstance<FrameBehaviors>();
    fbs.Behaviors = from.Behaviors.Select(FrameBehavior.Clone).ToList();
    fbs.EndFrame = from.EndFrame;
    fbs.PreviewPrefab = from.PreviewPrefab;
    return fbs;
  }

  [SerializeReference]
  public List<FrameBehavior> Behaviors;
  [Min(1)]
  public int EndFrame = 60;
  public MonoBehaviour PreviewPrefab;
}