using State;

public class SpellHolder : AbstractState {
  public readonly EventSource<Power> OnChange = new();

  Power Current;

  public Power Power => Current;

  public bool TryAdd(Power power) {
    if (Current == null) {
      Current = power;
      OnChange.Fire(power);
      return true;
    } else {
      return false;
    }
  }

  public Power Remove() {
    var power = Current;
    Current = null;
    OnChange.Fire(null);
    return power;
  }
}