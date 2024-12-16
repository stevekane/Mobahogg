using UnityEngine;

namespace AimAssist {
  [DefaultExecutionOrder((int)ExecutionGroups.Managed)]
  public class AimAssistTargeter : MonoBehaviour {
    void Start() {
      AimAssistManager.Instance.Assisted.Add(this);
    }

    void OnDestroy() {
      AimAssistManager.Instance.Assisted.Remove(this);
    }
  }
}