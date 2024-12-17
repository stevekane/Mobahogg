using System.Collections.Generic;
using Melee;
using State;
using UnityEngine;

public class CreepDropZone : MonoBehaviour {
  [SerializeField] Animator Animator;
  [SerializeField] Transform SacrificeTransformPrefab;
  [SerializeField] Team Team;
  [SerializeField] LocalClock LocalClock;
  [SerializeField] BoxCollider BoxCollider;
  [SerializeField] int FramesPerConsumption = 60;
  [SerializeField] List<Transform> SacrificeTransforms;
  [SerializeField] float MoveSpeed = 10;
  [SerializeField] float TurnSpeed = 360;
  [SerializeField] Timeval ClosedDuration = Timeval.FromSeconds(3);

  bool Closed => ClosedFramesRemaining > 0;
  int ClosedFramesRemaining;
  int Consumed;
  int FramesTillConsumption;
  List<DeadCreep> DeadCreeps = new(24);

  void Start() {
    var ri = Quaternion.Euler(0, -60, 0);
    var rf = Quaternion.Euler(0, 60, 0);
    var count = 11;
    for (var i = 0; i <= (count-1); i++) {
      var r = Quaternion.Slerp(ri, rf, (float)i/count);
      var position = transform.position + 5 * (r * transform.forward);
      var rotation = Quaternion.LookRotation((transform.position - position).normalized);
      var sacrificeTransform = Instantiate(
        SacrificeTransformPrefab,
        position,
        rotation,
        CreepManager.Active.transform);
      SacrificeTransforms.Add(sacrificeTransform);
    }
    MatchManager.Instance.SetRequiredResources(Team.TeamType, SacrificeTransforms.Count);
  }

  public void OnHurt(MeleeAttackEvent attackEvent) {
    Debug.Log("OnHurt DropZone");
    ClosedFramesRemaining = ClosedDuration.Ticks;
  }

  public bool TryAdd(DeadCreep deadCreep) {
    if (DeadCreeps.Count < SacrificeTransforms.Count) {
      FramesTillConsumption = DeadCreeps.Count > 0 ? FramesTillConsumption : FramesPerConsumption;
      DeadCreeps.Add(deadCreep);
      return true;
    } else {
      return false;
    }
  }

  void FixedUpdate() {
    var dt = LocalClock.DeltaTime();
    for (var i = 0; i < DeadCreeps.Count; i++) {
      var currentPosition = DeadCreeps[i].transform.position;
      var targetPosition = SacrificeTransforms[i].position;
      var position = Vector3.MoveTowards(currentPosition, targetPosition, dt * MoveSpeed);
      var currentRotation = DeadCreeps[i].transform.rotation;
      var targetRotation = SacrificeTransforms[i].rotation;
      var rotation = Quaternion.RotateTowards(currentRotation, targetRotation, dt * TurnSpeed);
      DeadCreeps[i].transform.SetPositionAndRotation(position, rotation);
    }
    if (!Closed && FramesTillConsumption <= 0 && Consumed < DeadCreeps.Count) {
      var deadCreep = DeadCreeps[Consumed];
      WorldSpaceMessageManager.Instance.SpawnMessage(
        message: "DEVOURED",
        position: deadCreep.transform.position + Vector3.up,
        lifetime: 3);
      Consumed++;
      FramesTillConsumption = FramesPerConsumption;
      MatchManager.Instance.DeductRequiredResource(Team.TeamType);
      // TODO: Probably want to like... do something here like play a throwing animation or whatever
      // Destroy(deadCreep.gameObject);
    } else {
      FramesTillConsumption = Mathf.Max(0, FramesTillConsumption-LocalClock.DeltaFrames());
    }
    ClosedFramesRemaining = Mathf.Max(0, ClosedFramesRemaining-LocalClock.DeltaFrames());
    Animator.SetBool("Closed", Closed);
  }

  void OnDrawGizmos() {
    var team = GetComponent<Team>();
    var color = team.TeamType == TeamType.Turtles ? Color.green : Color.red;
    color.a = 0.3f;
    var position = transform.TransformPoint(BoxCollider.center);
    Gizmos.color = color;
    Gizmos.DrawCube(position, BoxCollider.size);
  }
}