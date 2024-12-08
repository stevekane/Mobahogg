using UnityEngine;

[DefaultExecutionOrder((int)ExecutionGroups.Managed)]
public class SpellFlower : MonoBehaviour {
  void Start() {
    SpellFlowerManager.Active.Flowers.Add(this);
  }

  void OnDestroy() {
    SpellFlowerManager.Active.Flowers.Remove(this);
  }
}