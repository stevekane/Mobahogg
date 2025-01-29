using UnityEngine;

[CreateAssetMenu(fileName = "Knockback Settings", menuName = "Knockback/KnockbackSettings")]
public class KnockbackSettings : ScriptableObject {
  public float KnockbackDecayRate = 3;
}