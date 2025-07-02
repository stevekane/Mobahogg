using System;
using UnityEngine;

namespace Melee {
  [Serializable]
  public struct MeleeAttackEvent {
    public MeleeAttackConfig Config;
    public Combatant Attacker;
    public Combatant Victim;
    public float KnockbackScale;
    public Vector3 ToVictim =>
      Victim.transform.position - Attacker.transform.position;
    public Vector3 Knockback =>
      KnockbackScale*Config.KnockBackStrength * (Victim.transform.position - Attacker.transform.position).XZ().normalized;
    public int KnockbackFrames =>
      Config.KnockbackDuration.Ticks;
    public int Damage;
    public Vector3 EstimatedContactPoint;
  }
}