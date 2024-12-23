using UnityEngine;

[DefaultExecutionOrder((int)ExecutionGroups.Managed)]
public class CannonTarget : MonoBehaviour {
  void Awake() {
    CannonManager.Instance.Targets.Add(this);
  }

  void OnDestroy() {
    CannonManager.Instance.Targets.Remove(this);
  }
}