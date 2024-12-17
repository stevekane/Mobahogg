using System.Threading;
using Cysharp.Threading.Tasks;
using State;
using UnityEngine;

[DefaultExecutionOrder((int)ExecutionGroups.Managed)]
public class Player : MonoBehaviour {
  [Header("Settings")]
  [SerializeField] AbilitySettings Settings;

  [Header("Character Control")]
  [SerializeField] KCharacterController CharacterController;

  [Header("Child References")]
  [SerializeField] LocalClock LocalClock;
  [SerializeField] Animator Animator;
  [SerializeField] Health Health;

  // TODO: Would it make sense to use the already-available "name" property Unity has?
  public string Name => MatchManager.Instance.Players[PortIndex].Name;
  public AttackAbility AttackAbility;
  public SpinAbility SpinAbility;
  public SpellCastAbility SpellCastAbility;
  public MoveAbility MoveAbility;
  public TurnAbility TurnAbility;
  public int PortIndex;

  void OnHurt(Combatant attacker) {
    Health.Change(-1);
  }

  void Start() {
    LivesManager.Active.Players.AddFirst(this);
  }

  void OnDestroy() {
    LivesManager.Active.Players.Remove(this);
  }

  void FixedUpdate() {
    if (!LocalClock.Frozen() && (Health.CurrentValue <= 0 || transform.position.y <= -10)) {
      LivesManager.Active.OnPlayerDeath(this);
    }
  }

  #region JUMP
  public bool CanJump() => CharacterController.IsGrounded;
  public bool TryJump() {
    if (CanJump()) {
      DashCancelTokenSource?.Cancel();
      DashCancelTokenSource?.Dispose();
      DashCancelTokenSource = null;
      DashFramesRemaining = 0;
      CharacterController.Launch(Settings.InitialJumpSpeed(Physics.gravity.y) * Vector3.up);
      return true;
    } else {
      return false;
    }
  }
  #endregion

  // TODO: Could move to library?
  Vector3 TruncateByMagnitude(Vector3 v, float maxMagnitude) =>
    Mathf.Min(v.magnitude, maxMagnitude) * v.normalized;
  #region DASH
  int DashFramesRemaining;
  CancellationTokenSource DashCancelTokenSource;
  public bool CanDash() => CharacterController.IsGrounded && !IsDashing() && !AttackAbility.IsRunning;
  public bool IsDashing() => DashFramesRemaining > 0;
  public bool TryDash() {
    if (CanDash()) {
      BeginDash();
      return true;
    } else {
      return false;
    }
  }
  void BeginDash() {
    DashCancelTokenSource?.Cancel();
    DashCancelTokenSource?.Dispose();
    DashCancelTokenSource = CancellationTokenSource.CreateLinkedTokenSource(this.destroyCancellationToken);
    RunDash(DashCancelTokenSource.Token).ContinueWith(EndDash).Forget();
  }
  void CancelDash() {
    DashCancelTokenSource?.Cancel();
    DashCancelTokenSource?.Dispose();
    DashCancelTokenSource = null;
  }
  void EndDash() {
    CancelDash();
  }
  async UniTask RunDash(CancellationToken token) {
    DashFramesRemaining = Settings.DashTotalFrames;
    try {
      var dt = Time.fixedDeltaTime;
      var dashSpeed = Settings.DashSpeed(dt);
      while (DashFramesRemaining-- > 0) {
        var currentVelocity = CharacterController.Velocity;
        var desiredVelocity = dashSpeed * CharacterController.Forward;
        var targetVelocity = (desiredVelocity-currentVelocity).XZ();
        var maxMoveSpeed = dashSpeed;
        var nextVelocity = TruncateByMagnitude(targetVelocity, maxMoveSpeed);
        CharacterController.Acceleration += 1/dt * nextVelocity;
        await UniTask.NextFrame(token);
      }
    } finally {
      DashFramesRemaining = 0;
    }
  }
  #endregion
}