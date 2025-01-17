using UnityEngine;

public class Hurtbox : MonoBehaviour {
  [SerializeField] Combatant Combatant;
  [SerializeField] Collider Collider;

  public Combatant Owner => Combatant;

  public bool CollisionEnabled {
    get => Collider.enabled;
    set => Collider.enabled = value;
  }
}