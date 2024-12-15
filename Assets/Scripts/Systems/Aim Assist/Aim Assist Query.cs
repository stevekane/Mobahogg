using UnityEngine;

namespace AimAssist {
  [CreateAssetMenu(menuName = "AimAssistQuery")]
  public class AimAssistQuery : ScriptableObject {
    public LayerMask LayerMask = default;
    public QueryTriggerInteraction TriggerInteraction = QueryTriggerInteraction.Collide;
    public float MaxDistance = 5;
    public float MaxAngle = 30;
  }
}