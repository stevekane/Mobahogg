using State;
using UnityEngine;

public enum RespawnPodState {
  Available,
  Respawning,
  Used
}

public class RespawnPod : MonoBehaviour {
  [SerializeField] Team Team;
  [SerializeField] LocalClock LocalClock;

  public RespawnPodState State;

  int FramesRemaining;

  public bool Usable(TeamType teamType) =>
    State == RespawnPodState.Available && Team.TeamType == teamType;

  public void Respawn(int frameDelay) {
    State = RespawnPodState.Respawning;
    FramesRemaining = frameDelay;
  }

  void Start() {
    LivesManager.Active.RespawnPods.AddFirst(this);
  }

  void OnDestroy() {
    LivesManager.Active.RespawnPods.Remove(this);
  }

  void FixedUpdate() {
    FramesRemaining -= LocalClock.DeltaFrames();
    if (FramesRemaining <= 0) {

    }
  }
}