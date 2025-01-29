#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Abilities {
  [CustomPropertyDrawer(typeof(TagSet))]
  public class TagSetDrawer : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
      EditorGUI.BeginProperty(position, label, property);

      SerializedProperty tagsProperty = property.FindPropertyRelative("Tags");

      // Fetch all available tags in the project
      Tag[] allTags = FindAllTags();
      List<Tag> selectedTags = tagsProperty.ToTagList();

      // Convert selections into a flag-style popup
      string displayText = selectedTags.Count == 0 ? "None" : string.Join(", ", selectedTags.Select(t => t.name));

      if (EditorGUI.DropdownButton(position, new GUIContent(displayText), FocusType.Keyboard)) {
        TagSelectorWindow.Show(tagsProperty, allTags);
      }

      EditorGUI.EndProperty();
    }

    private Tag[] FindAllTags() {
      string[] guids = AssetDatabase.FindAssets("t:Tag");
      return guids.Select(guid => AssetDatabase.LoadAssetAtPath<Tag>(AssetDatabase.GUIDToAssetPath(guid))).ToArray();
    }
  }

  // Extension method to convert SerializedProperty list into a List<Tag>
  public static class SerializedPropertyExtensions {
    public static List<Tag> ToTagList(this SerializedProperty property) {
      List<Tag> tagList = new();
      for (int i = 0; i < property.arraySize; i++) {
        SerializedProperty element = property.GetArrayElementAtIndex(i);
        if (element.objectReferenceValue is Tag tag)
          tagList.Add(tag);
      }
      return tagList;
    }
  }

  // Popup window for selecting tags
  public class TagSelectorWindow : EditorWindow {
    private static SerializedProperty _tagsProperty;
    private static Tag[] _allTags;
    private Vector2 _scrollPos;

    public static void Show(SerializedProperty tagsProperty, Tag[] allTags) {
      TagSelectorWindow window = GetWindow<TagSelectorWindow>(true, "Select Tags", true);
      window.minSize = new Vector2(250, 300);
      _tagsProperty = tagsProperty;
      _allTags = allTags;
      window.ShowPopup();
    }

    private void OnGUI() {
      if (_tagsProperty == null || _allTags == null) return;

      List<Tag> selectedTags = _tagsProperty.ToTagList();

      _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

      foreach (Tag tag in _allTags) {
        bool isSelected = selectedTags.Contains(tag);
        bool newValue = EditorGUILayout.ToggleLeft(tag.name, isSelected);

        if (newValue && !isSelected) {
          int newIndex = _tagsProperty.arraySize;
          _tagsProperty.InsertArrayElementAtIndex(newIndex);
          _tagsProperty.GetArrayElementAtIndex(newIndex).objectReferenceValue = tag;
        }
        else if (!newValue && isSelected) {
          for (int i = 0; i < _tagsProperty.arraySize; i++) {
            if (_tagsProperty.GetArrayElementAtIndex(i).objectReferenceValue == tag) {
              _tagsProperty.DeleteArrayElementAtIndex(i);
              break;
            }
          }
        }
      }

      EditorGUILayout.EndScrollView();

      if (GUILayout.Button("Close")) {
        Close();
      }
    }
  }
}
#endif