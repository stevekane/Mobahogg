using UnityEngine;

[DefaultExecutionOrder((int)ExecutionGroups.Managed)]
public class SpellCharge : MonoBehaviour {
  [SerializeField] Spell Spell;

  void Start() {
    SpellFlowerManager.Active.SpellCharges.Add(this);
  }

  void OnDestroy() {
    SpellFlowerManager.Active.SpellCharges.Remove(this);
  }

  void OnTriggerEnter(Collider other) {
    if (other.gameObject.CompareTag("Ground") &&
        TryGetComponent(out Rigidbody rigidbody) &&
        rigidbody.linearVelocity.y <= 0) {
      var position = transform.position;
      position.y = Mathf.CeilToInt(position.y);
      rigidbody.isKinematic = true;
    } else if (other.TryGetComponent(out SpellCollector collector)) {
      if (collector.TryCollect(Spell)) {
        Destroy(gameObject);
      }
    }
  }
}