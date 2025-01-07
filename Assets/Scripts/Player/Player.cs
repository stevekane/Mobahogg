using State;
using UnityEngine;

[DefaultExecutionOrder((int)ExecutionGroups.Managed)]
public class Player : MonoBehaviour {
  [SerializeField] KCharacterController CharacterController;
  [SerializeField] LocalClock LocalClock;
  [SerializeField] Health Health;
  [SerializeField] AnimatorCallbackHandler AnimatorCallbackHandler;
  [SerializeField] AttackAbility AttackAbility;
  [SerializeField] SpellCastAbility SpellCastAbility;
  [SerializeField] DiveRollAbility DiveRollAbility;
  [SerializeField] JumpAbility JumpAbility;
  [SerializeField] MoveAbility MoveAbility;
  [SerializeField] TurnAbility TurnAbility;

  public string Name => MatchManager.Instance.Players[PortIndex].Name;
  public int PortIndex;

  bool InValidState
    => !LocalClock.Frozen();
    // Alive
    // Not stunned

  bool AllNotRunningOrCancellable
    => (!AttackAbility.IsRunning || AttackAbility.CanCancel)
    && (!SpellCastAbility.IsRunning || SpellCastAbility.CanCancel)
    && (!DiveRollAbility.IsRunning || DiveRollAbility.CanCancel);

  void CancelRunning() {
    if (AttackAbility.CanCancel) AttackAbility.Cancel();
    if (SpellCastAbility.CanCancel) SpellCastAbility.Cancel();
    if (DiveRollAbility.CanCancel) DiveRollAbility.Cancel();
  }

  public bool CanJump
    => InValidState
    && AllNotRunningOrCancellable
    && JumpAbility.CanRun
    && CharacterController.IsGrounded;

  public bool CanAttack
    => InValidState
    && AllNotRunningOrCancellable
    && AttackAbility.CanRun
    && CharacterController.IsGrounded;

  public bool CanDash
    => InValidState
    && AllNotRunningOrCancellable
    && DiveRollAbility.CanRun
    && CharacterController.IsGrounded;

  public bool CanCastSpell
    => InValidState
    && AllNotRunningOrCancellable
    && SpellCastAbility.CanRun
    && CharacterController.IsGrounded;

  public bool CanMove
    => InValidState
    && MoveAbility.CanRun
    && !AttackAbility.IsRunning
    && !SpellCastAbility.IsRunning
    && !DiveRollAbility.IsRunning;

  public bool CanTurn
    => InValidState
    && TurnAbility.CanRun
    && !AttackAbility.IsRunning
    && !SpellCastAbility.IsRunning;

  public void Jump() {
    CancelRunning();
    JumpAbility.Run();
  }

  public void Dash(Vector2 direction) {
    CancelRunning();
    DiveRollAbility.Run(direction);
  }

  public void Attack(Vector2 direction) {
    CancelRunning();
    AttackAbility.Run(direction);
  }

  public void CastSpell(Vector2 direction) {
    CancelRunning();
    SpellCastAbility.Run(direction);
  }

  public void Move(Vector2 direction) {
    MoveAbility.Run(direction);
  }

  public void Turn(Vector2 direction) {
    TurnAbility.Run(direction);
  }

  void Start() {
    LivesManager.Active.Players.AddFirst(this);
  }

  void OnDestroy() {
    LivesManager.Active.Players.Remove(this);
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
}