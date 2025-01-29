using State;

// This is used to impart knockback on other entities!
public class KnockbackScale : AbstractState {
  float Sum;
  float Product;
  float Current;

  public void Add(float f) => Sum+=f;
  public void Mul(float f) => Product+=f;
  public float Value => Current;

  void FixedUpdate() {
    Current = Product*Sum;
    Product = 1;
    Sum = 1;
  }
}