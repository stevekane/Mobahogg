using System.Collections.Generic;
using UnityEngine;

public class CreepManager : MonoBehaviour {
  public static CreepManager Active;

  [SerializeField] LocalClock LocalClock;
  [SerializeField] Creep CreepPrefab;
  [SerializeField] DeadCreep DeadCreepPrefab;
  [SerializeField] int MAX_LIVING_CREEPS = 15;
  [SerializeField] int CREEP_SPAWN_FRAME_INTERVAL = 300;

  int FramesTillNextSpawn;
  List<Creep> LivingCreeps = new(128);

  // Sort of a per-battle psuedo-singleton... maybe there is some better way?
  void Awake() {
    Active = this;
  }

  public void OnCreepDeath(Creep creep, CreepOwner owner) {
    Debug.Log($"{creep.name} was killed by {owner?.name}");
    Destroy(creep.gameObject);
    LivingCreeps.Remove(creep);
    var position = creep.transform.position;
    var rotation = creep.transform.rotation;
    var deadCreep = Instantiate(DeadCreepPrefab, position, rotation, transform);
    deadCreep.CreepManager = this;
    deadCreep.State = DeadCreepState.Owned;
    deadCreep.Owner = owner;
    owner.Creeps.Add(deadCreep);
  }

  public void SpawnCreep(Vector3 position) {
    LivingCreeps.Add(Instantiate(CreepPrefab, position, Quaternion.identity, transform));
  }

  void FixedUpdate() {
    if (FramesTillNextSpawn <= 0 && LivingCreeps.Count < MAX_LIVING_CREEPS) {
      var location = new Vector3(Random.Range(-10,10), 0, Random.Range(-5,5));
      SpawnCreep(location);
      FramesTillNextSpawn = CREEP_SPAWN_FRAME_INTERVAL;
    }
    FramesTillNextSpawn = Mathf.Max(0, FramesTillNextSpawn-LocalClock.DeltaFrames());
  }
}