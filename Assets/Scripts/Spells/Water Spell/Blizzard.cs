using UnityEngine;

public class Blizzard : MonoBehaviour {
  void OnTriggerStay(Collider other) {
    if (other.TryGetComponent(out SpellAffected spellAffected) && spellAffected.MoveSpeed) {
      spellAffected.MoveSpeed.Mul(0.5f);
    }
  }
}