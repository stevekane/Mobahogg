using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

public static class NoFoldoutInspectorUtility {
  public static VisualElement CreateNoFoldoutInspector(SerializedProperty property) {
    var container = new VisualElement();
    var copy = property.Copy();
    var end = property.GetEndProperty();
    bool enterChildren = true;
    while (copy.NextVisible(enterChildren) && !SerializedProperty.EqualContents(copy, end)) {
      enterChildren = false;
      if (copy.depth == property.depth + 1) {
        container.Add(CreateNoFoldoutField(copy.Copy()));
      }
    }
    return container;
  }
  public static VisualElement CreateNoFoldoutField(SerializedProperty property) {
    var container = new VisualElement();
    if (property.hasVisibleChildren && property.propertyType == SerializedPropertyType.Generic) {
      var header = new Label(property.displayName);
      container.Add(header);
      container.Add(CreateNoFoldoutInspector(property));
    }
    else {
      var field = new PropertyField(property, property.displayName);
      container.Add(field);
    }
    return container;
  }
}
