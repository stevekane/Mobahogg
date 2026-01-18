using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomPropertyDrawer(typeof(PolymorphicList<>), true)]
public class PolymorphicListDrawer : PropertyDrawer
{
  const string ItemsFieldName = "items";

  readonly Dictionary<string, ReorderableList> listsByPropertyPath = new();

  public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
  {
    var list = GetOrCreateList(property, label);
    return list.GetHeight();
  }

  public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
  {
    var list = GetOrCreateList(property, label);
    list.DoList(position);
  }

  ReorderableList GetOrCreateList(SerializedProperty property, GUIContent label)
  {
    // One ReorderableList per property path (handles multiple instances / multi-object editing nicely).
    if (listsByPropertyPath.TryGetValue(property.propertyPath, out var list))
      return list;

    var itemsProp = property.FindPropertyRelative(ItemsFieldName);
    if (itemsProp == null || !itemsProp.isArray)
    {
      list = new ReorderableList(new SerializedObject(property.serializedObject.targetObjects), property, true, true, false, false);
      listsByPropertyPath[property.propertyPath] = list;
      return list;
    }

    list = new ReorderableList(property.serializedObject, itemsProp, true, true, true, false);

    list.drawHeaderCallback = rect =>
    {
      EditorGUI.LabelField(rect, label);
    };

    list.elementHeightCallback = index =>
    {
      if (index < 0 || index >= itemsProp.arraySize) return EditorGUIUtility.singleLineHeight;
      var element = itemsProp.GetArrayElementAtIndex(index);
      return EditorGUI.GetPropertyHeight(element, true) + 6f;
    };

    list.drawElementCallback = (rect, index, active, focused) =>
    {
      if (index < 0 || index >= itemsProp.arraySize) return;

      var element = itemsProp.GetArrayElementAtIndex(index);

      rect.y += 2f;
      rect.height -= 4f;

      // Delete button area.
      const float deleteWidth = 22f;
      var deleteRect = new Rect(rect.xMax - deleteWidth, rect.y, deleteWidth, EditorGUIUtility.singleLineHeight);

      // Property field area.
      var fieldRect = new Rect(rect.x, rect.y, rect.width - deleteWidth - 4f, rect.height);

      var typeName = element.managedReferenceValue?.GetType().Name ?? "<null>";
      EditorGUI.PropertyField(fieldRect, element, new GUIContent(typeName), true);

      if (GUI.Button(deleteRect, "X"))
      {
        // ManagedReference needs two-step deletion sometimes (null then delete) to be reliable.
        element.managedReferenceValue = null;
        property.serializedObject.ApplyModifiedProperties();

        itemsProp.DeleteArrayElementAtIndex(index);
        property.serializedObject.ApplyModifiedProperties();
      }
    };

    list.onAddDropdownCallback = (rect, l) =>
    {
      var baseType = GetListElementBaseType(fieldInfo);
      var menu = new GenericMenu();

      if (baseType == null)
      {
        menu.AddDisabledItem(new GUIContent("Could not determine element base type"));
        menu.DropDown(rect);
        return;
      }

      var candidates = FindConcreteCandidates(baseType);
      if (candidates.Count == 0)
      {
        menu.AddDisabledItem(new GUIContent($"No concrete types found for {baseType.Name}"));
        menu.DropDown(rect);
        return;
      }

      foreach (var t in candidates)
      {
        var niceName = ObjectNames.NicifyVariableName(t.Name);
        var path = string.IsNullOrEmpty(t.Namespace) ? niceName : $"{t.Namespace}/{niceName}";

        menu.AddItem(new GUIContent(path), false, () =>
        {
          property.serializedObject.Update();

          var array = itemsProp;
          var index = array.arraySize;
          array.InsertArrayElementAtIndex(index);

          var newElement = array.GetArrayElementAtIndex(index);
          newElement.managedReferenceValue = Activator.CreateInstance(t);

          property.serializedObject.ApplyModifiedProperties();
        });
      }

      menu.DropDown(rect);
    };

    // Keep Unity’s built-in minus / remove behavior? We add per-row delete already, so disable footer remove.
    // If you prefer the standard footer minus button too, set list.displayRemove = true and implement onRemoveCallback.
    list.displayRemove = false;

    listsByPropertyPath[property.propertyPath] = list;
    return list;
  }

  static Type GetListElementBaseType(System.Reflection.FieldInfo wrapperFieldInfo)
  {
    // wrapperFieldInfo.FieldType should be PolymorphicList<TBase>.
    var ft = wrapperFieldInfo?.FieldType;
    if (ft == null) return null;

    if (ft.IsGenericType && ft.GetGenericTypeDefinition() == typeof(PolymorphicList<>))
      return ft.GetGenericArguments()[0];

    return null;
  }

  static List<Type> FindConcreteCandidates(Type baseType)
  {
    // baseType could be an interface (including a closed generic interface type).
    // TypeCache is fast & Unity-idiomatic for editor reflection.
    var types = TypeCache.GetTypesDerivedFrom(baseType);

    // Filter to "constructible" types.
    var candidates = types
      .Where(t =>
        t != null &&
        !t.IsAbstract &&
        !t.IsInterface &&
        !t.ContainsGenericParameters &&
        t.GetConstructor(Type.EmptyTypes) != null)
      .ToList();

    // If baseType is interface, some implementations might not show up in GetTypesDerivedFrom on older versions,
    // so we also scan all types derived from object and check assignability (still using TypeCache).
    if (candidates.Count == 0 && baseType.IsInterface)
    {
      foreach (var t in TypeCache.GetTypesDerivedFrom<object>())
      {
        if (t == null) continue;
        if (t.IsAbstract || t.IsInterface || t.ContainsGenericParameters) continue;
        if (t.GetConstructor(Type.EmptyTypes) == null) continue;
        if (baseType.IsAssignableFrom(t))
          candidates.Add(t);
      }

      candidates = candidates.Distinct().ToList();
    }

    candidates.Sort((a, b) => string.Compare(a.FullName, b.FullName, StringComparison.Ordinal));
    return candidates;
  }
}