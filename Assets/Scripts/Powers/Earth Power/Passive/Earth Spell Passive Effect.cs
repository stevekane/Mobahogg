using UnityEngine;

public class EarthSpellPassiveEffect : PowerEffect {
  SpellAffected SpellAffected;

  void Start() {
    SpellAffected = EffectManager.GetComponent<SpellAffected>();
  }

  void FixedUpdate() {
    SpellAffected.ScaleKnockbackStrength(3);
  }
}