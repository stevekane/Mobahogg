using UnityEngine;
using UnityEngine.Events;

public class TriggerEvents : MonoBehaviour {
  public UnityEvent<Collider> TriggerEnter;
  public UnityEvent<Collider> TriggerStay;
  public UnityEvent<Collider> TriggerExit;
  public LayerMask LayerMask;

  bool IsLayerInLayerMask(int layer, LayerMask layerMask) {
    return (layerMask.value & (1 << layer)) != 0;
  }

  bool Satisfied(Collider c) => IsLayerInLayerMask(c.gameObject.layer, LayerMask);

  void OnTriggerEnter(Collider c) {
    if (Satisfied(c)) {
      TriggerEnter?.Invoke(c);
    }
  }

  void OnTriggerStay(Collider c) {
    if (Satisfied(c)) {
      TriggerStay?.Invoke(c);
    }
  }

  void OnTriggerExit(Collider c) {
    if (Satisfied(c)) {
      TriggerExit?.Invoke(c);
    }
  }
}