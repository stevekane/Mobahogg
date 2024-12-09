using System.Linq;
using UnityEngine;

public class SpellStaff : MonoBehaviour {
  [SerializeField] GameObject SpellStaffCharge;
  [SerializeField] SpellHolder SpellHolder;
  [SerializeField] Transform[] SpellChargeLocations;

  void AddSpell(Spell spell) {
    var index = SpellHolder.SpellQueue.Count-1;
    var owner = SpellChargeLocations[index];
    var staffCharge = Instantiate(SpellStaffCharge, owner);
  }

  void Start() {
    SpellHolder.OnObtainSpell.Listen(AddSpell);
  }

  void OnDestroy() {
    SpellHolder.OnObtainSpell.Unlisten(AddSpell);
  }
}