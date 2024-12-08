using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DefaultExecutionOrder((int)ExecutionGroups.Managed)]
public class SpellFlowerPlantingZone : MonoBehaviour {
  public HashSet<SpellFlowerSprout> PlantedSprouts = new();
  public HashSet<SpellFlower> PlantedFlowers = new();

  public bool Occupied(Vector3 p)
    => PlantedSprouts.Any(s => s.transform.position == p)
    || PlantedFlowers.Any(s => s.transform.position == p);

  void Start() {
    SpellFlowerManager.Active.PlantingZones.Add(this);
  }

  void OnDestroy() {
    SpellFlowerManager.Active.PlantingZones.Remove(this);
  }

  void OnTriggerEnter(Collider collider) {
    if (collider.TryGetComponent(out SpellFlowerPollen pollen)) {
      SpellFlowerManager.Active.OnPollenReachLandingZone(pollen, this);
    }
  }

  void OnDrawGizmos() {
    var collider = GetComponent<Collider>();
    var color = Color.cyan;
    color.a = 0.1f;
    var size = collider.bounds.size;
    size.y = 0.05f;
    var position = collider.bounds.center;
    position.y = collider.bounds.max.y + 0.05f;
    Gizmos.color = color;
    Gizmos.DrawCube(position, size);
  }
}