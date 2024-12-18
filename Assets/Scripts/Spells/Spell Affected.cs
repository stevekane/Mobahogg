using State;
using UnityEngine;

public class SpellAffected : MonoBehaviour {
  [SerializeField] KCharacterController Controller;
  [SerializeField] MoveSpeed MoveSpeed;
  [SerializeField] Health Health;

  public void Slow(float fraction) => MoveSpeed.Mul(fraction);
  public void Push(Vector3 acceleration) => Controller.Acceleration += acceleration;
  public void Damage(int amount) => Health.Change(amount);
}