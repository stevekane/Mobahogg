using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DefaultExecutionOrder((int)ExecutionGroups.Managers)]
public class SpellFlowerManager : MonoBehaviour {
  public static SpellFlowerManager Active;

  [SerializeField] SpellFlowerSprout SproutPrefab;
  [SerializeField] SpellFlower FlowerPrefab;
  [SerializeField] int SpellChargesPerFlower = 3;
  [SerializeField] int SpellChargeLaunchStrength = 3;
  [SerializeField] SpellDescriptions SpellDescriptions;

  public List<SpellFlowerPlantingZone> PlantingZones = new();
  public List<SpellFlowerPollen> Pollens = new();
  public List<SpellFlowerSprout> Sprouts = new();
  public List<SpellFlower> Flowers = new();
  public List<SpellCharge> SpellCharges = new();

  void Awake() {
    Active = this;
  }

  public void OnPollenReachLandingZone(SpellFlowerPollen pollen, SpellFlowerPlantingZone zone) {
    var collider = zone.GetComponent<Collider>();
    var position = new Vector3(
      Mathf.RoundToInt(pollen.transform.position.x),
      Mathf.RoundToInt(collider.bounds.max.y),
      Mathf.RoundToInt(pollen.transform.position.z));
    if (!zone.Occupied(position)) {
      var sprout = Instantiate(
        SproutPrefab,
        position,
        Quaternion.LookRotation(Vector3.back),
        transform);
      zone.PlantedSprouts.Add(sprout);
    }
    Destroy(pollen.gameObject);
  }

  public void OnSproutReadyToGrow(SpellFlowerSprout sprout) {
    var zone = PlantingZones.First(z => z.PlantedSprouts.Contains(sprout));
    var flower = Instantiate(
      FlowerPrefab,
      sprout.transform.position,
      sprout.transform.rotation,
      transform);
    zone.PlantedSprouts.Remove(sprout);
    zone.PlantedFlowers.Add(flower);
    Destroy(sprout.gameObject);
  }

  public void OnFlowerOpen(SpellFlower flower) {
    for (var i = 0; i < SpellChargesPerFlower; i++) {
      var spellIndex = Random.Range(0, SpellDescriptions.Value.Count);
      var spellChargePrefab = SpellDescriptions.Value[spellIndex].SpellCharge;
      var charge = Instantiate(
        spellChargePrefab,
        flower.transform.position,
        flower.transform.rotation,
        transform);
      var rigidBody = charge.GetComponent<Rigidbody>();
      var direction = Random.onUnitSphere;
      direction.y = 0;
      direction.Normalize();
      direction.y = 1;
      direction.Normalize();
      rigidBody.AddForce(SpellChargeLaunchStrength * direction, ForceMode.Impulse);
    }
  }
}