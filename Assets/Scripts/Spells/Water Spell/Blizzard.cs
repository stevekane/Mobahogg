using UnityEngine;

public class Blizzard : MonoBehaviour {
  [SerializeField] WaterSpellSettings Settings;

  void OnTriggerStay(Collider other) {
    if (other.TryGetComponent(out SpellAffected spellAffected)) {
      spellAffected.MultiplySpeed(Settings.SlowFraction);
    }
  }
}