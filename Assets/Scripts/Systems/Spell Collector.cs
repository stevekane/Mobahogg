using UnityEngine;

public class SpellCollector : MonoBehaviour {
  [SerializeField] SpellHolder SpellHolder;

  void Start() {
    SpellHolder.OnObtainSpell.Listen(Debug.Log);
  }

  void OnDestroy() {
    SpellHolder.OnObtainSpell.Unlisten(Debug.Log);
  }

  public bool TryCollect(Spell spell) => SpellHolder.TryAdd(spell);
}