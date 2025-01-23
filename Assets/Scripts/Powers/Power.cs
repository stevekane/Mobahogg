using Abilities;
using UnityEngine;

[CreateAssetMenu(fileName = "Power", menuName = "Power")]
public class Power : ScriptableObject {
  public Material SurfaceMaterial;
  public Effect SpellPassiveEffectPrefab;
  public Ability ActiveAbilityPrefab;
  public Ability UltimateAbilityPrefab;
}