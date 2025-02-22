using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System;

[CustomEditor(typeof(AFuckingList))]
public class AFuckingListEditor : Editor {
  VisualElement Elements;
  GenericMenu TypeSelectionMenu;

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
    var addButton = new Button(DisplayConcreteTypeMenu);
    addButton.text = "+";
    TypeSelectionMenu = new GenericMenu();
    var types = FrameBehaviorTypeCache.ConcreteTypesFor(typeof(AFuckingItem));
    foreach (var type in types) {
      TypeSelectionMenu.AddItem(new GUIContent(type.Name), false, () => Add(type));
    }
    root.Add(Elements);
    root.Add(addButton);
    root.Bind(serializedObject);
    return root;
  }

  public void DisplayConcreteTypeMenu() {
    TypeSelectionMenu.ShowAsContext();
  }

  public void Add(Type type) {
    var instance = (AFuckingItem)Activator.CreateInstance(type);
    instance.Name = type.Name;
    serializedObject.Update();
    var itemsProperty = serializedObject.FindProperty("Items");
    var index = itemsProperty.arraySize;
    itemsProperty.arraySize++;
    var property = itemsProperty.GetArrayElementAtIndex(index);
    property.managedReferenceValue = instance;
    serializedObject.ApplyModifiedPropertiesWithoutUndo();
    var listElement = new ListElement(Delete);
    listElement.SetProperty(property);
    Elements.Add(listElement);
  }

  public void Delete(int index) {
    serializedObject.Update();
    var itemsProperty = serializedObject.FindProperty("Items");
    itemsProperty.DeleteArrayElementAtIndex(index);
    serializedObject.ApplyModifiedPropertiesWithoutUndo();
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
    DeleteButton.style.flexShrink = 1;
    PropertyField = new PropertyField();
    PropertyField.style.flexGrow = 1;
    Add(PropertyField);
    Add(DeleteButton);
    style.flexDirection = FlexDirection.Row;
  }

  public void SetProperty(SerializedProperty property) {
    PropertyField.Unbind();
    PropertyField.BindProperty(property);
  }

  void DeleteSelf() {
    OnDelete?.Invoke(parent.IndexOf(this));
  }
}