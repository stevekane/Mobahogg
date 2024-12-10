using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpellStaff : MonoBehaviour {
  [SerializeField] GameObject SpellStaffCharge;
  [SerializeField] SpellHolder SpellHolder;
  [SerializeField] List<Transform> SpellChargeLocations;

  Queue<GameObject> SpellCharges = new();

  void AddSpell(Spell spell) {
    var index = SpellHolder.SpellQueue.Count-1;
    var owner = SpellChargeLocations[index];
    var staffCharge = Instantiate(SpellStaffCharge, owner);
    SpellCharges.Enqueue(staffCharge);
  }

  void ShiftSpells(Spell spell) {
    var spellCharge = SpellCharges.Dequeue();
    for (var i = 0; i < SpellCharges.Count; i++) {
      SpellCharges.ElementAt(i).transform.SetParent(SpellChargeLocations[i], false);
    }
    Destroy(spellCharge.gameObject);
  }

  void Start() {
    SpellHolder.OnAddSpell.Listen(AddSpell);
    SpellHolder.OnRemoveSpell.Listen(ShiftSpells);
  }

  void OnDestroy() {
    SpellHolder.OnAddSpell.Unlisten(AddSpell);
    SpellHolder.OnRemoveSpell.Unlisten(ShiftSpells);
  }
}