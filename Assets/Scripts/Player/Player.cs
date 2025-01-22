using Abilities;
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

  public Ability ActiveAbility;
  public Ability UltimateAbility;

  public string Name => MatchManager.Instance.Players[PortIndex].Name;
  public int PortIndex;

  bool AliveAndActive
    => !LocalClock.Frozen()
    && Health.CurrentValue > 0;
    // Not stunned

  bool AbleToAct
    => (!AttackAbility.IsRunning || AttackAbility.CanCancel)
    && (!DiveRollAbility.IsRunning || DiveRollAbility.CanCancel)
    && (!HoverAbility.IsRunning || HoverAbility.CanCancel)
    && (!ActiveAbility || !ActiveAbility.IsRunning || ActiveAbility.CanCancel)
    && (!UltimateAbility || !UltimateAbility.IsRunning || UltimateAbility.CanCancel);

  void StopRunning() {
    if (AttackAbility.CanCancel) AttackAbility.Cancel();
    if (DiveRollAbility.CanCancel) DiveRollAbility.Cancel();
    if (HoverAbility.CanCancel) HoverAbility.Cancel();
    if (ActiveAbility && ActiveAbility.CanCancel) ActiveAbility.Cancel();
    if (UltimateAbility && UltimateAbility.CanCancel) UltimateAbility.Cancel();
  }

  public bool CanUseActiveAbility
    => AliveAndActive
    && AbleToAct
    && ActiveAbility
    && ActiveAbility.CanRun;

  public bool CanUseUltimateAbility
    => AliveAndActive
    && AbleToAct
    && UltimateAbility
    && UltimateAbility.CanRun;

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

  public bool CanMove
    => AliveAndActive
    && MoveAbility.CanRun
    && !AttackAbility.IsRunning
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

  public void UseActiveAbility(Vector2 direction) {
    StopRunning();
    ActiveAbility.Run();
    if (ActiveAbility is IAimed aimed && aimed.CanAim)
      aimed.Aim(direction);
  }

  public void UseUltimateAbility(Vector2 direction) {
    StopRunning();
    UltimateAbility.Run();
    if (UltimateAbility is IAimed aimed && aimed.CanAim)
      aimed.Aim(direction);
  }

  public void Move(Vector2 direction) {
    MoveAbility.Run();
    MoveAbility.Aim(direction);
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