using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder((int)ExecutionGroups.Managers)]
public class CreepManager : MonoBehaviour {
  public static CreepManager Active;

  [SerializeField] LocalClock LocalClock;
  [SerializeField] Creep CreepPrefab;
  [SerializeField] DeadCreep DeadCreepPrefab;
  [SerializeField] int MAX_LIVING_CREEPS = 15;
  [SerializeField] int CREEP_SPAWN_FRAME_INTERVAL = 300;
  public List<Creep> LivingCreeps = new(128);

  int FramesTillNextSpawn;

  void Awake() {
    Active = this;
  }

  public void OnCreepDeath(Creep creep, CreepOwner owner) {
    Destroy(creep.gameObject);
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