using System;
using UnityEngine;

[CreateAssetMenu(fileName = "AirSpell Settings", menuName = "Spells/AirSpell Settings")]
public class AirSpellSettings : ScriptableObject {
  public AnimationCurve TornadoSuctionFalloff = AnimationCurve.Linear(0, 1, 1, 0);
  public float MaxTornadoSpeed = 5f;          // Maximum speed of the tornado
  public float MaxTornadoTurningSpeed = 45f;     // Maximum turning speed in degrees per second
  public float TornadoDriftNoiseScale = 0.1f; // Scale for Perlin noise (affects smoothness of drift)
  public float TornadoSpeedNoiseScale = 0.1f; // Scale for Perlin noise (affects smoothness of speed)
  public float TornadoInnerRadius = 3;
  public float TornadoOuterRadius = 15;
  public float TornadoMaxSuction = 100;
  public float TornadoSuction(float distance) {
    if (distance > TornadoOuterRadius || distance < TornadoInnerRadius)
      return 0;
    var interpolant = Mathf.InverseLerp(TornadoInnerRadius, TornadoOuterRadius, distance);
    return TornadoMaxSuction * TornadoSuctionFalloff.Evaluate(interpolant);
  }
}