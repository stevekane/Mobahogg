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

    // TODO: JUST FOR TESTING
    [SerializeField] AimAssistQuery Query;
    void FixedUpdate() {
      var target = AimAssistManager.Instance.BestTarget(this, Query);
      if (target) {
        Debug.DrawLine(transform.position + Vector3.up, target.transform.position + Vector3.up);
      }
    }
  }
}