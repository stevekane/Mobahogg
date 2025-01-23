using UnityEngine;

[CreateAssetMenu(fileName = "LightPowerSettings", menuName = "Powers/Light/LightPowerSettings")]
public class LightPowerSettings : ScriptableObject {
  [Header("Passive Effect")]
  public Timeval ChimeFrameCooldown = Timeval.FromSeconds(3);
  public GameObject ChimePrefab;
  public float ChimeMinSpawnRadius = 5;
  public float ChimeMaxSpawnRadius = 10;
  public float ChimeSpeedSurgeAmount = 3;
  public Timeval ChimeSpeedSurgeDuration = Timeval.FromSeconds(2);
  public AnimationCurve ChimeSpeedCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

  [Header("Ultimate")]
  public Timeval UltimateChargeDuration = Timeval.FromMillis(600);
}