using System;
using System.Linq;
using UnityEditor;

[InitializeOnLoad]
public static class TypeExtensions {
  public static Type[] ConcreteTypes(this Type abstractType) {
    return AppDomain.CurrentDomain.GetAssemblies()
      .SelectMany(a => {
        Type[] types;
        try { types = a.GetTypes(); } catch { types = new Type[0]; }
        return types;
      })
      .Where(x => x.IsSubclassOf(abstractType) && !x.IsAbstract)
      .ToArray();
  }
}