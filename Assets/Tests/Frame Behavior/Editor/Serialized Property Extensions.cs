using System;
using System.Reflection;
using UnityEditor;

public static class SerializedPropertyExtensions {
  public static SerializedProperty AddManagedReferenceToList(
  this SerializedProperty property,
  object managedReference,
  int index) {
    property.InsertArrayElementAtIndex(index);
    var newProp = property.GetArrayElementAtIndex(index);
    newProp.managedReferenceValue = managedReference;
    return newProp;
  }

  public static int IndexOf(
  this SerializedProperty property,
  SerializedProperty childProperty) {
    var count = property.arraySize;
    for (var i = 0; i < count; i++) {
      if (property.GetArrayElementAtIndex(i).propertyPath == childProperty.propertyPath)
        return i;
    }
    return -1;
  }

  public static object GetTargetObjectOfProperty(this SerializedProperty prop) {
    var path = prop.propertyPath.Replace(".Array.data[", "[");
    object obj = prop.serializedObject.targetObject;
    var elements = path.Split('.');
    foreach (var element in elements) {
      if (element.Contains("[")) {
        var elementName = element.Substring(0, element.IndexOf("["));
        int index = Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
        obj = GetValue(obj, elementName, index);
      } else {
        obj = GetValue(obj, element);
      }
    }
    return obj;
  }

  static object GetValue(object source, string name) {
    if (source == null) return null;
    var type = source.GetType();
    var field = type.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
    if (field == null) {
      var prop = type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase);
      if (prop == null) return null;
      return prop.GetValue(source, null);
    }
    return field.GetValue(source);
  }

  static object GetValue(object source, string name, int index) {
    var enumerable = GetValue(source, name) as System.Collections.IEnumerable;
    if (enumerable == null) return null;
    var enm = enumerable.GetEnumerator();
    for (int i = 0; i <= index; i++) {
      if (!enm.MoveNext()) return null;
    }
    return enm.Current;
  }
}