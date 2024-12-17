using UnityEngine;

namespace Melee {
  [CreateAssetMenu(menuName = "Melee/Attack Config")]
  public class MeleeAttackConfig : ScriptableObject {
    [Header("Health")]
    public int Damage = 1;
    [Header("HitStop")]
    public Timeval HitStopDuration = Timeval.FromMillis(100);
    [Header("Knockback")]
    public Timeval KnockbackDuration = Timeval.FromMillis(200);
    [Header("Knockback")]
    public float KnockBackStrength = 10;
    [Header("Vibration")]
    public float VibrationAmplitude = 0.125f;
    public float VibrationFrequency = 30;
    [Header("Camera Shake")]
    public float CameraShakeIntensity = 1;
  }
}