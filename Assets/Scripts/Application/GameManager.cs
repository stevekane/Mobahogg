using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : SingletonBehavior<GameManager> {
  [Header("Scenes")]
  [SerializeField] SceneAsset FirstScene;
  [SerializeField] bool LoadFakeMatch;

  void Start() {
    if (LoadFakeMatch) {
      var players = new PotentialPlayer[2] {
        new PotentialPlayer { Name = "Alice", Team = true, State = PotentialPlayerState.Ready },
        new PotentialPlayer { Name = "Bob", Team = false, State = PotentialPlayerState.Ready }
      };
      MatchManager.Instance.StartMatch(players);
    } else {
      SceneManager.LoadScene(FirstScene.name);
    }
  }
}