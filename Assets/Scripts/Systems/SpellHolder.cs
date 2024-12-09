using System.Collections.Generic;
using State;
using UnityEngine;

public class SpellHolder : AbstractState {
  public static int MAX_SPELL_QUEUE_SIZE = 3;

  public readonly EventSource<Spell> OnObtainSpell = new();

  Queue<Spell> NextSpellQueue = new(3);
  Queue<Spell> CurrentSpellQueue = new(3);
  int CurrentSpellQueueSize = 3;
  int NextSpellQueueSize = 3;
  public Queue<Spell> SpellQueue => CurrentSpellQueue;
  public int SpellQueueSize => CurrentSpellQueueSize;
  public bool TryAdd(Spell spell) {
    if (CurrentSpellQueue.Count < CurrentSpellQueueSize) {
      NextSpellQueue.Enqueue(spell);
      return true;
    } else {
      return false;
    }
  }
  public bool TrySetSize(int size) {
    if (size < MAX_SPELL_QUEUE_SIZE) {
      NextSpellQueueSize = size;
      return true;
    } else {
      return false;
    }
  }

  // N.B. I wonder if this "collection event" should really be processed
  // in the next frame's system pass?
  // Seems inconsistent to process it here
  void FixedUpdate() {
    // IMPORTANT: SINGLE LOOP SO THAT CURRENTSPELL.COUNT is right for listeners
    foreach (var spell in NextSpellQueue) {
      CurrentSpellQueue.Enqueue(spell);
      OnObtainSpell.Fire(spell);
    }
    NextSpellQueue.Clear();
    CurrentSpellQueueSize = NextSpellQueueSize;
  }

  void OnDrawGizmosSelected() {
    Debug.Log($"{CurrentSpellQueue.Count} spells in queue");
  }
}