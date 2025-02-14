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
  [SerializeField] Vector2 SpawnMin = new(-20,-20);
  [SerializeField] Vector2 SpawnMax = new(20,20);

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
  // Additional consideration, eventually damage is dealt through spells
  // as well. therefore using a meleeAttack is unlikely to be the right approach
  public void OnOwnerDeath(CreepOwner creepOwner) {
    var lastAttacker = creepOwner.GetComponent<Combatant>().LastAttacker;
    if (lastAttacker) {
      var newOwner = lastAttacker.GetComponent<CreepOwner>();
      for (var i = creepOwner.DeadCreeps.Count-1; i >= 0; i--) {
        var deadCreep = creepOwner.DeadCreeps[i];
        if (newOwner.TryPossess(deadCreep)) {
          creepOwner.DeadCreeps.RemoveAt(i);
        }
      }
    }
    creepOwner.DeadCreeps.ForEach(c => Destroy(c.gameObject, 1));
    creepOwner.DeadCreeps.Clear();
  }

  public void OnCreepDeath(Creep creep, CreepOwner owner) {
    var position = creep.transform.position;
    var rotation = creep.transform.rotation;
    var deadCreep = Instantiate(DeadCreepPrefab, position, rotation, transform);
    owner.DeadCreeps.Add(deadCreep);
    LivingCreeps.Remove(creep);
    Destroy(creep.gameObject);
  }

  public void SpawnCreep(Vector3 position) {
    LivingCreeps.Add(Instantiate(CreepPrefab, position, Quaternion.identity, transform));
  }

  void FixedUpdate() {
    if (FramesTillNextSpawn <= 0 && LivingCreeps.Count < MAX_LIVING_CREEPS) {
      var location = new Vector3(Random.Range(SpawnMin.x,SpawnMax.x), 0, Random.Range(SpawnMin.y,SpawnMax.y));
      var spawnLocation = TerrainManager.Instance.SamplePoint(location);
      if (spawnLocation.HasValue) {
        SpawnCreep(spawnLocation.Value.Point);
      }
      FramesTillNextSpawn = CREEP_SPAWN_FRAME_INTERVAL;
    }
    FramesTillNextSpawn = Mathf.Max(0, FramesTillNextSpawn-LocalClock.DeltaFrames());
  }
}