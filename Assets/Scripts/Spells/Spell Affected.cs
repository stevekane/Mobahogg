using State;
using UnityEngine;

public class SpellAffected : MonoBehaviour {
  [SerializeField] KCharacterController Controller;
  [SerializeField] MoveSpeed MoveSpeed;
  [SerializeField] Health Health;
  [SerializeField] Damage Damage;

  public bool Immune;
  public void MultiplySpeed(float fraction) => MoveSpeed.Mul(Immune ? 1 : fraction);
  public void AddDamage(int damageDelta) => Damage.Add(Immune ? 0 : damageDelta);
  public void Heal(int healthDelta) => Health.Change(Immune ? 0 : healthDelta);
  public void Push(Vector3 acceleration) => Controller.Acceleration.Add(Immune ? Vector3.zero : acceleration);
}