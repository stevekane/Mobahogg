using UnityEngine;

[DefaultExecutionOrder((int)ExecutionGroups.Managed)]
public class SpellCharge : MonoBehaviour, ICollectable {
  public Power SpellPrefab;

  void Start() {
    SpellFlowerManager.Active.SpellCharges.Add(this);
  }

  void OnDestroy() {
    SpellFlowerManager.Active.SpellCharges.Remove(this);
  }

  public void TryCollect(Collector collector, GameObject owner) {
    if (owner.TryGetComponent(out SpellHolder spellHolder) && spellHolder.TryAdd(SpellPrefab)) {
      Destroy(gameObject);
    }
  }
}