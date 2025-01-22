using UnityEngine;

public abstract class Effect : MonoBehaviour {
  protected SpellAffected SpellAffected;
  protected LocalClock LocalClock;

  public void Initialize(SpellAffected spellAffected, LocalClock localClock) {
    SpellAffected = spellAffected;
    LocalClock = localClock;
  }
}