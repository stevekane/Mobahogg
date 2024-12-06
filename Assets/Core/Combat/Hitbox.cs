using UnityEngine;

public class Hitbox : MonoBehaviour {
  [SerializeField] Combatant Combatant;
  [SerializeField] Collider Collider;

  public Combatant Owner => Combatant;

  public bool CollisionEnabled {
    get => Collider.enabled;
    set => Collider.enabled = value;
  }

  void OnTriggerEnter(Collider c) {
    if (c.TryGetComponent(out Hurtbox hurtbox) && hurtbox.Owner != Owner) {
      Owner.SendMessage("OnHit", hurtbox.Owner, SendMessageOptions.DontRequireReceiver);
      hurtbox.Owner.SendMessage("OnHurt", Owner, SendMessageOptions.DontRequireReceiver);
    }
  }
}