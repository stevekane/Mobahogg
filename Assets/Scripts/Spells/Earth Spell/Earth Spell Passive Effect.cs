public class EarthSpellPassiveEffect : SpellPassiveEffect {
  void FixedUpdate() {
    SpellAffected.Immune.Set(true);
  }
}