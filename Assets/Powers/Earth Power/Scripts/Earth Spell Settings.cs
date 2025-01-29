using UnityEngine;

[CreateAssetMenu(fileName = "EarthSpell Settings", menuName = "Spells/EarthSpellSettings")]
public class EarthSpellSettings : ScriptableObject {
  [Header("Passive")]
  public float PassiveKnockbackStrength = 3;

  [Header("Active")]
  public AnimationMontage ActiveAnimationMontage;
  public Timeval ActiveSlamHitStop = Timeval.FromTicks(12);
  public float ActiveKnockbackRadius = 5;
  public float ActiveKnockbackStrength = 25;
  public float ActiveRootMotionMultiplier = 2;

  [Header("Ultimate")]
  public GameObject EarthBallPrefab;
  public GameObject RockPrefab;
  public GameObject SpikePrefab;
  public GameObject RockSprayPrefab;
  public float BallTravelDistance = 20;
  public int HealthDelta = -4;
  public float KnockbackStrength = 50;
  public float MaxDamageDistance = 3;
  public int TravelFrames = 60;
  public int LingerFrames = 60;
  public float RockMinSize = 0.1f;
  public float RockMaxSize = 0.25f;
  public float RockMaxLateralOffset = 0.5f;
  public float RockJitterAmplitude = 0.25f;
  public float RockJitterFrequency = 20;
  public float SpikeMinThickness = 0.125f;
  public float SpikeMaxThickness = 0.25f;
  public float SpikeMinLength = 0.75f;
  public float SpikeMaxLength = 2f;
  public float SpikeMaxTiltAngle = 25;
  public float CameraShakeIntensity = 1.5f;
}