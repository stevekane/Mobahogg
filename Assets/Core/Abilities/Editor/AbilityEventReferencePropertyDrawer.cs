using System;
using System.Reflection;
using UnityEngine;
using UnityEditor;

[Serializable]
[CustomPropertyDrawer(typeof(AbilityEventReference))]
public class AbilityEventReferencePropertyDrawer : PropertyDrawer {
  public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
    var eventName = property.FindPropertyRelative("EventName");
    var ability = property.FindPropertyRelative("Ability");
    var target = ability.objectReferenceValue;
    var dropDownText = eventName.stringValue == "" ? "Select event" : eventName.stringValue;
    var p1 = new Rect(position); p1.width /= 2;
    EditorGUI.ObjectField(p1, ability, GUIContent.none);
    var p2 = new Rect(position); p2.width /= 2; p2.x += p2.width;
    if (target && EditorGUI.DropdownButton(p2, new GUIContent(dropDownText), FocusType.Keyboard)) {
      var menu = new GenericMenu();
      var props = target.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
      foreach (var prop in props) {
        if (prop.FieldType == typeof(EventSource)) {
          menu.AddItem(new GUIContent(prop.Name), eventName.stringValue == prop.Name, delegate {
            eventName.stringValue = prop.Name;
            property.serializedObject.ApplyModifiedProperties();
            property.serializedObject.Update();
          });
        }
      }
      if (menu.GetItemCount() == 0) {
        menu.AddDisabledItem(new GUIContent("No valid events found."), false);
      }
      menu.ShowAsContext();
    }
  }
}
