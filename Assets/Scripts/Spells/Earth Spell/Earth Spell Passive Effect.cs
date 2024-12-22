public class EarthSpellPassiveEffect : SpellPassiveEffect {
  void Start() {
    SpellAffected.Immune = true;
  }

  void OnDestroy() {
    SpellAffected.Immune = false;
  }
}