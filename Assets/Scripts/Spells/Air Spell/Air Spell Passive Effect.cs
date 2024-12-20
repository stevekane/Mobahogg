using UnityEngine;

public class AirSpellPassiveEffect : SpellPassiveEffect {
  [SerializeField] AirSpellSettings Settings;

  void FixedUpdate() {
    SpellAffected.MultiplySpeed(Settings.PassiveMoveSpeedMultiplier);
  }
}