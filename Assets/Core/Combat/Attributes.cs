using System;

[Serializable]
public enum AttributeTag {
  Damage,
  Health,
  Knockback,
  Weight,
  MoveSpeed,
  TurnSpeed,
  AttackSpeed,
  MaxFallSpeed,
  Gravity,
  HasGravity,
  CanAttack,
  IsHittable,
  IsDamageable,
  IsGrounded,
  IsHurt,
  IsInterruptible,
  GoldGain,
}

// AttributeValues serve to accumulate a single value from multiple sources.
// Example: Damage =
//   Flat:(character base damage + weapon damage) *
//   AddFactor:(character damage upgrade + frenzy damage factor) *
//   MultFactor:(hitbox centered factor * weakspot factor)
[Serializable]
public class AttributeValue {
  public static AttributeValue TimesZero = new() { MultFactor = 0 };
  public static AttributeValue TimesOne = new() { MultFactor = 1 };
  public static AttributeValue Plus(float n) => new() { Flat = n };
  public static AttributeValue Times(float n) => new() { MultFactor = n };

  public float Flat = 0;
  public float AddFactor = 1;
  public float MultFactor = 1;
  public float Value => Flat * MultFactor * AddFactor;
  public AttributeValue Merge(AttributeValue other) {
    Flat += other.Flat;
    AddFactor += other.AddFactor;
    MultFactor *= other.MultFactor;
    return this;
  }
}
