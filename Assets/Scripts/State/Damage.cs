using UnityEngine;
using State;

public class Damage : AbstractState {
  [SerializeField] int Base = 1;

  int Accumulator;
  int Current;
  public void Add(int v) => Accumulator += v;
  public int Value => Current;

  void FixedUpdate() {
    Current = Base + Accumulator;
    Accumulator = 0;
  }
}