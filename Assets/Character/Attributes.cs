using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public enum AttributeTag {
  Damage,
  Health,
  Knockback,
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
  LocalTimeScale,
}

public static class AttributeTagValueExtensions {
  public static void MergeAttribute(this Dictionary<AttributeTag, AttributeValue> dict, AttributeTag attrib, AttributeValue value) {
    var v = dict.GetOrAdd(attrib, () => new());
    v.Merge(value);
  }
}

// Fuck you, Unity
[Serializable]
public class AttributeTagValuePair {
  public SerializableEnum<AttributeTag> Attribute;
  public AttributeValue Value;
}

// Holds a list of attributes for a character.
// Final attributes are calculated by merging (base attributes)+(per-frame attributes via Status).
public class Attributes : MonoBehaviour {
  public List<AttributeTagValuePair> BaseAttributes;
  Dictionary<AttributeTag, AttributeValue> BaseAttributesDict = new();
  Status Status;
  private void Awake() {
    Status = GetComponent<Status>();
    BaseAttributes.ForEach(kv => BaseAttributesDict.Add(kv.Attribute, kv.Value));
  }
  AttributeValue MaybeMerge(AttributeValue modifier, AttributeValue toMerge) => toMerge != null ? modifier.Merge(toMerge) : modifier;
  public AttributeValue GetAttribute(AttributeTag attrib) {
    AttributeValue modifier = new();
    MaybeMerge(modifier, BaseAttributesDict.GetValueOrDefault(attrib, null));
    MaybeMerge(modifier, Status.GetAttribute(attrib));
    return modifier;
  }
  public float GetValue(AttributeTag attrib) => GetAttribute(attrib).Value;
}