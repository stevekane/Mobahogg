using UnityEngine;

public enum DeadCreepState {
  Free,
  Owned,
  PreConsume,
  Consumed
}

public class DeadCreep : MonoBehaviour {
  [SerializeField] LocalClock LocalClock;
  [SerializeField] float FollowStrength = 1;
  [SerializeField] float FollowDamping = .95f;

  public DeadCreepState State;
  public CreepManager CreepManager;
  public CreepOwner Owner;
  public Vector3 Destination;

  void FixedUpdate() {
    var target = State switch {
      DeadCreepState.Owned => Owner.transform.TransformPoint(Vector3.back),
      DeadCreepState.PreConsume => Destination,
      _ => transform.position
    };
    var delta = target-transform.position;
    var dt = LocalClock.DeltaTime();
    var acceleration = FollowStrength * delta;
    var velocity = dt * acceleration;
    velocity *= FollowDamping;
    transform.Translate(dt * velocity);
  }
}