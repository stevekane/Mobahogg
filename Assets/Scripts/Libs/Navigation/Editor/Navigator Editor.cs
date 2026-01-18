using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

[CustomEditor(typeof(Navigator))]
public class NavigatorEditor : Editor
{
  public override VisualElement CreateInspectorGUI()
  {
    var root = new VisualElement();
    InspectorElement.FillDefaultInspector(root, serializedObject, this);
    return root;
  }
}