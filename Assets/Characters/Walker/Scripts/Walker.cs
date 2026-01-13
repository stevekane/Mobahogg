using System;
using Melee;
using UnityEngine;
using UnityEngine.AI;

namespace Characters.Walker
{
  enum State : uint
  {
    Spawning,
    Alive,
    Dying
  }

  class Walker : MonoBehaviour
  {
    static void Noop() { }

    [SerializeField] WalkerConfig Config;
    [SerializeField] KCharacterController CharacterController;

    Transform Target;
    State State;
    NavMeshPath NavMeshPath;
    Vector3[] Corners = new Vector3[32];
    int CornerCount;
    int Timer;

    public void OnHurt(MeleeAttackEvent meleeAttackEvent)
    {
      StartDying();
    }

    void Awake()
    {
      NavMeshPath = new();
    }

    void Start()
    {
      StartSpawning();
    }

    void StartSpawning()
    {
      State = State.Spawning;
      Timer = Config.SpawnDuration.Ticks;
    }

    void StartAlive()
    {
      State = State.Alive;
    }

    void StartDying()
    {
      State = State.Dying;
      Timer = Config.DyingDuration.Ticks;
      GetComponentInChildren<MeshRenderer>().material.color = Color.red;
    }

    void FixedUpdate()
    {
      CharacterController.Acceleration.Add(Gravity * Vector3.up);
      Action action = State switch
      {
        State.Spawning => SpawningTick,
        State.Alive => AliveTick,
        State.Dying => DyingTick,
        _ => Noop
      };
      action();
    }

    void SpawningTick()
    {
      if (Timer-- <= 0)
      {
        StartAlive();
      }
    }

    void DyingTick()
    {
      if (Timer-- <= 0)
      {
        Destroy(gameObject);
      }
    }

    Vector3 CalculateDestination()
    {
      if (Target)
        return Target.position;
      return transform.position;
    }

    Vector3 Destination;
    bool SourceOnNavMesh;
    bool DestinationOnNavMesh;
    NavMeshHit SourceNavMeshHit;
    NavMeshHit DestinationNavMeshHit;
    void AliveTick()
    {
      Target = Targeting.BestTarget(transform, SpawnManager.Active.Players);
      Destination = CalculateDestination();
      SourceOnNavMesh = NavMesh.SamplePosition(
        sourcePosition: transform.position,
        hit: out SourceNavMeshHit,
        maxDistance: .125f,
        areaMask: NavMesh.AllAreas);
      DestinationOnNavMesh = NavMesh.SamplePosition(
        sourcePosition: Destination,
        hit: out DestinationNavMeshHit,
        maxDistance: .125f,
        areaMask: NavMesh.AllAreas);
      NavMesh.CalculatePath(
        sourcePosition: transform.position,
        targetPosition: Destination,
        areaMask: NavMesh.AllAreas,
        NavMeshPath);
      CornerCount = NavMeshPath.GetCornersNonAlloc(Corners);
      bool grounded = CharacterController.IsGrounded;
      if (grounded)
      {
        CharacterController.Velocity.Set(Vector3.zero);
      }
      if (CanJump() && JumpValue() > 0)
      {
        Jump();
      }
      else if (grounded)
      {
        Move();
      }
    }

    bool CanMove()
    {
      return true;
    }

    float MoveValue()
    {
      return 1;
    }

    void Move()
    {
      transform.GetPositionAndRotation(out var currentPosition, out var currentRotation);
      var dt = Time.fixedDeltaTime;
      var targetPosition = Corners[1];
      var targetDelta = targetPosition - currentPosition;
      var targetHeading = targetDelta.normalized;
      var targetRotation =
        Mathf.Approximately(targetHeading.sqrMagnitude, 0)
        ? currentRotation
        : Quaternion.LookRotation(targetHeading);
      var nextRotation = Quaternion.RotateTowards(
        from: currentRotation,
        to: targetRotation,
        maxDegreesDelta: dt * Config.TurnSpeed);
      var nextForward = nextRotation * Vector3.forward;
      var speedFactor =
        CharacterController.IsGrounded
        ? Math.Clamp(Vector3.Dot(targetHeading, nextForward), 0, 1)
        : 1;
      CharacterController.DirectVelocity.Add(speedFactor * Config.MoveSpeed * nextForward);
      CharacterController.Rotation.Set(nextRotation);
    }

    Vector3 BallisticPosition(
    Vector3 p0,
    Vector3 horizontalVelocity,
    float initialUpwardVelocity,
    float gravity,
    float t)
    {
      Vector3 v0 = horizontalVelocity + Vector3.up * initialUpwardVelocity;
      Vector3 g = gravity * Vector3.up;
      return p0 + v0 * t + 0.5f * t * t * g;
    }

    bool CanJump() => CharacterController.IsGrounded && SourceOnNavMesh;

    [SerializeField] bool UseNavMeshRayCast;
    [SerializeField] bool UseJumpTrajectoryCheck;
    float JumpValue()
    {
      // If no path we should not jump
      if (NavMeshPath.status == NavMeshPathStatus.PathInvalid)
      {
        return 0;
      }

      Vector3 currentPosition = transform.position;
      Vector3 targetPosition = Corners[1];

      // If we have direct path to target then should not jump
      if (!NavMesh.Raycast(currentPosition, targetPosition, out var _, NavMesh.AllAreas))
      {
        return 0;
      }

      Vector3? landingPosition = null;
      Vector3 horizontalVelocity = Config.MoveSpeed * transform.forward.XZ();
      float initialUpdateVelocity = InitialVerticalJumpSpeed;
      const float MIN_JUMP_DURATION = 0.25f;
      const float MAX_JUMP_DURATION = 2;
      const float JUMP_DURATION_SAMPLE_PERIOD = 0.05f;
      const float MAX_DISTANCE_FROM_TERRAIN = 0.5f;
      // TODO: We could check that there are no potential collisions along our jump arc
      // TODO: Check if landing position enables shorter path to destination
      for (var t = MIN_JUMP_DURATION; t < MAX_JUMP_DURATION; t += JUMP_DURATION_SAMPLE_PERIOD)
      {
        var p = BallisticPosition(currentPosition, horizontalVelocity, initialUpdateVelocity, Gravity, t);
        if (NavMesh.SamplePosition(p, out var navMeshHit, MAX_DISTANCE_FROM_TERRAIN, NavMesh.AllAreas))
        {
          landingPosition = navMeshHit.position;
          Debug.DrawLine(p, landingPosition.Value, Color.green, 2);
        }
        else
        {
          Debug.DrawRay(p, Vector3.down, Color.red, 2);
        }
      }
      return landingPosition.HasValue ? 1 : 0;
    }

    [SerializeField] float InitialVerticalJumpSpeed = 25;
    [SerializeField] float Gravity = -40;
    void Jump()
    {
      CharacterController.ForceUnground.Set(true);
      // CharacterController.Velocity.SetY(InitialVerticalJumpSpeed);
      CharacterController.Velocity.Set(InitialVerticalJumpSpeed * Vector3.up + Config.MoveSpeed * transform.forward);
    }

    void OnDrawGizmos()
    {
      Gizmos.color = SourceOnNavMesh ? Color.green : Color.grey;
      Gizmos.DrawSphere(SourceNavMeshHit.position, radius: 1);
      Gizmos.color = DestinationOnNavMesh ? Color.green : Color.grey;
      Gizmos.DrawSphere(DestinationNavMeshHit.position, radius: 1);
      Gizmos.DrawLineStrip(Corners.AsSpan(0, CornerCount), looped: false);
    }
  }
}