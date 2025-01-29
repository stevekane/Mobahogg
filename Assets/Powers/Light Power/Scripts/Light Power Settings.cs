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

  [Header("Active")]
  public Timeval ActiveChargeDuration = Timeval.FromSeconds(1);
  public Timeval ActiveProcCooldown = Timeval.FromMillis(500);
  public int ActiveProcHealthChange = 1;
  public GameObject ActiveChargeBeam;
  public GameObject ActiveHealingLight;

  [Header("Ultimate")]
  public Timeval UltimateChargeDuration = Timeval.FromSeconds(1);
  public Timeval UltimateChannelDuration = Timeval.FromSeconds(3);
  public Timeval UltimateProcCooldown = Timeval.FromMillis(500);
  public int UltimateProcHealthChange = 2;
  public float UltimateTurnSpeed = 30;
  public Material ChargeSphereMaterial;
  public GameObject UltimateChargeBeamPrefab;
  public LayerMask UltimateLayerMask = new();
}