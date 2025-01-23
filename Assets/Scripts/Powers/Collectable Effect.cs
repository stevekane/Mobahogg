using UnityEngine;

public class CollectableEffect : MonoBehaviour, ICollectable {
  [SerializeField] Effect EffectPrefab;

  public void TryCollect(Collector collector, GameObject owner) {
    owner.GetComponent<EffectManager>().Register(Instantiate(EffectPrefab));
    Destroy(gameObject);
  }
}