using UnityEngine;

namespace AimAssist {
  [DefaultExecutionOrder((int)ExecutionGroups.Managed)]
  public class AimAssistTarget : MonoBehaviour {
    [SerializeField] GameObject Owner;

    public bool OwnedBy(GameObject go) => Owner == go;

    void Start() {
      AimAssistManager.Instance.Targets.Add(this);
    }

    void OnDestroy() {
      AimAssistManager.Instance?.Targets.Remove(this);
    }
  }
}