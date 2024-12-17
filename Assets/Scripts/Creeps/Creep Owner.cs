using System.Collections.Generic;
using UnityEngine;

public class CreepOwner : MonoBehaviour {
  [SerializeField] LocalClock LocalClock;
  [SerializeField] RopeSolver Tether;
  [SerializeField] float MoveSpeed = 10;
  [SerializeField] float TurnSpeed = 360;

  int MaxOwnable => Tether.nodeCount;

  public List<DeadCreep> DeadCreeps;

  public void OnEnterCreepDropZone(CreepDropZone dropZone) {
    // Transfer all you can to the drop zone
    for (var i = DeadCreeps.Count-1; i >= 0; i--) {
      var deadCreep = DeadCreeps[i];
      if (dropZone.TryAdd(deadCreep)) {
        DeadCreeps.RemoveAt(i);
      }
    }
  }

  public bool TryPossess(DeadCreep deadCreep) {
    if (DeadCreeps.Count < MaxOwnable) {
      DeadCreeps.Add(deadCreep);
      return true;
    } else {
      return false;
    }
  }

  void FixedUpdate() {
    var dt = LocalClock.DeltaTime();
    Tether.Simulate(dt);
    for (var i = 0; i < DeadCreeps.Count; i++) {
      var currentPosition = DeadCreeps[i].transform.position;
      var targetPosition = Tether.GetNodePosition(i);
      var position = Vector3.MoveTowards(currentPosition, targetPosition, dt * MoveSpeed);
      var currentRotation = DeadCreeps[i].transform.rotation;
      var targetRotation = Quaternion.LookRotation(Tether.GetNodeForward(i));
      var rotation = Quaternion.RotateTowards(currentRotation, targetRotation, dt * TurnSpeed);
      DeadCreeps[i].transform.SetPositionAndRotation(position, rotation);
    }
  }
}