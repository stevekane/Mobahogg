using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class Player : MonoBehaviour {
  [Header("Settings")]
  [SerializeField] AbilitySettings Settings;

  [Header("Character Control")]
  [SerializeField] KCharacterController CharacterController;

  [Header("Child References")]
  [SerializeField] Hitbox AttackHitbox;
  [SerializeField] Animator Animator;

  // TODO: Obviously this is all trash
  public void OnHurt(Combatant attacker) {
    Debug.Log("Help I been hit");
    Animator.SetTrigger("Hit Flinch");
    GetComponentInChildren<HitStop>().FramesRemaining = 10;
    attacker.GetComponentInChildren<HitStop>().FramesRemaining = 10;
  }

  public float MoveSpeed;

  void Start() {
    InitializeAttack();
    MoveSpeed = Settings.GroundMoveSpeed;
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
  public bool CanDash() => CharacterController.IsGrounded && !IsDashing() && !IsAttacking();
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

  #region ATTACK
  CancellationTokenSource AttackCancelTokenSource;
  public bool CanAttack() => CharacterController.IsGrounded && DashFramesRemaining <= 0 && !IsAttacking();
  public bool IsAttacking() => AttackCancelTokenSource != null;
  public bool TryAttack() {
    if (CanAttack()) {
      BeginAttack();
      return true;
    } else {
      return false;
    }
  }
  public void InitializeAttack() {
    AttackHitbox.CollisionEnabled = false;
  }
  void BeginAttack() {
    AttackCancelTokenSource?.Cancel();
    AttackCancelTokenSource?.Dispose();
    AttackCancelTokenSource = CancellationTokenSource.CreateLinkedTokenSource(this.destroyCancellationToken);
    RunAttack(AttackCancelTokenSource.Token).ContinueWith(EndAttack).Forget();
  }
  void CancelAttack() {
    AttackCancelTokenSource?.Cancel();
    AttackCancelTokenSource?.Dispose();
    AttackCancelTokenSource = null;
  }
  void EndAttack() {
    CancelAttack();
  }
  async UniTask RunAttack(CancellationToken token) {
    try {
      Animator.SetTrigger("Attack");
      MoveSpeed = 0;
      await UniTask.DelayFrame(Settings.WindupAttackFrames, PlayerLoopTiming.FixedUpdate, token);
      AttackHitbox.CollisionEnabled = true;
      await UniTask.DelayFrame(Settings.ActiveAttackFrames, PlayerLoopTiming.FixedUpdate, token);
      AttackHitbox.CollisionEnabled = false;
      await UniTask.DelayFrame(Settings.RecoveryAttackFrames, PlayerLoopTiming.FixedUpdate, token);
    } finally {
      MoveSpeed = Settings.GroundMoveSpeed;
      AttackHitbox.CollisionEnabled = false;
    }
  }
  #endregion
}