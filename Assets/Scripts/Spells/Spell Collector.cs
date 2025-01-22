using UnityEngine;

public class SpellCollector : MonoBehaviour {
  [SerializeField] SpellHolder SpellHolder;

  public bool TryCollect(Power power) => SpellHolder.TryAdd(power);
}