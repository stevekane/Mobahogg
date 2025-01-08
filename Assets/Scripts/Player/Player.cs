using State;
using UnityEngine;

[DefaultExecutionOrder((int)ExecutionGroups.Managed)]
public class Player : MonoBehaviour {
  [SerializeField] AbilitySettings Settings;
  [SerializeField] KCharacterController CharacterController;
  [SerializeField] LocalClock LocalClock;
  [SerializeField] Health Health;
  [SerializeField] AttackAbility AttackAbility;
  [SerializeField] SpellCastAbility SpellCastAbility;
  [SerializeField] DiveRollAbility DiveRollAbility;
  [SerializeField] JumpAbility JumpAbility;
  [SerializeField] MoveAbility MoveAbility;
  [SerializeField] HoverAbility HoverAbility;

  public string Name => MatchManager.Instance.Players[PortIndex].Name;
  public int PortIndex;

  bool Jumped;

  bool InValidState
    => !LocalClock.Frozen()
    && Health.CurrentValue > 0;
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
    && SpellCastAbility.CanRun;

  public bool CanMove
    => InValidState
    && MoveAbility.CanRun
    && !AttackAbility.IsRunning
    && !SpellCastAbility.IsRunning
    && !DiveRollAbility.IsRunning;

  public bool CanSteer
    => DiveRollAbility.IsRunning
    && DiveRollAbility.CanSteer;

  public bool CanHover
    => InValidState
    && !CharacterController.IsGrounded
    && CharacterController.Falling
    && !SpellCastAbility.IsRunning;

  public void Jump() {
    CancelRunning();
    Jumped = true;
    JumpAbility.Run();
  }

  public void Hover() {
    HoverAbility.IsRunning = true;
  }

  public void EndHover() {
    HoverAbility.IsRunning = false;
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

  public void Steer(Vector2 direction) {
    DiveRollAbility.Steer(direction);
  }

  void Start() {
    LivesManager.Active.Players.AddFirst(this);
  }

  void OnDestroy() {
    LivesManager.Active.Players.Remove(this);
  }

  void FixedUpdate() {
    if (Jumped) {
      var jumpSpeed = Settings.InitialJumpSpeed;
      CharacterController.ForceUnground.Set(true);
      CharacterController.Velocity.SetY(jumpSpeed);
    } else if (HoverAbility.IsRunning) {
      CharacterController.Velocity.SetY(-Mathf.Abs(Settings.HoverVelocity));
    } else if (CharacterController.IsGrounded) {
      CharacterController.Velocity.SetY(0);
    } else {
      CharacterController.Acceleration.Add(Settings.Gravity(CharacterController.Velocity.Current));
    }
    Jumped = false;
  }
}