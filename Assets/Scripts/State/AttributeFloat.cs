using UnityEngine;

namespace State {
  public struct ModifierFloat {
    public float Add;
    public float Mul;
    public float Set;
    public ModifierFloat(float add, float mul, float set) {
      Add = add;
      Mul = mul;
      Set = set;
    }
    public override string ToString() {
      return $"ADD:{Add} MUL:{Mul} SET:{Set}";
    }
  }

  public abstract class AttributeFloat : AbstractState {
    ModifierFloat Accumulator = new(0, 1, float.NaN);
    ModifierFloat Current = new(0, 1, float.NaN);
    public abstract float Base { get; set; }
    public float Value => Evaluate(0);
    public float Evaluate(float f) => !float.IsNaN(Current.Set) ? Current.Set : (Base + f + Current.Add) * Current.Mul;
    public void Add(float v) => Accumulator.Add += v;
    public void Mul(float v) => Accumulator.Mul *= v;
    public void Set(float v) => Accumulator.Set = float.IsNaN(Accumulator.Set) ? v : Mathf.Min(v, Accumulator.Set);
    public void FixedUpdate() {
      Current = Accumulator;
      Accumulator = new(0, 1, float.NaN);
    }
  }
}