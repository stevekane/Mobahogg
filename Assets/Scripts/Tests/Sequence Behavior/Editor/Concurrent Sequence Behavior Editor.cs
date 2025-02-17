#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System;
using System.Linq;

[CustomEditor(typeof(ConcurrentSequenceBehaviors))]
public class ConcurrentSequenceBehaviorsEditor : Editor {
  public DetailView detailView;
  public TimelineView timelineView;
  public ConcurrentSequenceBehaviors Data => target as ConcurrentSequenceBehaviors;

  public override VisualElement CreateInspectorGUI() {
    var root = new VisualElement();
    var header = new VisualElement();
    var addButton = new Button(() => ShowAddMenu()) { text = "Add Sequence Behavior" };
    timelineView = new TimelineView(this);
    detailView = new DetailView(this);
    detailView.Select(null);
    header.Add(addButton);
    root.Add(header);
    root.Add(timelineView);
    root.Add(detailView);
    timelineView.RegisterCallback<GeometryChangedEvent>(e => timelineView.Rebuild());
    return root;
  }

  void ShowAddMenu() {
    var menu = new GenericMenu();
    foreach(var t in GetAllBehaviorTypes()) {
      menu.AddItem(new GUIContent(t.Name), false, () => {
        var behavior = CreateBehavior(t);
        timelineView.AddBehavior(behavior);
      });
    }
    menu.ShowAsContext();
  }

  SequenceBehavior CreateBehavior(Type t) {
    var asset = CreateInstance(t) as SequenceBehavior;
    asset.name = t.Name;
    asset.EndFrame = 1;
    AssetDatabase.AddObjectToAsset(asset, Data);
    Data.behaviors.Add(asset);
    EditorUtility.SetDirty(Data);
    AssetDatabase.SaveAssets();
    AssetDatabase.Refresh();
    return asset;
  }

  Type[] GetAllBehaviorTypes() {
    return AppDomain.CurrentDomain.GetAssemblies()
      .SelectMany(a => {
        Type[] types;
        try { types = a.GetTypes(); } catch { types = new Type[0]; }
        return types;
      })
      .Where(x => x.IsSubclassOf(typeof(SequenceBehavior)) && !x.IsAbstract)
      .ToArray();
  }
}

public class TimelineView : VisualElement {
  const int MAX_FRAME = 600;

  ConcurrentSequenceBehaviorsEditor editor;

  public TimelineView(ConcurrentSequenceBehaviorsEditor ed) {
    editor = ed;
    style.position = Position.Relative;
    style.borderBottomWidth = 1;
    style.borderBottomColor = Color.gray;
    style.marginBottom = 10;
  }

  public void Rebuild() {
    Clear();
    for(int i = 0; i < editor.Data.behaviors.Count; i++) {
      AddBehavior(editor.Data.behaviors[i]);
    }
  }

  public void AddBehavior(SequenceBehavior b) {
    Add(new BehaviorRow(editor, b, MAX_FRAME, childCount));
    style.minHeight = editor.Data.behaviors.Count * 24 + 10;
  }

  public void RemoveBehavior(int childIndex) {
    Remove(this[childIndex]);
    style.minHeight = editor.Data.behaviors.Count * 24 + 10;
  }

  public void UpdateBehaviors() {
    for (var i = 0; i < childCount; i++) {
      var behaviorRow = this[i] as BehaviorRow;
      behaviorRow.Update(i, MAX_FRAME);
    }
  }
}

public class BehaviorRow : VisualElement {
  float MouseDownX;
  int DragStartFrame;
  int DragEndFrame;
  ConcurrentSequenceBehaviorsEditor Editor;
  SequenceBehavior SequenceBehavior;
  Label NameLabel;

  public BehaviorRow(ConcurrentSequenceBehaviorsEditor ed, SequenceBehavior b, int maxFrame, int rowIndex) {
    Editor = ed;
    SequenceBehavior = b;
    var startRegion = new VisualElement();
    startRegion.style.width = 10;
    startRegion.style.flexShrink = 0;
    startRegion.style.flexGrow = 0;
    startRegion.style.backgroundColor = Color.white;
    startRegion.RegisterCallback<MouseDownEvent>(e => {
      MouseDownX = e.originalMousePosition.x;
      DragStartFrame = SequenceBehavior.StartFrame;
      Editor.detailView.Select(SequenceBehavior);
      startRegion.CaptureMouse();
    });
    startRegion.RegisterCallback<MouseMoveEvent>(e => {
      if (!startRegion.HasMouseCapture())
        return;
      var mousePosition = e.originalMousePosition.x;
      var mouseDelta = mousePosition-MouseDownX;
      var parentWidth = parent.contentRect.width;
      var framesPerWidth = maxFrame / (float)parentWidth;
      var frameDelta = Mathf.RoundToInt(framesPerWidth * mouseDelta);
      b.StartFrame = DragStartFrame+frameDelta;
      Update(rowIndex, maxFrame);
    });
    startRegion.RegisterCallback<MouseUpEvent>(e => {
      startRegion.ReleaseMouse();
    });
    var endRegion = new VisualElement();
    endRegion.style.width = 10;
    endRegion.style.flexShrink = 0;
    endRegion.style.flexGrow = 0;
    endRegion.style.backgroundColor = Color.white;
    endRegion.RegisterCallback<MouseDownEvent>(e => {
      MouseDownX = e.originalMousePosition.x;
      DragEndFrame = SequenceBehavior.EndFrame;
      Editor.detailView.Select(SequenceBehavior);
      endRegion.CaptureMouse();
    });
    endRegion.RegisterCallback<MouseMoveEvent>(e => {
      if (!endRegion.HasMouseCapture())
        return;
      var mousePosition = e.originalMousePosition.x;
      var mouseDelta = mousePosition-MouseDownX;
      var parentWidth = parent.contentRect.width;
      var framesPerWidth = maxFrame / (float)parentWidth;
      var frameDelta = Mathf.RoundToInt(framesPerWidth * mouseDelta);
      b.EndFrame = DragEndFrame+frameDelta;
      Update(rowIndex, maxFrame);
    });
    endRegion.RegisterCallback<MouseUpEvent>(e => {
      endRegion.ReleaseMouse();
    });
    var middleRegion = new VisualElement();
    middleRegion.style.backgroundColor = Color.grey;
    middleRegion.style.alignContent = Align.Center;
    middleRegion.style.paddingLeft = 8;
    middleRegion.style.flexGrow = 1;
    middleRegion.RegisterCallback<MouseDownEvent>(e => {
      MouseDownX = e.originalMousePosition.x;
      DragStartFrame = SequenceBehavior.StartFrame;
      DragEndFrame = SequenceBehavior.EndFrame;
      Editor.detailView.Select(SequenceBehavior);
      middleRegion.CaptureMouse();
    });
    middleRegion.RegisterCallback<MouseMoveEvent>(e => {
      if (!middleRegion.HasMouseCapture())
        return;
      var mousePosition = e.originalMousePosition.x;
      var mouseDelta = mousePosition-MouseDownX;
      var parentWidth = parent.contentRect.width;
      var framesPerWidth = maxFrame / (float)parentWidth;
      var frameDelta = Mathf.RoundToInt(framesPerWidth * mouseDelta);
      b.StartFrame = DragStartFrame+frameDelta;
      b.EndFrame = DragEndFrame+frameDelta;
      Update(rowIndex, maxFrame);
    });
    middleRegion.RegisterCallback<MouseUpEvent>(e => {
      middleRegion.ReleaseMouse();
    });
    NameLabel = new Label();
    NameLabel.text = b.name;
    NameLabel.style.unityTextAlign = TextAnchor.MiddleLeft;
    NameLabel.style.color = Color.white;
    NameLabel.style.justifyContent = Justify.FlexStart;
    Add(startRegion);
    middleRegion.Add(NameLabel);
    Add(middleRegion);
    Add(endRegion);
    style.position = Position.Absolute;
    style.height = 20;
    style.top = rowIndex * 24;
    style.alignContent = Align.Center;
    style.flexDirection = FlexDirection.Row;
    Update(rowIndex);
  }

  public void Update(int rowIndex, int maxFrame = 600) {
    int start = SequenceBehavior.StartFrame;
    int end = SequenceBehavior.EndFrame;
    style.top = rowIndex * 24;
    float pctStart = start / (float)maxFrame;
    float pctEnd = end / (float)maxFrame;
    style.left = new Length(pctStart * 100, LengthUnit.Percent);
    style.width = new Length((pctEnd - pctStart) * 100, LengthUnit.Percent);
    NameLabel.text = SequenceBehavior.Name;
  }
}

public class DetailView : VisualElement {
  ConcurrentSequenceBehaviorsEditor Editor;
  VisualElement Container;
  Button DeletButton;
  SequenceBehavior SelectedBehavior;

  public DetailView(ConcurrentSequenceBehaviorsEditor ed) {
    Editor = ed;
    Container = new VisualElement();
    Add(Container);
    style.marginTop = 10;
    DeletButton = new Button(DeleteSelectedBehavior) { text = "Delete Behavior" };
    Add(DeletButton);
  }

  public void Select(SequenceBehavior behavior) {
    SelectedBehavior = behavior;
    Container.Clear();
    if(SelectedBehavior == null) {
      DeletButton.SetEnabled(false);
    } else {
      DeletButton.SetEnabled(true);
      var serializedObject = new SerializedObject(behavior);
      var inspector = new InspectorElement(serializedObject);
      inspector.RegisterCallback<SerializedPropertyChangeEvent>(evt => {
        serializedObject.ApplyModifiedProperties();
        Editor.timelineView.UpdateBehaviors();
      });
      Container.Add(inspector);
    }
  }

  void DeleteSelectedBehavior() {
    if(SelectedBehavior == null) return;
    var data = (ConcurrentSequenceBehaviors)Editor.target;
    var index = data.behaviors.IndexOf(SelectedBehavior);
    data.behaviors.Remove(SelectedBehavior);
    Editor.timelineView.RemoveBehavior(index);
    AssetDatabase.RemoveObjectFromAsset(SelectedBehavior);
    EditorUtility.SetDirty(data);
    AssetDatabase.SaveAssets();
    AssetDatabase.Refresh();
    Select(null);
  }
}
#endif
