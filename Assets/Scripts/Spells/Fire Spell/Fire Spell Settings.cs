using UnityEngine;

[CreateAssetMenu(fileName = "FireSpell Settings", menuName = "Spells/FireSpell Settings")]
public class FireSpellSettings : ScriptableObject {
  [Header("Egg")]
  public GameObject EggPrefab;
  public Vector3 EggLocalTravelDelta = new(0, 0, 10);
  public Timeval EggTravelDuration = Timeval.FromSeconds(1);
  public EasingFunctions.EasingFunctionName EggTravelEasingFunctionName;
  public EasingFunctions.EasingFunctionName EggSpinupEasingFunctionName;
  public EasingFunctions.EasingFunctionName EggVibrationEasingFunctionName;

  [Header("Explosion")]
  public GameObject ExplosionPrefab;
  public float ExplosionCameraShakeIntensity = 0.25f;
  public float ExplosionKnockback = 50;
  public float ExplosionRadius = 5;

  [Header("Dragons")]
  public GameObject DragonPrefab;
  public float DragonTravelSpeed = 30;
  public float DragonSpreadAngle = 135;
  public int DragonCount = 5;
}