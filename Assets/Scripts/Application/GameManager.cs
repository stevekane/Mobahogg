using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : SingletonBehavior<GameManager> {
  public static string BOOT_SCENE_NAME = "Boot";
  static public int FIXED_FPS = 60;
  static public int FPS = 60;

  // Called right before whatever scene happens to be open in-editor is loaded
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
  static void OnBoot() {
    // TODO: This isn't technically enough to lock update and fixed
    // together. you also need to disable vsync which is.. sketchy.
    // Consider what this might mean for how to do this "properly"
    Application.targetFrameRate = FPS;
    Time.fixedDeltaTime = 1f/FIXED_FPS;
    SceneManager.LoadScene(BOOT_SCENE_NAME);
  }

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