using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Unity.VisualScripting;

[CustomEditor(typeof(AFuckingList))]
public class AFuckingListEditor : Editor {
  public override VisualElement CreateInspectorGUI() {
    var list = new PolymorphicList<AFuckingItem, PropertyFieldBinder>();
    list.BindProperty(serializedObject.FindProperty("Items"));
    return list;
  }
}

public class PropertyFieldBinder : PropertyField, IPropertyBinder {
  public void BindProperty(SerializedProperty property) {
    bindingPath = property.propertyPath;
    this.Bind(property.serializedObject);
  }
}

public class PolymorphicListElement<E> : VisualElement, IPropertyBinder
where
E : VisualElement, IBindable, IPropertyBinder, new() {
  Button DeleteButton;
  VisualElement DeleteButtonContainer;
  VisualElement DetailsContainer;
  E Details;
  Action<int> OnDelete;

  public E GetDetails => Details;

  public PolymorphicListElement(Action<int> onDelete) {
    OnDelete = onDelete;
    DetailsContainer = new();
    DetailsContainer.style.flexGrow = 1;
    DeleteButton = new Button(DeleteSelf);
    DeleteButton.text = "Delete";
    DeleteButtonContainer = new();
    DeleteButtonContainer.style.width = 60;
    DeleteButtonContainer.style.flexDirection = FlexDirection.Column;
    Add(DetailsContainer);
    Add(DeleteButtonContainer);
    DeleteButtonContainer.Add(DeleteButton);
    style.flexDirection = FlexDirection.Row;
  }

  public void BindProperty(SerializedProperty property) {
    if (Details != null) {
      Details.Unbind();
      DetailsContainer.Clear();
    }
    Details = new E();
    Details.BindProperty(property);
    DetailsContainer.Add(Details);
  }

  void DeleteSelf() {
    OnDelete?.Invoke(parent.IndexOf(this));
  }
}

public class PolymorphicList<T, E> : VisualElement, IPropertyBinder where
E : VisualElement, IBindable, IPropertyBinder, new() {
  VisualElement ListContainer;
  GenericMenu TypeSelectionMenu;
  string PropertyPath;
  SerializedObject SerializedObject;

  public EventSource OnChange = new();
  public List<E> Elements = new();

  public PolymorphicList() {
    ListContainer = new VisualElement { name = "ListItemsContainer" };
    Add(ListContainer);
    Button addButton = new Button(DisplayConcreteTypeMenu);
    addButton.text = $"Add New {typeof(T).HumanName()}";
    Add(addButton);
  }

  public void BindProperty(SerializedProperty property) {
    PropertyPath = property.propertyPath;
    SerializedObject = property.serializedObject;
    RebuildUI();
    this.Bind(SerializedObject);
  }

  void RebuildUI() {
    ListContainer.Clear();
    SerializedProperty itemsProperty = SerializedObject.FindProperty(PropertyPath);
    for (int i = 0; i < itemsProperty.arraySize; i++) {
      SerializedProperty property = itemsProperty.GetArrayElementAtIndex(i);
      PolymorphicListElement<E> listElement = new PolymorphicListElement<E>(Delete);
      listElement.BindProperty(property);
      Elements.Add(listElement.GetDetails);
      ListContainer.Add(listElement);
    }
    OnChange.Fire();
    SetupTypeSelectionMenu();
  }

  private void SetupTypeSelectionMenu() {
    TypeSelectionMenu = new GenericMenu();
    var types = FrameBehaviorTypeCache.ConcreteTypesFor(typeof(T));
    foreach (var type in types) {
      TypeSelectionMenu.AddItem(new GUIContent(type.Name), false, () => Add(type));
    }
  }

  private void DisplayConcreteTypeMenu() {
    TypeSelectionMenu?.ShowAsContext();
  }

  private void Add(Type type) {
    // Create a new instance of the concrete type.
    T instance = (T)Activator.CreateInstance(type);
    SerializedObject.Update();
    SerializedProperty itemsProperty = SerializedObject.FindProperty(PropertyPath);
    int index = itemsProperty.arraySize;
    itemsProperty.arraySize++;
    SerializedProperty property = itemsProperty.GetArrayElementAtIndex(index);
    property.managedReferenceValue = instance;
    SerializedObject.ApplyModifiedPropertiesWithoutUndo();

    // Create and add a new list element.
    PolymorphicListElement<E> listElement = new PolymorphicListElement<E>(Delete);
    listElement.BindProperty(property);
    Elements.Add(listElement.GetDetails);
    ListContainer.Add(listElement);
    OnChange.Fire();
  }

  private void Delete(int index) {
    SerializedObject.Update();
    SerializedProperty itemsProperty = SerializedObject.FindProperty(PropertyPath);
    itemsProperty.DeleteArrayElementAtIndex(index);
    SerializedObject.ApplyModifiedPropertiesWithoutUndo();

    // Rebuild the list UI to ensure that binding paths are updated.
    itemsProperty = SerializedObject.FindProperty(PropertyPath);
    Elements.Clear();
    ListContainer.RemoveAt(index);
    for (int i = 0; i < ListContainer.childCount; i++) {
      PolymorphicListElement<E> element = ListContainer[i] as PolymorphicListElement<E>;
      SerializedProperty property = itemsProperty.GetArrayElementAtIndex(i);
      element.BindProperty(property);
      Elements.Add(element.GetDetails);
    }
    OnChange.Fire();
  }
}