using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System;

[CustomEditor(typeof(AFuckingList))]
public class AFuckingListEditor : Editor {
  VisualElement Elements;

  public override VisualElement CreateInspectorGUI() {
    var root = new VisualElement();
    var itemsProperty = serializedObject.FindProperty("Items");
    Elements = new VisualElement();
    for (var i = 0; i < itemsProperty.arraySize; i++) {
      var property = itemsProperty.GetArrayElementAtIndex(i);
      var listElement = new ListElement(Delete);
      listElement.SetProperty(property);
      Elements.Add(listElement);
    }
    var addButton = new Button(Add);
    addButton.text = "+";
    root.Add(Elements);
    root.Add(addButton);
    return root;
  }

  public void Add() {
    serializedObject.Update();
    var itemsProperty = serializedObject.FindProperty("Items");
    var index = itemsProperty.arraySize;
    itemsProperty.arraySize++;
    var property = itemsProperty.GetArrayElementAtIndex(index);
    property.FindPropertyRelative("Name").stringValue = "New Item";
    serializedObject.ApplyModifiedProperties();
    var listElement = new ListElement(Delete);
    listElement.SetProperty(property);
    Elements.Add(listElement);
  }

  public void Delete(int index) {
    serializedObject.Update();
    var itemsProperty = serializedObject.FindProperty("Items");
    itemsProperty.DeleteArrayElementAtIndex(index);
    serializedObject.ApplyModifiedProperties();
    itemsProperty = serializedObject.FindProperty("Items");
    Elements.RemoveAt(index);
    for (var i = 0; i < Elements.childCount; i++) {
      var element = Elements[i] as ListElement;
      var property = itemsProperty.GetArrayElementAtIndex(i);
      element.SetProperty(property);
    }
  }
}

class ListElement : VisualElement {
  Button DeleteButton;
  PropertyField PropertyField;
  Action<int> OnDelete;

  public ListElement(Action<int> onDelete) {
    OnDelete = onDelete;
    DeleteButton = new Button(DeleteSelf);
    DeleteButton.text = "X";
    PropertyField = new PropertyField();
    style.flexDirection = FlexDirection.Row;
    DeleteButton.style.flexShrink = 1;
    PropertyField.style.flexGrow = 1;
    Add(PropertyField);
    Add(DeleteButton);
  }

  public void SetProperty(SerializedProperty property) {
    PropertyField.BindProperty(property);
    PropertyField.MarkDirtyRepaint();
  }

  void DeleteSelf() {
    OnDelete?.Invoke(parent.IndexOf(this));
  }
}