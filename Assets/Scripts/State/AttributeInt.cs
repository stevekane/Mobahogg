using UnityEngine;

namespace State {
  public class ModifierInt {
    public int Add = 0;
    public int Mul = 1;
    public int? Set = null;
  }

  public abstract class AttributeInt : MonoBehaviour {
    ModifierInt Accumulator = new();
    ModifierInt Current = new();
    public abstract int Base { get; set; }
    public int Value => Evaluate(0);
    public int Evaluate(int i) => Current.Set.HasValue ? Current.Set.Value : (Base + i + Current.Add) * Current.Mul;
    public void Add(int v) => Accumulator.Add += v;
    public void Mul(int v) => Accumulator.Mul *= v;
    public void Set(int v) => Accumulator.Set = Mathf.Min(Accumulator.Set.Value, v);
    void FixedUpdate() {
      Current = Accumulator;
      Accumulator = new();
    }
  }
}