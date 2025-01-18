#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Abilities {
  [CustomPropertyDrawer(typeof(TagSet))]
  public class TagSetDrawer : PropertyDrawer
  {
      private const float Padding = 2f;

      public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
      {
          // Get the tags list and calculate height
          SerializedProperty tagsProperty = property.FindPropertyRelative("Tags");
          return EditorGUIUtility.singleLineHeight + (tagsProperty.arraySize + 1) * (EditorGUIUtility.singleLineHeight + Padding);
      }

      public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
      {
          EditorGUI.BeginProperty(position, label, property);

          SerializedProperty tagsProperty = property.FindPropertyRelative("Tags");

          // Draw label
          Rect labelRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
          EditorGUI.LabelField(labelRect, label);

          // Draw the list
          Rect listRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + Padding, position.width, position.height - EditorGUIUtility.singleLineHeight);
          DrawTagList(listRect, tagsProperty);

          EditorGUI.EndProperty();
      }

      private void DrawTagList(Rect position, SerializedProperty tagsProperty)
      {
          // Iterate over the list of tags
          for (int i = 0; i < tagsProperty.arraySize; i++)
          {
              Rect elementRect = new Rect(position.x, position.y + i * (EditorGUIUtility.singleLineHeight + Padding), position.width - 30, EditorGUIUtility.singleLineHeight);
              SerializedProperty elementProperty = tagsProperty.GetArrayElementAtIndex(i);

              // Draw the tag object field
              EditorGUI.ObjectField(elementRect, elementProperty, GUIContent.none);

              // Remove button
              Rect removeButtonRect = new Rect(position.x + position.width - 25, elementRect.y, 25, elementRect.height);
              if (GUI.Button(removeButtonRect, "X"))
              {
                  tagsProperty.DeleteArrayElementAtIndex(i);
                  break;
              }
          }

          // Add button
          Rect addButtonRect = new Rect(position.x, position.y + tagsProperty.arraySize * (EditorGUIUtility.singleLineHeight + Padding), position.width, EditorGUIUtility.singleLineHeight);
          if (GUI.Button(addButtonRect, "Add Tag"))
          {
              tagsProperty.InsertArrayElementAtIndex(tagsProperty.arraySize);
              tagsProperty.GetArrayElementAtIndex(tagsProperty.arraySize - 1).objectReferenceValue = null;
          }
      }
  }
  #endif
}