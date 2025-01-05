using State;
using UnityEngine;

[DefaultExecutionOrder((int)ExecutionGroups.Managed)]
public class Player : MonoBehaviour {
  [SerializeField] KCharacterController CharacterController;
  [SerializeField] LocalClock LocalClock;
  [SerializeField] Health Health;
  [SerializeField] AnimatorCallbackHandler AnimatorCallbackHandler;

  // TODO: Would it make sense to use the already-available "name" property Unity has?
  public string Name => MatchManager.Instance.Players[PortIndex].Name;
  public int PortIndex;
  public AttackAbility AttackAbility;
  public JumpAbility JumpAbility;
  public AirAttackAbility AirAttackAbility;
  public SpellCastAbility SpellCastAbility;
  public DiveRollAbility DiveRollAbility;
  public MoveAbility MoveAbility;
  public TurnAbility TurnAbility;
  public float ArmIKWeightSpeed = 2;

  public bool AbilityActive
    => AttackAbility.IsRunning
    || AirAttackAbility.IsRunning
    || DiveRollAbility.IsRunning
    || SpellCastAbility.IsRunning;

  public bool Grounded => CharacterController.IsGrounded;

  void Start() {
    LivesManager.Active.Players.AddFirst(this);
    AnimatorCallbackHandler.OnIK.Listen(OnAnimatorIK);
  }

  void OnDestroy() {
    LivesManager.Active.Players.Remove(this);
    AnimatorCallbackHandler.OnIK.Unlisten(OnAnimatorIK);
  }

  void FixedUpdate() {
    // TODO: Handling falling death in this hardcoded way probably isn't that bright...
    if (Health.CurrentValue <= 0 || transform.position.y <= -10) {
      // TODO: This is probably not quite right. Seems like potentially this could be an
      // event that goes on some bus where high level systems listen to react?
      CreepManager.Active.OnOwnerDeath(GetComponent<CreepOwner>());
      LivesManager.Active.OnPlayerDeath(this);
    }
    AnimatorCallbackHandler.Animator.SetBool("Grounded", CharacterController.IsGrounded);
  }

  float ArmIKWeight = 0;
  void OnAnimatorIK(int layer) {
    var weight = 0f;
    var position = Vector3.zero;
    var rotation = Quaternion.identity;
    if (Grounded && !AbilityActive) {
      weight = 1;
      rotation = Quaternion.LookRotation(transform.right);
      position = transform.position + Vector3.up + 2 * transform.right;
    }
    ArmIKWeight = Mathf.MoveTowards(ArmIKWeight, weight, LocalClock.DeltaTime() * ArmIKWeightSpeed);
    AnimatorCallbackHandler.Animator.SetIKPosition(AvatarIKGoal.RightHand, position);
    AnimatorCallbackHandler.Animator.SetIKRotation(AvatarIKGoal.RightHand, rotation);
    AnimatorCallbackHandler.Animator.SetIKPositionWeight(AvatarIKGoal.RightHand, ArmIKWeight);
    AnimatorCallbackHandler.Animator.SetIKRotationWeight(AvatarIKGoal.RightHand, ArmIKWeight);
  }
}