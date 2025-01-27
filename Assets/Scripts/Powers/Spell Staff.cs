using Abilities;
using UnityEngine;

public class SpellStaff : MonoBehaviour {
  [SerializeField] AbilityManager AbilityManager;
  [SerializeField] EffectManager EffectManager;
  [SerializeField] Player Player;
  [SerializeField] SpellHolder SpellHolder;
  [SerializeField] Animator Animator;
  [SerializeField] float Scale = 1;

  Transform SpellCharge;

  public Transform SpellChargeContainer;
  public Transform EmissionPoint;

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
      if (Player.PowerActiveAbility) {
        AbilityManager.Unregister(Player.PowerActiveAbility);
        Player.PowerActiveAbility = null;
      }
      if (Player.PowerUltimateAbility) {
        AbilityManager.Unregister(Player.PowerUltimateAbility);
        Player.PowerUltimateAbility = null;
      }
      if (Player.PowerPassiveEffect) {
        EffectManager.Unregister(Player.PowerPassiveEffect);
        Player.PowerPassiveEffect = null;
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
        Player.PowerActiveAbility = Instantiate(power.ActiveAbilityPrefab);
        AbilityManager.Register(Player.PowerActiveAbility);
      }
      if (power.UltimateAbilityPrefab) {
        Player.PowerUltimateAbility = Instantiate(power.UltimateAbilityPrefab);
        AbilityManager.Register(Player.PowerUltimateAbility);
      }
      if (power.SpellPassiveEffectPrefab) {
        Player.PowerPassiveEffect = Instantiate(power.SpellPassiveEffectPrefab);
        EffectManager.Register(Player.PowerPassiveEffect);
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