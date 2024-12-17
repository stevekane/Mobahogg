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

  // TODO: Better way to do this would be CreepOwner subscribes to Combatant.OnHurt
  // And stores its own notion of last attacker which it uses here
  // Not sure though because this still does create some link between an element
  // of the Creep system and the combat system... maybe not avoidable?
  // They would have to communicate via some data after all...
  public void OnOwnerDeath(CreepOwner creepOwner) {
    var lastAttacker = creepOwner.GetComponent<Combatant>().LastAttacker;
    if (lastAttacker) {
      var newOwner = lastAttacker.GetComponent<CreepOwner>();
      creepOwner.Creeps.ForEach(newOwner.Creeps.Add);
    } else {
      creepOwner.Creeps.ForEach(c => Destroy(c.gameObject));
    }
    creepOwner.Creeps.Clear();
  }

  public void OnCreepDeath(Creep creep, CreepOwner owner) {
    var position = creep.transform.position;
    var rotation = creep.transform.rotation;
    var deadCreep = Instantiate(DeadCreepPrefab, position, rotation, transform);
    owner.Creeps.Add(deadCreep);
    Destroy(creep.gameObject);
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