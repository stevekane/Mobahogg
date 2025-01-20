using UnityEngine;

public class SpellPassiveEffectManager : MonoBehaviour {
  [SerializeField] SpellHolder SpellHolder;
  [SerializeField] SpellAffected SpellAffected;
  [SerializeField] LocalClock LocalClock;

  SpellPassiveEffect CurrentEffect;

  void Awake() {
    SpellHolder.OnChange.Listen(UpdatePassiveEffect);
  }

  void OnDestroy() {
    SpellHolder.OnChange.Unlisten(UpdatePassiveEffect);
  }

  void UpdatePassiveEffect(Spell spell) {
    if (CurrentEffect)
      Destroy(CurrentEffect);
    if (spell) {
      CurrentEffect = Instantiate(spell.SpellPassiveEffectPrefab, transform);
      CurrentEffect.Initialize(SpellAffected, LocalClock);
    }
  }
}