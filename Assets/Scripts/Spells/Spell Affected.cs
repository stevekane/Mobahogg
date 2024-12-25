using State;
using UnityEngine;

[DefaultExecutionOrder((int)ExecutionGroups.State)]
public class SpellAffected : MonoBehaviour {
  [SerializeField] KCharacterController Controller;
  [SerializeField] MoveSpeed MoveSpeedState;
  [SerializeField] Health HealthState;
  [SerializeField] Damage DamageState;
  [SerializeField] Knockback KnockbackState;

  public BooleanAnyAttribute Immune;
  public void MultiplySpeed(float fraction) =>
    MoveSpeedState.Mul(Immune.Current ? 1 : fraction);
  public void ChangeDamage(int damageDelta) =>
    DamageState.Add(Immune.Current ? 0 : damageDelta);
  public void ChangeHealth(int healthDelta) =>
    HealthState.Change(Immune.Current ? 0 : healthDelta);
  public void Push(Vector3 directVelocity) =>
    Controller.DirectVelocity.Add(Immune.Current ? Vector3.zero : directVelocity);
  public void Knockback(Vector3 knockback) =>
    KnockbackState.Set(Immune.Current ? Vector3.zero : knockback);

  void FixedUpdate() {
    Immune.Sync();
  }
}