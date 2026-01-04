using UnityEngine;
using UnityEngine.VFX;

static class Bootup
{
  const string BOOT_PREFAB_PATH = "Globals";

  static public float FIXED_DELTA_TIME = 1f/60;
  static public int TARGET_FPS = 60;

  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
  static void SubsystemRegistration()
  {
    //Debug.Log("Subsystem Registration");
    Application.targetFrameRate = TARGET_FPS;
    Time.fixedDeltaTime = FIXED_DELTA_TIME;
    VFXManager.fixedTimeStep = FIXED_DELTA_TIME;
  }

  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
  static void AfterAssembliesLoaded()
  {
    //Debug.Log("After Assemblies loaded");
  }

  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
  static void BeforeSplashScreen()
  {
    //Debug.Log("Before SplashScreen");
  }

  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
  static void BeforeSceneLoad()
  {
    var prefab = Resources.Load<GameObject>(BOOT_PREFAB_PATH);
    var go = Object.Instantiate(prefab);
    go.name = prefab.name;
    Object.DontDestroyOnLoad(go);
    Debug.Log("Loaded global managers");
  }

  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
  static void AfterSceneLoad()
  {
    // Debug.Log("After scene load. Objects loaded and awakened");
  }
}