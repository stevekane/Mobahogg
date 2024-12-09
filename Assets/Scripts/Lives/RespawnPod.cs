using State;
using UnityEngine;

public enum RespawnPodState {
  Available,
  Respawning,
  Used
}

[DefaultExecutionOrder((int)ExecutionGroups.Managed)]
public class RespawnPod : MonoBehaviour {
  [SerializeField] LocalClock LocalClock;
  [SerializeField] Collider Collider;

  public int PortIndex { get; private set; }

  RespawnPodState State;
  int FramesRemaining;

  public bool Usable(TeamType teamType) =>
    State == RespawnPodState.Available && GetComponent<Team>().TeamType == teamType;

  public void StartRespawn(int frameDelay, int portIndex) {
    State = RespawnPodState.Respawning;
    FramesRemaining = frameDelay;
    PortIndex = portIndex;
  }

  void Start() {
    LivesManager.Active.RespawnPods.AddFirst(this);
  }

  void OnDestroy() {
    LivesManager.Active.RespawnPods.Remove(this);
  }

  void FixedUpdate() {
    if (LocalClock.Frozen() || State != RespawnPodState.Respawning)
      return;
    if (FramesRemaining <= 0) {
      // Destroy is delayed to end of frame. Instantiate is instant.
      // Therefore, must disable the collider on the pod to prevent unwanted interaction
      State = RespawnPodState.Used;
      Collider.enabled = false;
      Destroy(gameObject);
      LivesManager.Active.SpawnPlayerFromPod(this);
    }
    FramesRemaining -= LocalClock.DeltaFrames();
  }
}