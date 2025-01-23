using UnityEngine;

public class FireSpellPassiveEffect : PowerEffect {
  [SerializeField] FireSpellSettings Settings;

  SpellAffected SpellAffected;

  void Start() {
    SpellAffected = EffectManager.GetComponent<SpellAffected>();
  }

  void FixedUpdate() {
    SpellAffected.ChangeDamage(Settings.PassiveExtraDamage);
  }
}