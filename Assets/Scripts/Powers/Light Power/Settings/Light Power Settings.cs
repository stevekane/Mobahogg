using UnityEngine;

[CreateAssetMenu(fileName = "LightPowerSettings", menuName = "Powers/Light/LightPowerSettings")]
public class LightPowerSettings : ScriptableObject {
  [Header("Passive Effect")]
  public Timeval ChimeFrameCooldown = Timeval.FromSeconds(3);
  public GameObject ChimePrefab;
  public float ChimeMinSpawnRadius = 5;
  public float ChimeMaxSpawnRadius = 10;
  public float ChimeSpeedSurgeAmount = 3;
  public int ChimeSpeedSurgeDuration = 1;
}