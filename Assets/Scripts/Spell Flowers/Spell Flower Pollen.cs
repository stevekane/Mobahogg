using UnityEngine;

/*
When we hit, we want to identify the nearest "correct" region that can support
a spell flower.

If there is already a spell flower of any kind here, we will not spawn anything
but will simply poof out of existence.
*/
[DefaultExecutionOrder((int)ExecutionGroups.Managed)]
public class SpellFlowerPollen : MonoBehaviour {
  void Start() {
    SpellFlowerManager.Active.Pollens.Add(this);
  }

  void OnDestroy() {
    SpellFlowerManager.Active.Pollens.Remove(this);
  }
}