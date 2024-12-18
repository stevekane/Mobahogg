using System;
using UnityEngine;

[CreateAssetMenu(fileName = "AirSpell Settings", menuName = "Spells/AirSpell Settings")]
public class AirSpellSettings : ScriptableObject {
  public AnimationCurve TornadoSuctionFalloff = AnimationCurve.Linear(0, 1, 1, 0);
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
