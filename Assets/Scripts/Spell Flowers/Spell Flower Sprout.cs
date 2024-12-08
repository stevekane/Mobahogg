using UnityEngine;

[DefaultExecutionOrder((int)ExecutionGroups.Managed)]
public class SpellFlowerSprout : MonoBehaviour {
  [SerializeField] int GrowthFrames = 60 * 3;
  [SerializeField] LocalClock LocalClock;

  public bool DoneGrowing { get; private set; }
  public bool Blocked => Blockers > 0;

  int Blockers;
  int GrowthFrame;

  void OnTriggerEnter(Collider c) {
    Blockers++;
  }

  void OnTriggerExit(Collider c) {
    Blockers--;
  }

  void Start() {
    SpellFlowerManager.Active.Sprouts.Add(this);
  }

  void OnDestroy() {
    SpellFlowerManager.Active.Sprouts.Remove(this);
  }

  void FixedUpdate() {
    if (LocalClock.Frozen())
      return;

    if (!Blocked && GrowthFrame >= GrowthFrames) {
      SpellFlowerManager.Active.OnSproutReadyToGrow(this);
    }
    GrowthFrame = Mathf.Min(GrowthFrames, GrowthFrame+LocalClock.DeltaFrames());
  }
}