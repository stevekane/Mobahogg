using UnityEngine;

public class DeadCreep : MonoBehaviour {
  [SerializeField] LocalClock LocalClock;
  [SerializeField] float FollowStrength = 1;
  [SerializeField] float FollowDamping = 0.95f;

  public CreepManager CreepManager;
  public CreepOwner Owner;
  public bool Consume;
  public Vector3 Destination;

  void FixedUpdate() {
    var target = Owner
      ? Owner.transform.TransformPoint(Vector3.back)
      : Destination;
    var delta = target-transform.position;
    var dt = LocalClock.DeltaTime();
    var acceleration = FollowStrength * delta;
    var velocity = dt * acceleration;
    velocity *= FollowDamping;
    transform.Translate(dt * velocity);
  }
}