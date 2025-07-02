using UnityEngine;

[CreateAssetMenu(fileName = "LiveWireShockEffectSettings", menuName = "Effects/LiveWireShockEffectSettings")]
public class LiveWireShockEffectSettings : ScriptableObject
{
  public BehaviorTag WeaponTipTag;
  public float KnockBackStrength = 10;
  public Timeval ShockDuration = Timeval.FromMillis(500);
  public PlasmaArc PlasmaArcPrefab;
}