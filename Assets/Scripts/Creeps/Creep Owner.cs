using System.Collections.Generic;
using State;
using UnityEngine;

public class CreepOwner : MonoBehaviour {
  public List<DeadCreep> Creeps;
  public void OnEnterCreepDropZone(CreepDropZone dropZone) {
    Creeps.ForEach(dropZone.EnqueueToConsume);
    Creeps.Clear();
  }
}