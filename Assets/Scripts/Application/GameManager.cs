using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : SingletonBehavior<GameManager> {
  public static string TEST_BATTLE_FLAG_NAME = "Test_Battle";
  public static string BOOT_SCENE_NAME = "Boot";
  public static string TITLE_SCENE_NAME = "TitleScreen";
  public static string MATCH_LAUNCHER_SCENE_NAME = "MatchLauncher";
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
    #if UNITY_EDITOR
    var openScene = SceneManager.GetActiveScene().name;
    SceneManager.LoadScene(BOOT_SCENE_NAME, LoadSceneMode.Additive);
    #else
    SceneManager.LoadScene(FIRST_SCENE_NAME);
    #endif
  }
}