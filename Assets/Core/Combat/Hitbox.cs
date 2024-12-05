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
      hurtbox.Owner.SendMessage("OnHurt", Owner, SendMessageOptions.DontRequireReceiver);
      Debug.Log($"Hitbox {Owner.name} has struck Hurtbox {hurtbox.Owner.name} to NO EFFECT");
      // OnHit stuff should happen here
    }
  }
}