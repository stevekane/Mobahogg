using UnityEngine;

public class Hurtbox : MonoBehaviour {
  public Combatant Owner;
  Collider Collider;

  public bool CollisionEnabled {
    get => Collider.enabled;
    set => Collider.enabled = value;
  }

  void Awake() {
    this.InitComponent(out Collider);
    Owner = Owner ?? GetComponentInParent<Combatant>();
  }
}