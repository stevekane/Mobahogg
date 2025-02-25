using System.Linq;
using Abilities;
using State;
using UnityEngine;

[DefaultExecutionOrder((int)ExecutionGroups.Managed)]
public class Player : MonoBehaviour {
  [SerializeField] AbilityManager AbilityManager;
  [SerializeField] LocalClock LocalClock;
  [SerializeField] Health Health;
  [SerializeField] AttackAbility AttackAbility;
  [SerializeField] SpellCastAbility SpellCastAbility;
  [SerializeField] DiveRollAbility DiveRollAbility;
  [SerializeField] JumpAbility JumpAbility;
  [SerializeField] MoveAbility MoveAbility;
  [SerializeField] HoverAbility HoverAbility;

  public Ability PowerActiveAbility;
  public Ability PowerUltimateAbility;
  public Effect PowerPassiveEffect;

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
    && (!PowerActiveAbility || !PowerActiveAbility.IsRunning || PowerActiveAbility.CanCancel)
    && (!PowerUltimateAbility || !PowerUltimateAbility.IsRunning || PowerUltimateAbility.CanCancel);

  void StopRunning() {
    if (AttackAbility.CanCancel) AttackAbility.Cancel();
    if (DiveRollAbility.CanCancel) DiveRollAbility.Cancel();
    if (HoverAbility.CanCancel) HoverAbility.Cancel();
    if (PowerActiveAbility && PowerActiveAbility.CanCancel) PowerActiveAbility.Cancel();
    if (PowerUltimateAbility && PowerUltimateAbility.CanCancel) PowerUltimateAbility.Cancel();
  }

  public bool CanUseActiveAbility
    => AliveAndActive
    && AbleToAct
    && PowerActiveAbility
    && PowerActiveAbility.CanRun;

  public bool CanUseUltimateAbility
    => AliveAndActive
    && AbleToAct
    && PowerUltimateAbility
    && PowerUltimateAbility.CanRun;

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
    && AbilityManager.CanRun(MoveAbility);

  bool Steerable(RegisteredAbility ra) =>
    ra.Ability.IsRunning && ra.Ability is ISteered steered && steered.CanSteer;
  public bool CanSteer
    => AbilityManager.RegisteredAbilities.Any(Steerable);

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
    DiveRollAbility.Aim(direction);
  }

  public void Attack(Vector2 direction) {
    StopRunning();
    AttackAbility.Run();
    AttackAbility.Aim(direction);
  }

  public void UseActiveAbility(Vector2 direction) {
    StopRunning();
    PowerActiveAbility.Run();
    if (PowerActiveAbility is IAimed aimed && aimed.CanAim)
      aimed.Aim(direction);
  }

  public void ReleaseActiveAbility() {
    if (PowerActiveAbility is IHeld held && held.CanRelease) {
      held.Release();
    }
  }

  public void UseUltimateAbility(Vector2 direction) {
    StopRunning();
    PowerUltimateAbility.Run();
    if (PowerUltimateAbility is IAimed aimed && aimed.CanAim)
      aimed.Aim(direction);
  }

  public void Move(Vector2 direction) {
    MoveAbility.Run();
    MoveAbility.Aim(direction);
  }

  public void Steer(Vector2 direction) {
    AbilityManager.RegisteredAbilities.ForEach(ra => {
      if (ra.Ability.IsRunning && ra.Ability is ISteered steered && steered.CanSteer) {
        steered.Steer(direction);
      }
    });
  }

  void Start() {
    LivesManager.Active.AddPlayer(this);
  }

  void OnDestroy() {
    LivesManager.Active.RemovePlayer(this);
  }
}