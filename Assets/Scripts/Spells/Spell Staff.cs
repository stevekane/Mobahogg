using Abilities;
using UnityEngine;

public class SpellStaff : MonoBehaviour {
  [SerializeField] AbilityManager AbilityManager;
  [SerializeField] Player Player;
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

  void UpdatePower(Power power) {
    if (SpellCharge) {
      Destroy(SpellCharge.gameObject);
      // N.B. If this player class ever stops storing explicit references to the
      // Active/Ultimate Ability INSTANCES then we'll need to be sure we store
      // them elsewhere or at least have some way to unregister the current ones
      // from AbilityManager directly
      if (Player.ActiveAbility) {
        AbilityManager.Unregister(Player.ActiveAbility);
        Player.ActiveAbility = null;
      }
      if (Player.UltimateAbility) {
        AbilityManager.Unregister(Player.UltimateAbility);
        Player.UltimateAbility = null;
      }
    }
    if (power) {
      SpellCharge = GameObject.CreatePrimitive(PrimitiveType.Sphere).transform;
      SpellCharge.SetParent(SpellChargeContainer, false);
      SpellCharge.localScale = Scale * Vector3.one;
      if (SpellCharge.TryGetComponent(out Collider collider)) {
        Destroy(collider);
      }
      if (SpellCharge.TryGetComponent(out MeshRenderer meshRenderer)) {
        meshRenderer.sharedMaterial = SpellHolder.Power.SurfaceMaterial;
      }
      if (power.ActiveAbilityPrefab) {
        Player.ActiveAbility = Instantiate(power.ActiveAbilityPrefab);
        AbilityManager.Register(Player.ActiveAbility);
      }
      if (power.UltimateAbilityPrefab) {
        Player.UltimateAbility = Instantiate(power.UltimateAbilityPrefab);
        AbilityManager.Register(Player.UltimateAbility);
      }
    }
  }

  void Awake() {
    SpellHolder.OnChange.Listen(UpdatePower);
  }

  void OnDestroy() {
    SpellHolder.OnChange.Unlisten(UpdatePower);
  }
}