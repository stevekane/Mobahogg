using UnityEngine;

[CreateAssetMenu(fileName = "WaterSpell Settings", menuName = "Spells/WaterSpell Settings")]
public class WaterSpellSettings : ScriptableObject {
  public float SlowFraction = 0.5f;
  public Timeval PassiveHealCooldown = Timeval.FromSeconds(3);
  public int PassiveHealAmount = 1;
}