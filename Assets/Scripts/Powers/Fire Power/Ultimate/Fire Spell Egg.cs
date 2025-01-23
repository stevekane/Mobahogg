using UnityEngine;

public class FireSpellEgg : MonoBehaviour {
  public readonly EventSource OnDetonate = new();
  public readonly EventSource OnCollision = new();

  public GameObject Owner;

  bool ValidExplosionTarget(Collider c)
    => c.gameObject != Owner
    && c.GetComponent<Creep>() == null;

  void OnTriggerEnter(Collider other) {
    if (ValidExplosionTarget(other)) {
      OnCollision.Fire();
    }
  }
}