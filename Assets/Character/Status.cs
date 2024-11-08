using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

public delegate void OnEffectComplete(Status status);

public abstract class StatusEffect : IDisposable {
  internal Status Status; // non-null while Added to this Status
  public OnEffectComplete OnComplete;
  public abstract bool Merge(StatusEffect e);
  public abstract void Apply(Status status);
  public virtual void OnRemoved(Status status) { }
  public void Dispose() => Status?.Remove(this);
}

public class InlineEffect : StatusEffect {
  Action<Status> ApplyFunc;
  string Name;
  public InlineEffect(Action<Status> apply, string name = "InlineEffect") => (ApplyFunc, Name) = (apply, name);
  public override bool Merge(StatusEffect e) => false;
  public override void Apply(Status status) => ApplyFunc(status);
  public override string ToString() => Name;
}

public class TimedEffect : StatusEffect {
  protected int Ticks = 0;
  protected int TotalTicks;
  public TimedEffect(int ticks) => TotalTicks = ticks;
  public override sealed void Apply(Status status) {
    if (Ticks++ < TotalTicks) {
      ApplyTimed(status);
    } else {
      status.Remove(this);
    }
  }
  public virtual void ApplyTimed(Status status) { }
  public override bool Merge(StatusEffect e) {
    var other = (TimedEffect)e;
    TotalTicks = Mathf.Max(TotalTicks - Ticks, other.TotalTicks);
    Ticks = 0;
    return true;
  }
}

public class HitStopEffect : TimedEffect {
  public Vector3 Axis;
  public HitStopEffect(Vector3 axis, int ticks) : base(ticks) => Axis = axis;
  public override void ApplyTimed(Status status) {
    status.CanAttack = false;
    var localTimeScale = Defaults.Instance.HitStopLocalTime.Evaluate((float)Ticks/TotalTicks);
    status.ModifyAttribute(AttributeTag.LocalTimeScale, new() { AddFactor = localTimeScale });
  }
}

public class HurtStunEffect : TimedEffect {
  public HurtStunEffect(int ticks) : base(ticks) { }
  public override void ApplyTimed(Status status) {
    status.CanMove = false;
    status.CanRotate = false;
    status.CanAttack = false;
    status.IsHurt = true;
  }
}

// The flying state that happens to a defender when they get hit by a strong attack.
public class KnockbackEffect : StatusEffect {
  const float DRAG = 5;
  const float AIRBORNE_SPEED = 100f;
  const float DONE_SPEED = 5f;

  // Returns the speed needed to cover the given distance with exponential decay due to drag.
  public static float GetSpeedToTravelDistance(float distance) => DRAG*distance;

  public float Drag;
  public Vector3 Velocity;
  public bool IsAirborne = false;
  public KnockbackEffect(Vector3 velocity, float drag = 5f) {
    Velocity = velocity;
    Drag = drag;
  }
  public override bool Merge(StatusEffect e) {
    Velocity = ((KnockbackEffect)e).Velocity;
    return true;
  }
  public override void Apply(Status status) {
    Velocity = Velocity * Mathf.Exp(-Time.fixedDeltaTime * Drag);
    status.Mover.Move(Velocity*Time.fixedDeltaTime);
    IsAirborne = Velocity.sqrMagnitude >= AIRBORNE_SPEED.Sqr();
    if (IsAirborne) {
      status.CanMove = false;
      status.CanRotate = false;
      status.CanAttack = false;
    }
    if (Velocity.sqrMagnitude < DONE_SPEED.Sqr())
      status.Remove(this);
  }
}

// The recoil effect that pushes you back when you hit something.
public class RecoilEffect : StatusEffect {
  static readonly float DRAG = 5f;
  static readonly float DONE_SPEED = 5f;

  public Vector3 Velocity;
  public RecoilEffect(Vector3 velocity) {
    Velocity = velocity;
  }
  // Ignore subsequent recoils.
  public override bool Merge(StatusEffect e) => true;
  public override void Apply(Status status) {
    Velocity = Velocity * Mathf.Exp(-Time.fixedDeltaTime * DRAG);
    status.Mover.Move(Velocity*Time.fixedDeltaTime);
    status.CanMove = false;
    status.CanRotate = false;
    if (Velocity.sqrMagnitude < DONE_SPEED.Sqr())
      status.Remove(this);
  }
}

// Some forward-translation juice for melee hits to convey momentum.
public class HitFollowthroughEffect : TimedEffect {
  Vector3 Velocity;
  List<Status> Defenders = new();
  public HitFollowthroughEffect(Vector3 velocity, Timeval duration, GameObject defender) : base(duration.Ticks) {
    Velocity = velocity;
    MaybeAddDefender(defender);
  }
  public override bool Merge(StatusEffect e) {
    ((HitFollowthroughEffect)e).Defenders.ForEach(d => MaybeAddDefender(d.gameObject));
    return true;
  }
  void MaybeAddDefender(GameObject defender) {
    if (defender && defender.TryGetComponent(out Status status) && status.IsHittable && status.IsInterruptible)
      Defenders.Add(status);
  }
  public override void ApplyTimed(Status status) {
    if (Defenders.Count == 0)
      return;
    var remaining = Defenders.Where(d => d != null);
    var delta = Velocity*Time.fixedDeltaTime;
    if (CanMove(status, delta) && remaining.Any(s => CanMove(s, delta))) {
      Move(status, delta);
    }
    remaining.ForEach(s => { if (CanMove(s, delta)) Move(s, delta); });
  }
  public bool CanMove(Status target, Vector3 delta) => !target.IsGrounded || target.Mover.IsOverGround(delta);
  public void Move(Status target, Vector3 delta) {
    target.Mover.Move(delta);
    target.CanMove = false;
    target.CanRotate = false;
  }
}

public class Status : MonoBehaviour {
  public List<StatusEffect> Active = new();
  internal CharacterController CharacterController;
  internal Mover Mover;
  Attributes Attributes;
  Dictionary<AttributeTag, AttributeValue> Modifiers = new();

  public bool IsGrounded { get; private set; }
  public bool JustGrounded { get; private set; }
  public bool JustTookOff { get; private set; }
  public bool IsHurt { get; set; }
  public bool CanMove { get => GetBoolean(AttributeTag.MoveSpeed); set => SetBoolean(AttributeTag.MoveSpeed, value); }
  public bool CanRotate { get => GetBoolean(AttributeTag.TurnSpeed); set => SetBoolean(AttributeTag.TurnSpeed, value); }
  public bool HasGravity { get => GetBoolean(AttributeTag.HasGravity); set => SetBoolean(AttributeTag.HasGravity, value); }
  public bool CanAttack { get => GetBoolean(AttributeTag.CanAttack); set => SetBoolean(AttributeTag.CanAttack, value); }
  public bool IsInterruptible { get => GetBoolean(AttributeTag.IsInterruptible); set => SetBoolean(AttributeTag.IsInterruptible, value); }
  public bool IsHittable { get => GetBoolean(AttributeTag.IsHittable); set => SetBoolean(AttributeTag.IsHittable, value); }
  public bool IsDamageable { get => GetBoolean(AttributeTag.IsDamageable); set => SetBoolean(AttributeTag.IsDamageable, value); }
  public AbilityTag Tags = 0;

  // All booleans default to true. Set to false after Modifiers.Clear() if you want otherwise.
  AttributeValue DefaultBoolean = new AttributeValue { Flat = 1 };
  bool GetBoolean(AttributeTag attrib) => DefaultBoolean.MergedWith(Attributes.GetAttribute(attrib)).Value > 0f;  // hacky
  void SetBoolean(AttributeTag attrib, bool value) {
    if (value) {
      ResetAttribute(attrib);
    } else {
      ModifyAttribute(attrib, AttributeValue.TimesZero);
    }
  }

  List<StatusEffect> Added = new();
  public StatusEffect Add(StatusEffect effect, OnEffectComplete onComplete = null) {
    Debug.Assert(!Active.Contains(effect), $"Effect {effect} is getting reused");
    Debug.Assert(!Added.Contains(effect), $"Effect {effect} is getting reused");
    effect.Status = this;
    var count = Active.Count;
    var existing = Active.FirstOrDefault((e) => e.GetType() == effect.GetType()) ?? Added.FirstOrDefault((e) => e.GetType() == effect.GetType());
    if (existing != null && existing.Merge(effect))
      return existing;
    effect.OnComplete = onComplete;
    Added.Add(effect);
    return effect;
    // TODO: merge onComplete with existing.OnComplete?
  }

  List<StatusEffect> Removed = new();
  public void Remove(StatusEffect effect) {
    if (effect != null)
      Removed.Add(effect);
  }

  public T Get<T>() where T : StatusEffect {
    return Active.FirstOrDefault(e => e is T) as T;
  }

  public AttributeValue GetAttribute(AttributeTag attrib) => Modifiers.GetValueOrDefault(attrib, null);
  public void ModifyAttribute(AttributeTag attrib, AttributeValue value) => Modifiers.MergeAttribute(attrib, value);
  public void ResetAttribute(AttributeTag attrib) => Modifiers.Remove(attrib);

  private void Awake() {
    Attributes = this.GetOrCreateComponent<Attributes>();
    Mover = GetComponent<Mover>();
  }

  private void FixedUpdate() {
    Tags = 0;
    Modifiers.Clear();

    var wasGrounded = IsGrounded;
    IsGrounded = Mover.IsOverGround(Vector3.zero);
    JustGrounded = !wasGrounded && IsGrounded;
    JustTookOff = wasGrounded && !IsGrounded;
    IsHurt = false;

    // TODO: differentiate between cancelled and completed?
    Removed.ForEach(e => {
      e.OnComplete?.Invoke(this);
      e.OnRemoved(this);
      e.Status = null;
      Active.Remove(e);
      Added.Remove(e);
    });
    Removed.Clear();
    Added.ForEach(e => Active.Add(e));
    Added.Clear();
    Active.ForEach(e => e.Apply(this));

#if UNITY_EDITOR
    DebugEffects.Clear();
    Active.ForEach(e => DebugEffects.Add($"{e}"));
#endif
  }

#if UNITY_EDITOR
  public List<string> DebugEffects = new();
  void OnDrawGizmos() {
    var controller = CharacterController != null ? CharacterController : GetComponent<CharacterController>();
    var cylinderHeight = Mathf.Max(0, controller.height - 2*controller.radius);
    var offsetDistance = cylinderHeight / 2;
    var offset = offsetDistance*Vector3.up;
    var position = transform.TransformPoint(controller.center-offset);
    Gizmos.DrawWireSphere(position, controller.radius);
  }
#endif
}