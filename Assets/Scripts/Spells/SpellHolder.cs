using State;

public class SpellHolder : AbstractState {
  public readonly EventSource<Spell> OnChange = new();

  Spell Current;

  public Spell Spell => Current;

  public bool TryAdd(Spell spell) {
    if (Current == null) {
      Current = spell;
      OnChange.Fire(spell);
      return true;
    } else {
      return false;
    }
  }

  public Spell Remove() {
    var spell = Current;
    Current = null;
    OnChange.Fire(null);
    return spell;
  }
}