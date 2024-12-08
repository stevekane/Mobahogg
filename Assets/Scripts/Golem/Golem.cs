using UnityEngine;

[DefaultExecutionOrder((int)ExecutionGroups.Managed)]
public class Golem : MonoBehaviour {
  [SerializeField] LocalClock LocalClock;
  [SerializeField] Rigidbody Rigidbody;
  [SerializeField] int FramesOfPursuit = 60 * 10;
  [SerializeField] float MaxAngleForMovement = 5;
  [SerializeField] float TurnSpeed = 360;
  [SerializeField] float MoveSpeed = 1;

  int PursuitFramesRemaining;
  public GolemAttractor CurrentAttractor { get; private set; }
  public bool IsAwake => CurrentAttractor && PursuitFramesRemaining > 0;

  void Start() {
    GolemManager.Active.Golems.Add(this);
  }

  void OnDestroy() {
    GolemManager.Active.Golems.Remove(this);
  }

  void FixedUpdate() {
    if (LocalClock.Frozen())
      return;
    if (IsAwake) {
      var df = LocalClock.DeltaFrames();
      var dt = LocalClock.DeltaTime();
      var delta = CurrentAttractor.transform.position-transform.position;
      var desiredDirection = delta.normalized;
      var desiredRotation = Quaternion.LookRotation(desiredDirection);
      var angleFromDesiredHeading = Vector3.Angle(transform.forward, desiredDirection);
      var moveSpeed = angleFromDesiredHeading <= MaxAngleForMovement ? MoveSpeed : 0;
      var nextPosition = Vector3.MoveTowards(transform.position, CurrentAttractor.transform.position, dt * moveSpeed);
      var nextRotation = Quaternion.RotateTowards(transform.rotation, desiredRotation, dt * TurnSpeed);
      Rigidbody.Move(nextPosition, nextRotation);
      PursuitFramesRemaining = Mathf.Max(0, PursuitFramesRemaining-df);
    } else {
      CurrentAttractor = null;
      PursuitFramesRemaining = 0;
    }
  }

  public void PursueAttractor(GolemAttractor golemAttractor) {
    CurrentAttractor = golemAttractor;
    PursuitFramesRemaining = FramesOfPursuit;
  }

  public void OnReachAttractor(GolemAttractor golemAttractor) {
    Debug.Log($"Golem reached {golemAttractor.name}");
    MatchManager.Instance.DefeatByGolem(golemAttractor.DefeatedTeam.TeamType);
  }
}