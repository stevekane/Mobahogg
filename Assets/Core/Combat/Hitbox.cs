using UnityEngine;

public class Hitbox : MonoBehaviour {
  public Combatant Owner;
  public AttributeValue Damage;
  public HitboxGroup HitboxGroup { get; set; }
  Collider Collider;

  public bool CollisionEnabled {
    get => Collider.enabled;
    set => Collider.enabled = value;
  }

  void Awake() {
    this.InitComponent(out Collider);
    Owner = Owner ?? GetComponentInParent<Combatant>();
  }

  void OnTriggerEnter(Collider c) {
    if (c.TryGetComponent(out Hurtbox hurtee))
      HitboxGroup.OnHit(this, hurtee);
  }
}