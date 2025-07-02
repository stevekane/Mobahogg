using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class LiveWireShockEffect : Effect {
  [SerializeField] LiveWireShockEffectSettings Settings;

  public Vector3 ContactPoint;
  public bool SpawnPlasmaArc;

  PlasmaArc PlasmaArc;

  void Start()
  {
    Run(this.destroyCancellationToken).Forget();
  }

  async UniTask Run(CancellationToken token)
  {
    try
    {
      const float HURT_DIRECTION_FRONT = 1f / 3;
      var localClock = EffectManager.GetComponent<LocalClock>();
      var provider = EffectManager.GetComponent<FrameBehaviorProvider>();
      var animator = provider.Get(typeof(Animator), default) as Animator;
      var weapon = provider.Get(typeof(SpellStaff), default) as SpellStaff;
      var weaponTip = provider.Get(typeof(Transform), Settings.WeaponTipTag) as Transform;
      var spellAffected = EffectManager.GetComponent<SpellAffected>();
      var duration = Settings.ShockDuration.Ticks;
      var knockbackDirection = (transform.position - ContactPoint).normalized;
      spellAffected.Knockback(Settings.KnockBackStrength * knockbackDirection);
      animator.SetFloat("Hurt Direction", HURT_DIRECTION_FRONT);
      animator.SetTrigger("Hurt Flinch");
      weapon.GetComponent<Flash>().Set(Settings.ShockDuration.Ticks);
      if (SpawnPlasmaArc)
      {
        PlasmaArc = Instantiate(Settings.PlasmaArcPrefab, transform);
      }
      for (var i = 0; i < duration; i++)
      {
        if (PlasmaArc)
        {
          var p0 = weaponTip.position;
          var p3 = ContactPoint;
          var p1 = p0 + 0.2f * (p3 - p0) + Vector3.up;
          var p2 = p0 + 0.8f * (p3 - p0) + Vector3.up;
          PlasmaArc.SetPoints(p0, p1, p2, p3);
        }
        await Tasks.Delay(1, localClock, token);
      }
    }
    finally
    {
      if (PlasmaArc)
      {
        Destroy(PlasmaArc.gameObject);
        PlasmaArc = null;
      }
      EffectManager.Unregister(this);
    }
  }
}