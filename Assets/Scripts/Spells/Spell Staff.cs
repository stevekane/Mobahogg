using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpellStaff : MonoBehaviour {
  [SerializeField] SpellHolder SpellHolder;
  [SerializeField] List<Transform> SpellChargeLocations;

  Queue<GameObject> SpellCharges = new();

  void AddSpell(Spell spell) {
    var index = SpellHolder.Count-1;
    var owner = SpellChargeLocations[index];
    var staffCharge = Instantiate(spell.SpellStaffChargePrefab, owner);
    SpellCharges.Enqueue(staffCharge);
  }

  void ShiftSpells(Spell spell) {
    var spellCharge = SpellCharges.Dequeue();
    for (var i = 0; i < SpellCharges.Count; i++) {
      SpellCharges.ElementAt(i).transform.SetParent(SpellChargeLocations[i], false);
    }
    Destroy(spellCharge.gameObject);
  }

  void Awake() {
    SpellHolder.OnAddSpell.Listen(AddSpell);
    SpellHolder.OnRemoveSpell.Listen(ShiftSpells);
  }

  void OnDestroy() {
    SpellHolder.OnAddSpell.Unlisten(AddSpell);
    SpellHolder.OnRemoveSpell.Unlisten(ShiftSpells);
  }
}