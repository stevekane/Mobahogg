using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System;

/*
The visual element created here is polymorphic in the following parameters:

Abstract Type of the Items
  Any System.Type ( not, this won't make much sense if it's not abstract )
Display of the Item
  (SerializedProperty (representing the item)) -> Visual Element
*/
[CustomEditor(typeof(AFuckingList))]
public class AFuckingListEditor : Editor {
  public override VisualElement CreateInspectorGUI() {
    var list = new PolymorphicList();
    list.BindSerializedObject(serializedObject);
    return list;
  }
}

public class PolymorphicListElement : VisualElement {
  Button DeleteButton;
  VisualElement DetailsContainer;
  VisualElement Details;
  Action<int> OnDelete;

  public PolymorphicListElement(Action<int> onDelete) {
    OnDelete = onDelete;
    DeleteButton = new Button(DeleteSelf);
    DeleteButton.text = "X";
    DeleteButton.style.flexShrink = 1;
    DetailsContainer = new();
    DetailsContainer.style.flexGrow = 1;
    Add(DetailsContainer);
    Add(DeleteButton);
    style.flexDirection = FlexDirection.Row;
  }

  public void SetProperty(SerializedProperty property) {
    if (Details != null) {
      Details.Unbind();
      DetailsContainer.Clear();
    }
    Details = NoFoldoutInspectorUtility.CreateNoFoldoutInspector(property);
    Details.Bind(property.serializedObject);
    DetailsContainer.Add(Details);
  }

  void DeleteSelf() {
    OnDelete?.Invoke(parent.IndexOf(this));
  }
}

public class PolymorphicList : VisualElement {
  VisualElement ListContainer;
  GenericMenu TypeSelectionMenu;
  SerializedObject SerializedObject;

  public PolymorphicList() {
    ListContainer = new VisualElement { name = "ListItemsContainer" };
    Add(ListContainer);
    Button addButton = new Button(DisplayConcreteTypeMenu) { text = "+" };
    Add(addButton);
  }

  public void BindSerializedObject(SerializedObject so) {
    SerializedObject = so;
    RebuildUI();
    this.Bind(so);
  }

  void RebuildUI() {
    ListContainer.Clear();
    SerializedProperty itemsProperty = SerializedObject.FindProperty("Items");
    for (int i = 0; i < itemsProperty.arraySize; i++) {
      SerializedProperty property = itemsProperty.GetArrayElementAtIndex(i);
      PolymorphicListElement listElement = new PolymorphicListElement(Delete);
      listElement.SetProperty(property);
      ListContainer.Add(listElement);
    }
    SetupTypeSelectionMenu();
  }

  private void SetupTypeSelectionMenu() {
    TypeSelectionMenu = new GenericMenu();
    var types = FrameBehaviorTypeCache.ConcreteTypesFor(typeof(AFuckingItem));
    foreach (var type in types) {
        TypeSelectionMenu.AddItem(new GUIContent(type.Name), false, () => Add(type));
    }
  }

  private void DisplayConcreteTypeMenu() {
    TypeSelectionMenu?.ShowAsContext();
  }

  private void Add(Type type) {
    // Create a new instance of the concrete type.
    AFuckingItem instance = (AFuckingItem)Activator.CreateInstance(type);
    instance.Name = type.Name;

    SerializedObject.Update();
    SerializedProperty itemsProperty = SerializedObject.FindProperty("Items");
    int index = itemsProperty.arraySize;
    itemsProperty.arraySize++;
    SerializedProperty property = itemsProperty.GetArrayElementAtIndex(index);
    property.managedReferenceValue = instance;
    SerializedObject.ApplyModifiedPropertiesWithoutUndo();

    // Create and add a new list element.
    PolymorphicListElement listElement = new PolymorphicListElement(Delete);
    listElement.SetProperty(property);
    ListContainer.Add(listElement);
  }

  private void Delete(int index) {
    SerializedObject.Update();
    SerializedProperty itemsProperty = SerializedObject.FindProperty("Items");
    itemsProperty.DeleteArrayElementAtIndex(index);
    SerializedObject.ApplyModifiedPropertiesWithoutUndo();

    // Rebuild the list UI to ensure that binding paths are updated.
    itemsProperty = SerializedObject.FindProperty("Items");
    ListContainer.RemoveAt(index);
    for (int i = 0; i < ListContainer.childCount; i++) {
      PolymorphicListElement element = ListContainer[i] as PolymorphicListElement;
      SerializedProperty property = itemsProperty.GetArrayElementAtIndex(i);
      element.SetProperty(property);
    }
  }
}