using UnityEngine;

public class GolemZoneCollisionHandler : MonoBehaviour {
  [SerializeField] Golem Golem;

  void OnTriggerEnter(Collider other) {
    if (other.TryGetComponent(out GolemZone golemZone)) {
      Golem.OnReachAttractor(golemZone.GolemAttractor);
    }
  }
}