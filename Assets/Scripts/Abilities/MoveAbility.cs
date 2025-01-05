using State;
using UnityEngine;

public class MoveAbility : MonoBehaviour, IAbility<Vector2> {
  [Header("Reads From")]
  [SerializeField] Player Player;
  [SerializeField] MoveSpeed MoveSpeed;
  [SerializeField] LocalClock LocalClock;

  [Header("Writes To")]
  [SerializeField] KCharacterController CharacterController;
  [SerializeField] Animator Animator;

  public bool CanRun
    => !LocalClock.Frozen()
    && !Player.AttackAbility.IsRunning
    && !Player.DiveRollAbility.IsRunning
    && !Player.SpellCastAbility.IsRunning;

  public bool TryRun(Vector2 value) {
    if (CanRun) {
      var velocity = MoveSpeed.Value * value.XZ();
      CharacterController.DirectVelocity.Add(velocity);
      Animator.SetFloat("Speed", velocity.magnitude);
      return true;
    } else {
      return false;
    }
  }
}