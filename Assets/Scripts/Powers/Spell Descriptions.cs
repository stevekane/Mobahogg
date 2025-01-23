using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SpellDescription {
  public string Name = "Spell";
  public SpellCharge SpellCharge;
  public Power Spell;
}

[CreateAssetMenu(fileName = "Spell Descriptions", menuName = "Spells/Spell Descriptions")]
public class SpellDescriptions : ScriptableObject {
  public List<SpellDescription> Value = new();
}