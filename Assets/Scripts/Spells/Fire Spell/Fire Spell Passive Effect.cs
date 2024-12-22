using UnityEngine;

public class FireSpellPassiveEffect : SpellPassiveEffect {
  [SerializeField] FireSpellSettings Settings;

  void FixedUpdate() {
    SpellAffected.AddDamage(Settings.PassiveExtraDamage);
  }
}