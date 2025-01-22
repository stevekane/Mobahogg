using UnityEngine;

public class AirSpellPassiveEffect : Effect {
  [SerializeField] AirSpellSettings Settings;

  void FixedUpdate() {
    SpellAffected.MultiplySpeed(Settings.PassiveMoveSpeedMultiplier);
  }
}