using UnityEngine;
using Abilities;
using Cysharp.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using State;
using System;

public class LightPowerActive : UniTaskAbility, IHeld {
  [SerializeField] LightPowerSettings Settings;

  SpellStaff SpellStaff;
  WeaponAim WeaponAim;
  Dictionary<Player, GameObject> PlayerToBeamMap = new();
  GameObject EmissionBeam;

  void Start() {
    SpellStaff = AbilityManager.LocateComponent<SpellStaff>();
    WeaponAim = AbilityManager.LocateComponent<WeaponAim>();
  }

  public bool CanRelease => IsRunning;
  public void Release() => Cancel();

  public override bool CanRun => true;
  public override bool CanCancel => true;
  protected override async UniTask Task(CancellationToken token) {
    try {
      await Chargeup(token);
      await RunHealing(token);
    } finally {
      Cleanup();
    }
  }

  async UniTask Chargeup(CancellationToken token) {
    try {
      Animator.SetTrigger("Light Active");
      await Tasks.Delay(
        Settings.ActiveChargeDuration.Ticks,
        LocalClock,
        token);
      WeaponAim.AimDirection = Vector3.up;
      SpellStaff.Open();
      EmissionBeam = Instantiate(
        Settings.ActiveChargeBeam,
        SpellStaff.EmissionPoint.position,
        Quaternion.LookRotation(SpellStaff.EmissionPoint.forward, SpellStaff.EmissionPoint.up),
        SpellStaff.EmissionPoint);
    } catch (Exception e) {
      Animator.SetTrigger("Stop Hold");
    }
  }

  async UniTask RunHealing(CancellationToken token) {
    try {
      PlayerToBeamMap.Clear();
      SpawnManager.Active.Players.ForEach(AddBeamForTeamate);
      SpawnManager.Active.OnAddPlayer.Listen(AddBeamForTeamate);
      SpawnManager.Active.OnRemovePlayer.Listen(RemoveBeamForTeamate);
      while (true) {
        foreach (var player in PlayerToBeamMap.Keys) {
          player.GetComponent<SpellAffected>().ChangeHealth(Settings.ActiveProcHealthChange);
        }
        await Tasks.Delay(Settings.ActiveProcCooldown.Ticks, LocalClock, token);
      }
    } finally {
      SpawnManager.Active.OnAddPlayer.Unlisten(AddBeamForTeamate);
      SpawnManager.Active.OnRemovePlayer.Unlisten(RemoveBeamForTeamate);
      foreach (var beamInstance in PlayerToBeamMap.Values) {
        beamInstance.Destroy();
      }
    }
  }

  void Cleanup() {
    if (Animator)
      Animator.SetTrigger("Stop Hold");
    if (WeaponAim)
      WeaponAim.AimDirection = null;
    if (SpellStaff)
      SpellStaff.Close();
    EmissionBeam.Destroy();
    foreach (var beamInstance in PlayerToBeamMap.Values) {
      beamInstance.Destroy();
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