using UnityEngine;
using UnityEngine.SceneManagement;

static class Bootup
{
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
  static void SubsystemRegistration()
  {
    //Debug.Log("Subsystem Registration");
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
    //Debug.Log("Before Scene Load. Objects loaded but not awakened.");
  }

  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
  static void AfterSceneLoad()
  {
    //Debug.Log("After scene load. Objects loaded and awakened");
  }
}