using UnityEngine;

public class EarthSpellPassiveEffect : PowerEffect {
  [SerializeField] EarthSpellSettings Settings;

  SpellAffected SpellAffected;

  void Start() {
    SpellAffected = EffectManager.GetComponent<SpellAffected>();
  }

  void FixedUpdate() {
    SpellAffected.ScaleKnockbackStrength(Settings.PassiveKnockbackStrength);
  }
}