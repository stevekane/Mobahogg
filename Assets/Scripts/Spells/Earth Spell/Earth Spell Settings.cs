using UnityEngine;

[CreateAssetMenu(fileName = "EarthSpell Settings", menuName = "Spells/EarthSpellSettings")]
public class EarthSpellSettings : ScriptableObject {
  public GameObject EarthBallPrefab;
  public GameObject RockPrefab;
  public GameObject SpikePrefab;
  public GameObject RockSprayPrefab;
  public float BallTravelDistance = 20;
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