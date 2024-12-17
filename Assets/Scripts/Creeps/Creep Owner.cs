using System.Collections.Generic;
using UnityEngine;

public class CreepOwner : MonoBehaviour {
  [SerializeField] LocalClock LocalClock;
  [SerializeField] RopeSolver Tether;
  [SerializeField] float MoveSpeed = 10;
  [SerializeField] float TurnSpeed = 360;

  public List<DeadCreep> Creeps;
  public void OnEnterCreepDropZone(CreepDropZone dropZone) {
    Creeps.ForEach(dropZone.EnqueueToConsume);
    Creeps.Clear();
  }

  void FixedUpdate() {
    var dt = LocalClock.DeltaTime();
    Tether.Simulate(dt);
    for (var i = 0; i < Creeps.Count; i++) {
      var currentPosition = Creeps[i].transform.position;
      var targetPosition = Tether.GetNodePosition(i);
      var position = Vector3.MoveTowards(currentPosition, targetPosition, dt * MoveSpeed);
      var currentRotation = Creeps[i].transform.rotation;
      var targetRotation = Quaternion.LookRotation(Tether.GetNodeForward(i));
      var rotation = Quaternion.RotateTowards(currentRotation, targetRotation, dt * TurnSpeed);
      Creeps[i].transform.SetPositionAndRotation(position, rotation);
    }
  }
}