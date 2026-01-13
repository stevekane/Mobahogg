using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[DefaultExecutionOrder(1)] // possibly puts it after navmeshagent though this may be irrelevant
class Agent : MonoBehaviour
{
  [SerializeField] NavMeshAgent NavMeshAgent;
  [SerializeField] KCharacterController CharacterController;
  [SerializeField] float MoveSpeed = 6;
  [SerializeField] float Acceleration = 3;
  [SerializeField] float Deceleration = 12;
  [SerializeField] float TurnSpeed = 90;

  /*
  Potentially useful APIs:

  NavMeshAgent
  .SamplePathPosition // look ahead
  .Raycast // detect if breaks in navmesh along in some direction
  .FindClosestEdge // seems to like... find a point near closest...edge? unsure
  .steeringTarget // either next corner or next link enter / link exit
  */

  bool CanRun() => NavMeshAgent.isOnNavMesh && CharacterController.IsGrounded;
  void Run(Vector3 direction, float speed)
  {
    CharacterController.DirectVelocity.Add(speed * direction);
  }

  void Awake()
  {
    NavMeshAgent.updatePosition = false;
    NavMeshAgent.updateRotation = false;
    NavMeshAgent.updateUpAxis = false;
  }

  [SerializeField] bool UseNavMeshAgent;
  void FixedUpdate()
  {
    var dt = Time.fixedDeltaTime;
    var currentVelocity = (CharacterController.DirectVelocity.Current + CharacterController.Velocity.Current).XZ();
    var currentSpeed = currentVelocity.magnitude;
    NavMeshAgent.nextPosition = CharacterController.transform.position;
    NavMeshAgent.destination =
      SpawnManager.Active.Players.Count > 0
      ? SpawnManager.Active.Players[0].transform.position
      : CharacterController.transform.position;
    if (NavMeshAgent.pathStatus != NavMeshPathStatus.PathComplete)
    {
      NavMeshAgent.ResetPath();
    }
    var desiredDirection = NavMeshAgent.steeringTarget;
    var desiredVelocity = NavMeshAgent.desiredVelocity.XZ();
    var desiredHeading = desiredVelocity.normalized;
    var desiredSpeed = desiredVelocity.magnitude;
    var nextHeading = CharacterController.transform.forward;
    if (!Mathf.Approximately(desiredHeading.sqrMagnitude, 0))
    {
      var currentRotation = CharacterController.transform.rotation;
      var desiredRotation = Quaternion.LookRotation(desiredHeading);
      var nextRotation = Quaternion.RotateTowards(currentRotation, desiredRotation, dt * TurnSpeed);
      nextHeading = nextRotation * Vector3.forward;
      CharacterController.Rotation.Set(nextRotation);
    }
    var acceleration = currentSpeed < desiredSpeed ? Acceleration : Deceleration;
    var nextSpeed = Mathf.MoveTowards(currentSpeed, desiredSpeed, dt * acceleration);
    CharacterController.Acceleration.Add(Physics.gravity);
    CharacterController.DirectVelocity.Add(nextSpeed * nextHeading);
  }

  void OnDrawGizmos()
  {
    if (NavMeshAgent.pathStatus == NavMeshPathStatus.PathComplete)
    {
      Gizmos.DrawLineStrip(NavMeshAgent.path.corners, looped: false);
      Gizmos.DrawSphere(NavMeshAgent.steeringTarget, 1f);
    }
  }

  void OnGUI()
  {
    GUILayout.BeginVertical("box");
    GUILayout.Label("Agent Debug");
    GUILayout.Label($"velocity: {NavMeshAgent.velocity}");
    GUILayout.Label($"desiredVelocity: {NavMeshAgent.desiredVelocity}");
    GUILayout.EndVertical();
  }
}