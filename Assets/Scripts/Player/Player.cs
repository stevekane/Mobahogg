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

  bool AliveAndActive
    => !LocalClock.Frozen()
    && Health.CurrentValue > 0;
    // Not stunned

  bool AbleToAct
    => (!AttackAbility.IsRunning || AttackAbility.CanCancel)
    && (!SpellCastAbility.IsRunning || SpellCastAbility.CanCancel)
    && (!DiveRollAbility.IsRunning || DiveRollAbility.CanCancel)
    && (!HoverAbility.IsRunning || HoverAbility.CanCancel);

  void StopRunning() {
    if (AttackAbility.CanCancel) AttackAbility.Cancel();
    if (SpellCastAbility.CanCancel) SpellCastAbility.Cancel();
    if (DiveRollAbility.CanCancel) DiveRollAbility.Cancel();
    if (HoverAbility.CanCancel) HoverAbility.Cancel();
  }

  public bool CanJump
    => AliveAndActive
    && AbleToAct
    && JumpAbility.CanRun;

  public bool CanAttack
    => AliveAndActive
    && AbleToAct
    && AttackAbility.CanRun;

  public bool CanDash
    => AliveAndActive
    && AbleToAct
    && DiveRollAbility.CanRun;

  public bool CanCastSpell
    => AliveAndActive
    && AbleToAct
    && SpellCastAbility.CanRun;

  public bool CanMove
    => AliveAndActive
    && MoveAbility.CanRun
    && !AttackAbility.IsRunning
    && !SpellCastAbility.IsRunning
    && !DiveRollAbility.IsRunning;

  public bool CanSteer
    => DiveRollAbility.IsRunning
    && DiveRollAbility.CanSteer;

  public bool CanHover
    => AliveAndActive
    && AbleToAct
    && HoverAbility.CanRun;

  public bool CanEndHover
    => AliveAndActive
    && HoverAbility.IsRunning
    && HoverAbility.CanCancel;

  public void Jump() {
    StopRunning();
    JumpAbility.Run();
  }

  public void StartHover() {
    StopRunning();
    HoverAbility.Run();
  }

  public void EndHover() {
    HoverAbility.Cancel();
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
}