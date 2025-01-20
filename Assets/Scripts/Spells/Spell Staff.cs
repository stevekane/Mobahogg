using UnityEngine;

public class SpellStaff : MonoBehaviour {
  [SerializeField] SpellHolder SpellHolder;
  [SerializeField] Transform SpellChargeContainer;
  [SerializeField] Animator Animator;
  [SerializeField] float Scale = 1;

  Transform SpellCharge;

  public void Open() {
    Animator.SetInteger("Head State", 2);
    Animator.SetBool("Spinning", true);
  }

  public void Cast() {
    Animator.SetInteger("Head State", 1);
    Animator.SetBool("Spinning", false);
  }

  public void Close() {
    Animator.SetInteger("Head State", 0);
    Animator.SetBool("Spinning", false);
  }

  void UpdateSpell(Spell spell) {
    if (SpellCharge) {
      Destroy(SpellCharge.gameObject);
    }
    if (spell) {
      SpellCharge = Instantiate(spell.SpellStaffChargePrefab, SpellChargeContainer).transform;
      SpellCharge.localScale = Scale * Vector3.one;
    }
  }

  void Awake() {
    SpellHolder.OnChange.Listen(UpdateSpell);
  }

  void OnDestroy() {
    SpellHolder.OnChange.Unlisten(UpdateSpell);
  }
}