using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(SceneBinding<>))]
public class SceneBindingPropertyDrawer : PropertyDrawer {
  public static string Joined (List<string> strings) {
    string output = "";
    foreach(var s in strings) {
      output += s;
      output += ".";
    }
    return output;
  }

  public static List<string> ParsePropertyPath(string propertyPath) {
    var tokens = new List<string>();
    string[] parts = propertyPath.Split('.');
    foreach (var part in parts) {
      if (part == "Array")
        continue;
      if (part.StartsWith("data[")) {
        int start = part.IndexOf('[') + 1;
        int end = part.IndexOf(']');
        if (end > start) {
          string indexStr = part.Substring(start, end - start);
          tokens.Add(indexStr);
        }
      } else {
        tokens.Add(part);
      }
    }
    return tokens;
  }

  static FieldInfo GetTargetFieldInfo(Type baseType, List<string> tokens) {
    Type currentType = baseType;
    FieldInfo fieldInfo = null;
    foreach (var token in tokens) {
      if (int.TryParse(token, out int index)) {
        if (currentType.IsArray) {
          currentType = currentType.GetElementType();
        } else if (currentType.IsGenericType && currentType.GetGenericTypeDefinition() == typeof(List<>)) {
          currentType = currentType.GetGenericArguments()[0];
        } else {
          return null;
        }
      } else {
        fieldInfo = currentType.GetField(token, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (fieldInfo == null)
          return null;
        currentType = fieldInfo.FieldType;
      }
    }
    return fieldInfo;
  }

  static Type BindingType(SerializedProperty property) {
    var targetObjectType = property.serializedObject.targetObject.GetType();
    var fieldPath = ParsePropertyPath(property.propertyPath);
    var field = GetTargetFieldInfo(targetObjectType, fieldPath);
    if (field != null && field.FieldType.IsGenericType) {
      return field.FieldType.GetGenericArguments()[0];
    } else {
      return default;
    }
  }

  public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
    var fieldRect = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
    var expectedType = BindingType(property);
    if (Selection.activeGameObject != null &&
        Selection.activeGameObject.TryGetComponent(out IExposedPropertyTable propertyTable)) {
      var propertyName = new PropertyName(property.propertyPath);
      var current = propertyTable.GetReferenceValue(propertyName, out var valid);
      EditorGUI.BeginChangeCheck();
      var next = EditorGUI.ObjectField(fieldRect, current, expectedType, true);
      if (EditorGUI.EndChangeCheck()) {
        propertyTable.SetReferenceValue(propertyName, next);
      }
    } else {
      EditorGUI.LabelField(fieldRect, $"SceneBinding<{expectedType.ToString()}>");
    }
  }

  public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
    return EditorGUIUtility.singleLineHeight;
  }
}