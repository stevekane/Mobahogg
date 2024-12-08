using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DefaultExecutionOrder((int)ExecutionGroups.Managers)]
public class SpellFlowerManager : MonoBehaviour {
  public static SpellFlowerManager Active;

  [SerializeField] SpellFlowerSprout SproutPrefab;
  [SerializeField] SpellFlower FlowerPrefab;

  public List<SpellFlowerPlantingZone> PlantingZones = new();
  public List<SpellFlowerPollen> Pollens = new();
  public List<SpellFlowerSprout> Sprouts = new();
  public List<SpellFlower> Flowers = new();

  void Awake() {
    Active = this;
  }

  public void OnPollenReachLandingZone(SpellFlowerPollen pollen, SpellFlowerPlantingZone zone) {
    var collider = zone.GetComponent<Collider>();
    var position = new Vector3(
      Mathf.RoundToInt(pollen.transform.position.x),
      Mathf.RoundToInt(collider.bounds.max.y),
      Mathf.RoundToInt(pollen.transform.position.z));
    Destroy(pollen.gameObject);
    if (!zone.Occupied(position)) {
      var sprout = Instantiate(
        SproutPrefab,
        position,
        Quaternion.LookRotation(Vector3.back),
        transform);
      zone.PlantedSprouts.Add(sprout);
    }
  }

  public void OnSproutReadyToGrow(SpellFlowerSprout sprout) {
    var zone = PlantingZones.First(z => z.PlantedSprouts.Contains(sprout));
    zone.PlantedSprouts.Remove(sprout);
    var flower = Instantiate(
      FlowerPrefab,
      sprout.transform.position,
      sprout.transform.rotation,
      transform);
    zone.PlantedFlowers.Add(flower);
    Destroy(sprout.gameObject);
  }
}