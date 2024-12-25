using State;
using UnityEngine;

public class MoveAbility : MonoBehaviour, IAbility<Vector2> {
  [Header("Reads From")]
  [SerializeField] Player Player;
  [SerializeField] MoveSpeed MoveSpeed;
  [SerializeField] LocalClock LocalClock;

  [Header("Writes To")]
  [SerializeField] KCharacterController CharacterController;

  public bool CanRun
    => !LocalClock.Frozen()
    && !Player.AttackAbility.IsRunning
    && !Player.DiveRollAbility.IsRunning
    && !Player.SpellCastAbility.IsRunning;

  public bool TryRun(Vector2 value) {
    if (CanRun) {
      CharacterController.DirectVelocity.Add(MoveSpeed.Value * value.XZ());
      return true;
    } else {
      return false;
    }
  }
}