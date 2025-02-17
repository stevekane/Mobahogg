using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

[CustomPropertyDrawer(typeof(InlineEditorAttribute))]
public class InlineEditorPropertyAttributePropertyDrawer : PropertyDrawer {
  Editor cachedEditor;

  public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
    var propertyHeight = EditorGUI.GetPropertyHeight(property, label, true);
    var fieldRect = new Rect(position.x, position.y, position.width, propertyHeight);
    EditorGUI.PropertyField(fieldRect, property, label, true);
    if (property.objectReferenceValue != null) {
      Editor.CreateCachedEditor(property.objectReferenceValue, null, ref cachedEditor);
      GUILayout.Space(4);
      GUIStyle style = new GUIStyle(EditorStyles.helpBox) { padding = new RectOffset(4, 4, 4, 4) };
      GUILayout.BeginVertical(style);
      cachedEditor.OnInspectorGUI();
      GUILayout.EndVertical();
      GUILayout.Space(4);
    }
  }

  public override VisualElement CreatePropertyGUI(SerializedProperty property) {
    VisualElement container = new VisualElement();
    PropertyField field = new PropertyField(property);
    VisualElement inspectorContainer = new VisualElement();
    System.Action updateInspector = () => {
      inspectorContainer.Clear();
      if (property.objectReferenceValue != null) {
        SerializedObject so = new SerializedObject(property.objectReferenceValue);
        VisualElement inspector = new InspectorElement(so);
        inspectorContainer.Add(inspector);
      }
    };
    container.style.backgroundColor = new StyleColor(new Color(0.2f, 0.2f, 0.2f));
    container.style.paddingLeft = 4;
    container.style.paddingRight = 4;
    container.style.paddingTop = 4;
    container.style.paddingBottom = 20;
    container.Add(field);
    container.Add(inspectorContainer);
    updateInspector();
    field.RegisterCallback<ChangeEvent<Object>>(evt => updateInspector());
    return container;
  }
}