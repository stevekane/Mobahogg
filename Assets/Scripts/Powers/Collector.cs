using UnityEngine;

public class Collector : MonoBehaviour {
  [SerializeField] GameObject Owner;

  void OnTriggerStay(Collider collider) {
    ICollectable collectable = null;
    if (collider.TryGetComponent(out collectable) || collider.TryGetComponentInParent(out collectable)) {
      collectable.TryCollect(this, Owner);
    }
  }
}

public interface ICollectable {
  public void TryCollect(Collector collector, GameObject owner);
}