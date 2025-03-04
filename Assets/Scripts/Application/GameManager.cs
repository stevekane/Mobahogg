using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.VFX;

public class GameManager : SingletonBehavior<GameManager> {
  static string BOOT_SCENE_NAME = "Boot";
  static public int FIXED_FPS = 60;
  static public int FPS = 60;

  public static Scene? DirectlyLoaded;

  // Called right before whatever scene happens to be open in-editor is loaded
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
  static void OnBoot() {
    // TODO: This isn't technically enough to lock update and fixed
    // together. you also need to disable vsync which is.. sketchy.
    // Consider what this might mean for how to do this "properly"
    Application.targetFrameRate = FPS;
    Time.fixedDeltaTime = 1f/FIXED_FPS;
    VFXManager.fixedTimeStep = 1f/FIXED_FPS;
    #if UNITY_EDITOR
    DirectlyLoaded = SceneManager.GetActiveScene();
    SceneManager.LoadScene(BOOT_SCENE_NAME, LoadSceneMode.Additive);
    // TODO: Doesn't seem like unloading a scene can be done sync anymore?
    // SceneManager.UnloadSceneAsync(BOOT_SCENE_NAME);
    #endif
  }
}