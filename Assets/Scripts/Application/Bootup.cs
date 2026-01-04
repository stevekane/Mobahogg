using UnityEngine;
using UnityEngine.VFX;

static class Bootup
{
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
  static void SubsystemRegistration()
  {
    const float FIXED_DELTA_TIME = 1f/60;
    const int TARGET_FPS = 60;
    Application.targetFrameRate = TARGET_FPS;
    Time.fixedDeltaTime = FIXED_DELTA_TIME;
    VFXManager.fixedTimeStep = FIXED_DELTA_TIME;
  }

  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
  static void AfterAssembliesLoaded()
  {
  }

  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
  static void BeforeSplashScreen()
  {
  }

  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
  static void BeforeSceneLoad()
  {
    const string BOOT_PREFAB_PATH = "Globals";
    var prefab = Resources.Load<GameObject>(BOOT_PREFAB_PATH);
    var go = Object.Instantiate(prefab);
    go.name = prefab.name;
    Object.DontDestroyOnLoad(go);
  }

  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
  static void AfterSceneLoad()
  {
  }
}