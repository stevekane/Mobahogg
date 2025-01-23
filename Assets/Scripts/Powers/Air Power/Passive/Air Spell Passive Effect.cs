using UnityEngine;

public class AirSpellPassiveEffect : PowerEffect {
  [SerializeField] AirSpellSettings Settings;

  SpellAffected SpellAffected;

  void Start() {
    SpellAffected = EffectManager.GetComponent<SpellAffected>();
  }

  void FixedUpdate() {
    SpellAffected.MultiplySpeed(Settings.PassiveMoveSpeedMultiplier);
  }
}