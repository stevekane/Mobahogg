using State;
using UnityEngine;

public class MoveAbility : MonoBehaviour, IAbility<Vector2> {
  [Header("Reads From")]
  [SerializeField] Player Player;
  [SerializeField] MoveSpeed MoveSpeed;
  [SerializeField] LocalClock LocalClock;
  [SerializeField] Vector3 HandRotationEulerAngles = Vector3.zero;
  [SerializeField] bool UseConstraint;

  [Header("Writes To")]
  [SerializeField] KCharacterController CharacterController;
  [SerializeField] AnimatorCallbackHandler AnimatorCallbackHandler;
  [SerializeField] Animator Animator;

  void Start() {
    AnimatorCallbackHandler.OnIK.Listen(OnAnimatorIK);
  }

  void OnDestroy() {
    AnimatorCallbackHandler.OnIK.Unlisten(OnAnimatorIK);
  }

  void OnAnimatorIK(int layer) {
    var target = Player.transform.position + Vector3.up + 2 * Player.transform.right;
    AnimatorCallbackHandler.Animator.SetIKPosition(AvatarIKGoal.RightHand, target);
    // AnimatorCallbackHandler.Animator.SetIKRotation(AvatarIKGoal.RightHand, Quaternion.Euler(HandRotationEulerAngles) * Player.transform.rotation);
    AnimatorCallbackHandler.Animator.SetIKRotation(AvatarIKGoal.RightHand, Quaternion.LookRotation(Player.transform.right));
    AnimatorCallbackHandler.Animator.SetIKPositionWeight(AvatarIKGoal.RightHand, UseConstraint && CanRun ? 1 : 0);
    AnimatorCallbackHandler.Animator.SetIKRotationWeight(AvatarIKGoal.RightHand, UseConstraint && CanRun ? 1 : 0);
  }

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