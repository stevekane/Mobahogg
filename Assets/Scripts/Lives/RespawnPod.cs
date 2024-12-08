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

  public int PortIndex { get; private set; }

  RespawnPodState State;
  int FramesRemaining;

  public bool Usable(TeamType teamType) =>
    State == RespawnPodState.Available && GetComponent<Team>().TeamType == teamType;

  public void StartRespawn(int frameDelay, int portIndex) {
    State = RespawnPodState.Respawning;
    FramesRemaining = frameDelay;
    PortIndex = portIndex;
    Debug.Log($"Pod Respawn Started {PortIndex}");
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
      State = RespawnPodState.Used;
      LivesManager.Active.SpawnPlayerFromPod(this);
      Destroy(gameObject);
      Debug.Log($"Pod Spawn initiated {PortIndex}");
    }
    FramesRemaining -= LocalClock.DeltaFrames();
  }
}