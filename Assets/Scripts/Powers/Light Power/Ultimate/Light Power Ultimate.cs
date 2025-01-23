using Abilities;
using UnityEngine;

public class LightPowerUltimate : Ability {
  public override bool CanRun => true;
  public override void Run() {
    AbilityManager.GetComponent<SpellHolder>().Remove();
  }

  public override bool IsRunning => false;
  public override bool CanCancel => false;
  public override void Cancel() {
    Debug.Log("Cancel called");
  }
}