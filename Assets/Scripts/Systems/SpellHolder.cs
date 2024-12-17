using System.Collections.Generic;
using UnityEngine;
using State;

/*
I have opted to, for now, make this class non-deterministic and dumb.
It simply enqueues the spell if it can otherwise it does not.
This means that holder/charge interactions will be processed in a first
-writer wins sort of way which is realistically probably fine for this game
given that determinism is not a primary design goal.
*/
public class SpellHolder : AbstractState {
  public readonly EventSource<Spell> OnAddSpell = new();
  public readonly EventSource<Spell> OnRemoveSpell = new();

  [SerializeField] int MaxCount = 3;

  Queue<Spell> SpellQueue = new();

  public int Count => SpellQueue.Count;

  public bool TryEnqueue(Spell spell) {
    if (SpellQueue.Count < MaxCount) {
      SpellQueue.Enqueue(spell);
      OnAddSpell.Fire(spell);
      return true;
    } else {
      return false;
    }
  }

  public void SetMax(int size) {
    MaxCount = size;
  }

  public Spell Dequeue() {
    var spell = SpellQueue.Dequeue();
    OnRemoveSpell.Fire(spell);
    return spell;
  }
}