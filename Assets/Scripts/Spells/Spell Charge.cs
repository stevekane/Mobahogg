using UnityEngine;

[DefaultExecutionOrder((int)ExecutionGroups.Managed)]
public class SpellCharge : MonoBehaviour {
  public Power SpellPrefab;

  void Start() {
    SpellFlowerManager.Active.SpellCharges.Add(this);
  }

  void OnDestroy() {
    SpellFlowerManager.Active.SpellCharges.Remove(this);
  }

  void OnTriggerStay(Collider other) {
    if (other.TryGetComponent(out SpellCollector collector)) {
      if (collector.TryCollect(SpellPrefab)) {
        Destroy(gameObject);
      }
    }
  }
}