using System;
using UnityEngine;

/*
Attributes are all designed to support concurrent writes and double-buffering.
When you read the value for an attribute, you'll always be reading the current value
of that attribute based on the last time aggregation was performed.

This means you control when aggregation occurs based on your game's needs. It is generally
recommended that you perform aggregation near the end of when all your systems run prior
to rendering though that is your choice.
*/

[Serializable]
public class Vector3Attribute {
  static float Longer(float x, float y) => Mathf.Abs(x) > Mathf.Abs(y) ? x : y;

  Vector3 DefaultValue;
  Vector3 Sum = Vector3.zero;
  float Scalar = 1;
  float? X;
  float? Y;
  float? Z;

  public Vector3Attribute(float x = 0, float y = 0, float z = 0) {
    DefaultValue = new Vector3(x, y, z);
  }

  public static Vector3Attribute WithDefault(Vector3 v) {
    return new Vector3Attribute(v.x, v.y, v.z);
  }

  public Vector3 Current { get; private set; }
  public void SetX(float f) => X = X.HasValue ? Longer(X.Value, f) : f;
  public void SetY(float f) => Y = Y.HasValue ? Longer(Y.Value, f) : f;
  public void SetZ(float f) => Z = Z.HasValue ? Longer(Z.Value, f) : f;
  public void Set(Vector3 v) {
    SetX(v.x);
    SetY(v.y);
    SetZ(v.z);
  }
  public void Add(Vector3 v) => Sum += v;
  public void Mul(float s) => Scalar += s;
  public void Sync(bool reset = true) {
    var current = Scalar * Sum;
    current.x = X.HasValue ? X.Value : current.x;
    current.y = Y.HasValue ? Y.Value : current.y;
    current.z = Z.HasValue ? Z.Value : current.z;
    Current = current;
    Scalar = 1;
    Sum = reset ? DefaultValue : Current;
    X = null;
    Y = null;
    Z = null;
  }
}

[Serializable]
public class QuaternionAttribute {
  // commutative aggregation. not necessarily sensible
  static Quaternion ByOrientation(Quaternion u, Quaternion v) => Quaternion.Dot(u, v) >= 0 ? u : v;

  Quaternion? Override;

  public Quaternion Current { get; private set; }
  public Vector3 Forward => Current * Vector3.forward;
  public void Set(Quaternion q) => Override = Override.HasValue ? ByOrientation(Override.Value, q) : q;
  public void Sync() {
    Current = Override.HasValue ? Override.Value : Current;
    Override = null;
  }
}

[Serializable]
public class BooleanAnyAttribute {
  bool Value;

  public bool Current { get; private set; }
  public void Set(bool v) => Value = Value || v;
  public void Sync() {
    Current = Value;
    Value = false;
  }
}

[Serializable]
public class FloatMinAttribute {
  float Default;
  float Value;

  public FloatMinAttribute(float defaultValue = 1) {
    Default = defaultValue;
    Value = defaultValue;
  }
  public float Current { get; private set; }
  public void Set(float v) => Value = Mathf.Min(Value, v);
  public void Sync() {
    Current = Value;
    Value = Default;
  }
}