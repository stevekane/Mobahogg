using UnityEngine;

namespace State {
  public class Health : AbstractState {
    [SerializeField] int Current = 1;
    int Accumulator;

    public void Change(int delta) => Accumulator += delta;
    public int Value { get => Current; }

    void FixedUpdate() {
      Current += Accumulator;
      Current = Mathf.Max(0, Current);
      Accumulator = 0;
    }
  }
}