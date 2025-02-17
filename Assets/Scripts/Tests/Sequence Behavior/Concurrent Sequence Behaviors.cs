using UnityEngine;
using System.Collections.Generic;
[CreateAssetMenu(menuName="Sequence/ConcurrentSequenceBehaviors")]
public class ConcurrentSequenceBehaviors : ScriptableObject {
  public List<SequenceBehavior> behaviors = new List<SequenceBehavior>();
}