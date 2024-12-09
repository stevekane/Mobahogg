using UnityEngine;

public class SpellCollector : MonoBehaviour {
  [SerializeField] SpellHolder SpellHolder;

  public bool TryCollect(Spell spell) => SpellHolder.TryAdd(spell);
}