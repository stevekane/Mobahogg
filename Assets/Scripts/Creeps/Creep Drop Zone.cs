using System.Collections.Generic;
using UnityEngine;

public class CreepDropZone : MonoBehaviour {
  [SerializeField] TeamType TeamType;
  [SerializeField] BoxCollider BoxCollider;
  [SerializeField] int FramesPerConsumption = 60;

  int FramesTillConsumption;
  Queue<DeadCreep> CreepsToConsume = new(128);

  public void EnqueueToConsume(DeadCreep deadCreep) {
    var position = transform.TransformPoint(2 * Random.onUnitSphere);
    Debug.Log(position);
    deadCreep.State = DeadCreepState.PreConsume;
    deadCreep.Owner = null;
    deadCreep.Destination = position;
    FramesTillConsumption = CreepsToConsume.Count > 0 ? FramesTillConsumption : FramesPerConsumption;
    CreepsToConsume.Enqueue(deadCreep);
  }

  void FixedUpdate() {
    if (FramesTillConsumption <= 0 && CreepsToConsume.TryDequeue(out var deadCreep)) {
      WorldSpaceMessageManager.Instance.SpawnMessage(
        message: "DEVOURED",
        position: deadCreep.transform.position + Vector3.up,
        lifetime: 3);
      FramesTillConsumption = FramesPerConsumption;
      MatchManager.Instance.DeductRequiredResource(TeamType);
      Destroy(deadCreep.gameObject);
    } else {
      FramesTillConsumption = Mathf.Max(0, FramesTillConsumption-1);
    }
  }

  void OnDrawGizmos() {
    var color = TeamType == TeamType.Turtles ? Color.green : Color.red;
    color.a = 0.3f;
    var position = transform.TransformPoint(BoxCollider.center);
    Gizmos.color = color;
    Gizmos.DrawCube(position, BoxCollider.size);
  }
}