public class EarthSpellPassiveEffect : Effect {
  void FixedUpdate() {
    SpellAffected.Immune.Set(true);
  }
}