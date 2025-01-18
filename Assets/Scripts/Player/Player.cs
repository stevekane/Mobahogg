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
  [SerializeField] bool ShowDebug = false;

  public string Name => MatchManager.Instance.Players[PortIndex].Name;
  public int PortIndex;

  // These kind of a stop-gap while I figure out how to properly handle these instant-like abilities
  public bool Jumped;
  public bool Hover;

  bool InCoyoteWindow
    => (LocalClock.FixedFrame() - CharacterController.LastGroundedFrame) < Settings.CoyoteFrameCount
    && !CharacterController.IsGrounded
    && CharacterController.Falling;

  bool InValidState
    => !LocalClock.Frozen()
    && Health.CurrentValue > 0;
    // Not stunned

  bool AllNotRunningOrStoppable
    => (!AttackAbility.IsRunning || AttackAbility.CanStop)
    && (!SpellCastAbility.IsRunning || SpellCastAbility.CanStop)
    && (!DiveRollAbility.IsRunning || DiveRollAbility.CanStop);

  void StopRunning() {
    if (AttackAbility.CanStop) AttackAbility.Stop();
    if (SpellCastAbility.CanStop) SpellCastAbility.Stop();
    if (DiveRollAbility.CanStop) DiveRollAbility.Stop();
  }

  public bool CanJump
    => InValidState
    && AllNotRunningOrStoppable
    && JumpAbility.CanRun
    && (CharacterController.IsGrounded || InCoyoteWindow);

  public bool CanAttack
    => InValidState
    && AllNotRunningOrStoppable
    && AttackAbility.CanRun
    && CharacterController.IsGrounded;

  public bool CanDash
    => InValidState
    && AllNotRunningOrStoppable
    && DiveRollAbility.CanRun
    && CharacterController.IsGrounded;

  public bool CanCastSpell
    => InValidState
    && AllNotRunningOrStoppable
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
    StopRunning();
    Jumped = true;
    JumpAbility.Run();
  }

  public void StartHover() {
    Hover = true;
  }

  public void EndHover() {
    Hover = false;
  }

  public void Dash(Vector2 direction) {
    StopRunning();
    DiveRollAbility.Run();
    DiveRollAbility.Launch(direction);
  }

  public void Attack(Vector2 direction) {
    StopRunning();
    AttackAbility.Run();
    AttackAbility.Aim(direction);
  }

  public void CastSpell(Vector2 direction) {
    StopRunning();
    SpellCastAbility.Run();
    SpellCastAbility.Aim(direction);
  }

  public void Move(Vector2 direction) {
    MoveAbility.Run();
    MoveAbility.Steer(direction);
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
    } else if (Hover) {
      CharacterController.Velocity.SetY(-Mathf.Abs(Settings.HoverVelocity));
    } else if (CharacterController.IsGrounded) {
      CharacterController.Velocity.SetY(0);
    } else {
      CharacterController.Acceleration.Add(Settings.Gravity(CharacterController.Velocity.Current));
    }
    Jumped = false;
  }
}