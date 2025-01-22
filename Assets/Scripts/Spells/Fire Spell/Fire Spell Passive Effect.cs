using UnityEngine;

public class FireSpellPassiveEffect : Effect {
  [SerializeField] FireSpellSettings Settings;

  void FixedUpdate() {
    SpellAffected.ChangeDamage(Settings.PassiveExtraDamage);
  }
}