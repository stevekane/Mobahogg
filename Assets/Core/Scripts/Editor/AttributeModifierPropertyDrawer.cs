using System;
using UnityEngine;
using UnityEditor;

[Serializable]
[CustomPropertyDrawer(typeof(AttributeValue))]
public class AttributeValuePropertyDrawer : PropertyDrawer {
  public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
    EditorGUI.BeginProperty(position, label, property);
    Rect p = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

    var indent = EditorGUI.indentLevel;
    EditorGUI.indentLevel = 0;

    EditorGUI.BeginChangeCheck();
    var FlatProp = property.FindPropertyRelative("Flat");
    var AddFactorProp = property.FindPropertyRelative("AddFactor");
    var MultFactorProp = property.FindPropertyRelative("MultFactor");

    p.width /= 3;
    p.width -= 8;
    var labels = new[] { new GUIContent("+"), new GUIContent("+x"), new GUIContent("*x") };
    EditorGUIUtility.labelWidth = EditorStyles.label.CalcSize(labels[0]).x;
    var newFlat = EditorGUI.FloatField(p, labels[0], FlatProp.floatValue);
    p.x += p.width + 4;
    EditorGUIUtility.labelWidth = EditorStyles.label.CalcSize(labels[1]).x;
    var newAF = EditorGUI.FloatField(p, labels[1], AddFactorProp.floatValue);
    p.x += p.width + 4;
    EditorGUIUtility.labelWidth = EditorStyles.label.CalcSize(labels[2]).x;
    var newMF = EditorGUI.FloatField(p, labels[2], MultFactorProp.floatValue);

    if (EditorGUI.EndChangeCheck()) {
      FlatProp.floatValue = newFlat;
      AddFactorProp.floatValue = newAF;
      MultFactorProp.floatValue = newMF;
    }

    EditorGUI.indentLevel = indent;

    EditorGUI.EndProperty();
  }
}
