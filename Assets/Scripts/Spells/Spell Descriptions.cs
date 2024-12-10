using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SpellDescription {
  public string Name = "Spell";
  public SpellCharge SpellCharge;
  public Spell Spell;
}

[CreateAssetMenu(fileName = "SpellDescriptions", menuName = "Scriptable Objects/SpellDescriptions")]
public class SpellDescriptions : ScriptableObject {
  public List<SpellDescription> Value = new();
}