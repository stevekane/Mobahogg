using State;
using UnityEngine;

// TODO: Could refactor this to be more generic. Could just emit Events which
// are wired up in the editor for looser coupling
public class SpellAffected : MonoBehaviour {
  [SerializeField] KCharacterController Controller;
  [SerializeField] MoveSpeed MoveSpeedState;
  [SerializeField] Health HealthState;
  [SerializeField] Damage DamageState;
  [SerializeField] Knockback KnockbackState;

  public bool Immune;
  public void MultiplySpeed(float fraction) =>
    MoveSpeedState.Mul(Immune ? 1 : fraction);
  public void AddDamage(int damageDelta) =>
    DamageState.Add(Immune ? 0 : damageDelta);
  public void Heal(int healthDelta) =>
    HealthState.Change(Immune ? 0 : healthDelta);
  public void Push(Vector3 directVelocity) =>
    Controller.DirectVelocity.Add(Immune ? Vector3.zero : directVelocity);
  public void Knockback(Vector3 knockback) =>
    KnockbackState.Set(Immune ? Vector3.zero : knockback);
}