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

  public AttackAbility AttackAbility;
  public SpinAbility SpinAbility;
  public SpellCastAbility SpellCastAbility;
  public float MoveSpeed;
  public int PortIndex;

  void OnHurt(Combatant attacker) {
    Health.Change(-1);
  }

  void Start() {
    LivesManager.Active.Players.AddFirst(this);
    MoveSpeed = Settings.GroundMoveSpeed;
  }

  void OnDestroy() {
    LivesManager.Active.Players.Remove(this);
  }

  void FixedUpdate() {
    if (!LocalClock.Frozen() && Health.Value <= 0) {
      LivesManager.Active.OnPlayerDeath(this);
    }
  }

  Vector3 TruncateByMagnitude(Vector3 v, float maxMagnitude) =>
    Mathf.Min(v.magnitude, maxMagnitude) * v.normalized;

  #region JUMP
  public bool CanJump() => CharacterController.IsGrounded;
  public bool TryJump() {
    if (CanJump()) {
      DashCancelTokenSource?.Cancel();
      DashCancelTokenSource?.Dispose();
      DashCancelTokenSource = null;
      DashFramesRemaining = 0;
      CharacterController.Launch(Settings.InitialJumpSpeed(Physics.gravity.y) / Time.fixedDeltaTime * Vector3.up);
      return true;
    } else {
      return false;
    }
  }
  #endregion

  #region MOVE
  public bool CanMove() => true;
  public bool TryMove(Vector2 value) {
    if (CanMove()) {
      var currentVelocity = CharacterController.PhysicsVelocity.XZ();
      var desiredVelocity = MoveSpeed * value.XZ().normalized;
      var maxMoveSpeed = CharacterController.IsGrounded
        ? Settings.GroundMoveSpeed
        : Settings.AirMoveSpeed(CharacterController.PhysicsVelocity);
      var targetVelocity = (desiredVelocity-currentVelocity).XZ();
      var steeringVelocity = TruncateByMagnitude(targetVelocity, maxMoveSpeed);
      CharacterController.PhysicsAcceleration += steeringVelocity / Time.fixedDeltaTime;
      CharacterController.Forward = desiredVelocity.magnitude > 0 ? desiredVelocity : CharacterController.Forward;
      return true;
    } else {
      return false;
    }
  }
  #endregion

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
        var currentVelocity = CharacterController.PhysicsVelocity;
        var desiredVelocity = dashSpeed * CharacterController.Forward;
        var targetVelocity = (desiredVelocity-currentVelocity).XZ();
        var maxMoveSpeed = dashSpeed;
        var nextVelocity = TruncateByMagnitude(targetVelocity, maxMoveSpeed);
        CharacterController.PhysicsAcceleration += 1/dt * nextVelocity;
        await UniTask.NextFrame(token);
      }
    } finally {
      DashFramesRemaining = 0;
    }
  }
  #endregion
}