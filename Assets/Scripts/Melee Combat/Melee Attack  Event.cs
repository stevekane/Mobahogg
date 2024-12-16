using System;
using UnityEngine;

namespace Melee {
  [Serializable]
  public struct MeleeAttackEvent {
    public MeleeAttackConfig Config;
    public Combatant Attacker;
    public Combatant Victim;
    public Vector3 KnockbackDirection =>
      Config.KnockBackStrength * (Victim.transform.position - Attacker.transform.position).normalized;
    public int KnockbackFrames =>
      Config.KnockbackDuration.Ticks;
    public int Damage =>
      Config.Damage;
  }
}