using System;
using UnityEngine;

namespace Melee {
  [Serializable]
  public struct MeleeAttackEvent {
    public MeleeAttackConfig Config;
    public Combatant Attacker;
    public Combatant Victim;
    public Vector3 ToVictim =>
      Attacker.transform.position - Victim.transform.position;
    public Vector3 Knockback =>
      Config.KnockBackStrength * (Victim.transform.position - Attacker.transform.position).XZ().normalized;
    public int KnockbackFrames =>
      Config.KnockbackDuration.Ticks;
    public int Damage;
  }
}