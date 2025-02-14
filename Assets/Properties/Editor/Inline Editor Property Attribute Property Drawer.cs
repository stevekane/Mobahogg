using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(InlineEditorAttribute))]
public class InlineEditorPropertyAttributePropertyDrawer : PropertyDrawer {
  Editor cachedEditor;

  public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
    var propertyHeight = EditorGUI.GetPropertyHeight(property, label, true);
    var fieldRect = new Rect(position.x, position.y, position.width, propertyHeight);
    EditorGUI.PropertyField(fieldRect, property, label, true);
    if (property.objectReferenceValue != null) {
      Editor.CreateCachedEditor(property.objectReferenceValue, null, ref cachedEditor);
      cachedEditor.OnInspectorGUI();
      EditorGUILayout.Space();
    }
  }
}