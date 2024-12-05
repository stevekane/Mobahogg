namespace State {
  public abstract class AttributeBool : AbstractState {
    int Accumulator;
    bool Current;
    public abstract bool Default { get; set; }
    public bool Value { get => Current; set => Accumulator += (value ? 1 : -1); }
    void FixedUpdate() {
      Current = Accumulator > 0;
      Accumulator = Default ? 1 : 0;
    }
  }
}