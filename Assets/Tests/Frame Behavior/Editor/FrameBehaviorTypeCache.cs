using System;
using System.Linq;
using UnityEditor;

[InitializeOnLoad]
public static class FrameBehaviorTypeCache {
  public static Type[] ConcreteTypes;

  static FrameBehaviorTypeCache() {
    ConcreteTypes = AppDomain.CurrentDomain.GetAssemblies()
      .SelectMany(a => {
        Type[] types;
        try { types = a.GetTypes(); } catch { types = new Type[0]; }
        return types;
      })
      .Where(x => x.IsSubclassOf(typeof(FrameBehavior)) && !x.IsAbstract)
      .ToArray();
  }
}