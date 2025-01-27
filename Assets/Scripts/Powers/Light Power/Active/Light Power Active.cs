using UnityEngine;
using Abilities;
using Cysharp.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using State;

public class LightPowerActive : UniTaskAbility {
  [SerializeField] LightPowerSettings Settings;

  SpellStaff SpellStaff;
  WeaponAim WeaponAim;
  Dictionary<Player, GameObject> PlayerToBeamMap = new();

  void Start() {
    SpellStaff = AbilityManager.LocateComponent<SpellStaff>();
    WeaponAim = AbilityManager.LocateComponent<WeaponAim>();
  }
  public override bool CanRun => true;
  public override bool CanCancel => true;
  protected override async UniTask Task(CancellationToken token) {
    GameObject emissionBeam = null;
    try {
      Animator.SetTrigger("Light Active");
      await Tasks.Delay(
        Settings.ActiveChargeDuration.Ticks,
        LocalClock,
        token);
      WeaponAim.AimDirection = Vector3.up;
      SpellStaff.Open();
      emissionBeam = Instantiate(
        Settings.ActiveChargeBeam,
        SpellStaff.SpellChargeContainer.position,
        Quaternion.identity);
      await Tasks.Delay(
        Settings.ActiveDownBeamsDelay.Ticks,
        LocalClock,
        token);
      await RunHealing(token);
    } finally {
      if (Animator)
        Animator.SetTrigger("Stop Hold");
      if (WeaponAim)
        WeaponAim.AimDirection = null;
      if (SpellStaff)
        SpellStaff.Close();
      emissionBeam.Destroy();
      foreach (var beamInstance in PlayerToBeamMap.Values) {
        beamInstance.Destroy();
      }
    }
  }

  async UniTask RunHealing(CancellationToken token) {
    try {
      PlayerToBeamMap.Clear();
      LivesManager.Active.Players.ForEach(AddBeamForTeamate);
      LivesManager.Active.OnAdd.Listen(AddBeamForTeamate);
      LivesManager.Active.OnRemove.Listen(RemoveBeamForTeamate);
      while (true) {
        foreach (var player in PlayerToBeamMap.Keys) {
          player.GetComponent<SpellAffected>().ChangeHealth(Settings.ActiveProcHealthChange);
        }
        await Tasks.Delay(Settings.ActiveProcCooldown.Ticks, LocalClock, token);
      }
    } finally {
      LivesManager.Active.OnAdd.Unlisten(AddBeamForTeamate);
      LivesManager.Active.OnRemove.Unlisten(RemoveBeamForTeamate);
      foreach (var beamInstance in PlayerToBeamMap.Values) {
        beamInstance.Destroy();
      }
    }
  }

  void AddBeamForTeamate(Player p) {
    if (Team.SameTeam(AbilityManager, p) && p.gameObject != AbilityManager.gameObject) {
      PlayerToBeamMap.Add(p, Instantiate(Settings.ActiveHealingLight, p.transform));
    }
  }

  void RemoveBeamForTeamate(Player p) {
    if (Team.SameTeam(AbilityManager, p) && PlayerToBeamMap.TryGetValue(p, out var beamInstance)) {
      PlayerToBeamMap.Remove(p);
      beamInstance.Destroy();
    }
  }
}