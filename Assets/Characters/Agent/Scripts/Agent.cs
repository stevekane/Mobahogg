using UnityEngine;
using UnityEngine.AI;

class Agent : MonoBehaviour
{
  [SerializeField] KCharacterController CharacterController;
  [SerializeField] float MoveSpeed = 6;
  [SerializeField] float MaxAcceleration = 3;
  [SerializeField] float MaxTurnSpeed = 90;
  NavMeshPath NavMeshPath;
  Vector3[] Corners;
  int CornerCount;

  void Awake()
  {
    NavMeshPath = new();
    Corners = new Vector3[16];
  }

  void FixedUpdate()
  {
    var currentVelocity = (CharacterController.DirectVelocity.Current + CharacterController.Velocity.Current).XZ();
    Vector3? destination =
      SpawnManager.Active.Players.Count > 0
      ? SpawnManager.Active.Players[0].transform.position
      : null;
    if (destination.HasValue && Vector3.SqrMagnitude(destination.Value-transform.position) > 1)
    {
      NavMesh.CalculatePath(transform.position, destination.Value, NavMesh.AllAreas, NavMeshPath);
      CornerCount = NavMeshPath.GetCornersNonAlloc(Corners);
      if (NavMeshPath.status == NavMeshPathStatus.PathComplete)
      {
        var dt = Time.fixedDeltaTime;
        var desiredDirection = Corners[1]-Corners[0];
        var velocity = NextVelocity(
          currentVelocity,
          desiredDirection,
          MaxTurnSpeed * Mathf.Deg2Rad,
          MaxAcceleration,
          MoveSpeed,
          dt).XZ();
        CharacterController.DirectVelocity.Add(velocity);
        if (velocity.sqrMagnitude > 0)
        {
          CharacterController.Rotation.Set(Quaternion.LookRotation(velocity.normalized));
        }
      }
    }
    else
    {
      NavMeshPath.ClearCorners();
      CornerCount = NavMeshPath.GetCornersNonAlloc(Corners);
    }
    CharacterController.Acceleration.Add(Physics.gravity);
  }

  Vector3 NextVelocity(
    Vector3 velocity,
    Vector3 desiredDir,      // must be normalized, y = 0
    float maxTurnRate,       // radians per second
    float maxAccel,          // units per second^2
    float maxSpeed,          // units per second
    float dt)
  {
    velocity.y = 0f;
    desiredDir.y = 0f;
    float speed = velocity.magnitude;
    if (speed < 1e-3f)
    {
      Vector3 v = desiredDir * Mathf.Min(maxAccel * dt, maxSpeed);
      return v;
    }
    Vector3 forward = velocity / speed;
    float maxTurn = maxTurnRate * dt;
    Vector3 newForward = Vector3.RotateTowards(forward, desiredDir, maxTurn, 0f);
    float desiredSpeed = maxSpeed;
    float newSpeed = Mathf.MoveTowards(speed, desiredSpeed, maxAccel * dt);
    return newForward * newSpeed;
  }

  void OnDrawGizmos()
  {
    if (NavMeshPath == null)
      return;
    Gizmos.color = NavMeshPath.status switch
    {
      NavMeshPathStatus.PathComplete => Color.green,
      NavMeshPathStatus.PathPartial => Color.yellow,
      _ => Color.red
    };
    // Debug.Log($"Count: {CornerCount} | Status: {NavMeshPath.status}");
    for (var i = 1; i < CornerCount; i++)
    {
      Gizmos.DrawLine(Corners[i], Corners[i-1]);
    }
  }
}