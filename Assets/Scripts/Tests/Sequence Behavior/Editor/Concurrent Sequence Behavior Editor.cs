#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System;
using System.Linq;
using UnityEngine.Timeline;

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
    asset.EndFrame = 60;
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
      behaviorRow.RowIndex = i;
      behaviorRow.MaxFrame = MAX_FRAME;
      behaviorRow.Update();
    }
  }
}

public class ClipDragRegion : VisualElement {
  float MouseDownX;
  int DragStartFrame;
  int DragEndFrame;
  BehaviorRow BehaviorRow;
  SequenceBehavior SequenceBehavior;

  public int FirstVisibleFrame;
  public int LastVisibleFrame;
  public bool UpdateStartFrame;
  public bool UpdateEndFrame;

  public ClipDragRegion(
  BehaviorRow behaviorRow,
  SequenceBehavior sequenceBehavior) {
    BehaviorRow = behaviorRow;
    SequenceBehavior = sequenceBehavior;
    RegisterCallback<MouseDownEvent>(OnMouseDown);
    RegisterCallback<MouseMoveEvent>(OnMouseMove);
    RegisterCallback<MouseUpEvent>(OnMouseUp);
  }

  void OnMouseDown(MouseDownEvent downEvent) {
    MouseDownX = downEvent.originalMousePosition.x;
    DragStartFrame = SequenceBehavior.StartFrame;
    DragEndFrame = SequenceBehavior.EndFrame;
    this.CaptureMouse();
  }

  void OnMouseUp(MouseUpEvent upEvent) {
    this.ReleaseMouse();
  }

  void OnMouseMove(MouseMoveEvent moveEvent) {
    if (!this.HasMouseCapture())
      return;

    var mousePosition = moveEvent.originalMousePosition.x;
    var mouseDelta = mousePosition-MouseDownX;
    var grandParentWidth = parent.parent.contentRect.width;
    var visibleFrames = LastVisibleFrame-FirstVisibleFrame;
    var framesPerWidth = visibleFrames / (float)grandParentWidth;
    var frameDelta = Mathf.RoundToInt(framesPerWidth * mouseDelta);

    if (frameDelta == 0)
      return;

    const int MAX_FRAME = 600; // hard-coded here because I hate threading data around it's so fucking boring
    if (UpdateStartFrame && UpdateEndFrame) {
      frameDelta = Mathf.Clamp(frameDelta, 0-DragStartFrame, MAX_FRAME-DragEndFrame);
      Undo.RecordObject(SequenceBehavior, "Move Behavior");
      SequenceBehavior.StartFrame = DragStartFrame+frameDelta;
      SequenceBehavior.EndFrame = DragEndFrame+frameDelta;
    } else if (UpdateStartFrame) {
      Undo.RecordObject(SequenceBehavior, "Move StartFrame");
      SequenceBehavior.StartFrame = DragStartFrame+frameDelta;
      SequenceBehavior.StartFrame = Mathf.Clamp(SequenceBehavior.StartFrame, 0, SequenceBehavior.EndFrame-1);
    } else if (UpdateEndFrame) {
      Undo.RecordObject(SequenceBehavior, "Move EndFrame");
      SequenceBehavior.EndFrame = DragEndFrame+frameDelta;
      SequenceBehavior.EndFrame = Mathf.Clamp(SequenceBehavior.EndFrame, SequenceBehavior.StartFrame+1, MAX_FRAME);
    }
    BehaviorRow.Update();
  }
}

public class BehaviorRow : VisualElement {
  ConcurrentSequenceBehaviorsEditor Editor;
  SequenceBehavior SequenceBehavior;
  Label NameLabel;

  public int MaxFrame;
  public int RowIndex;

  public BehaviorRow(ConcurrentSequenceBehaviorsEditor ed, SequenceBehavior b, int maxFrame, int rowIndex) {
    Editor = ed;
    SequenceBehavior = b;
    MaxFrame = maxFrame;
    RowIndex = rowIndex;
    var startRegion = new ClipDragRegion(this, SequenceBehavior);
    startRegion.UpdateStartFrame = true;
    startRegion.LastVisibleFrame = MaxFrame;
    startRegion.style.flexGrow = 10;
    startRegion.style.minWidth = 1;
    startRegion.style.maxWidth = 6;
    startRegion.style.backgroundColor = Color.white;
    var endRegion = new ClipDragRegion(this, SequenceBehavior);
    endRegion.LastVisibleFrame = MaxFrame;
    endRegion.UpdateEndFrame = true;
    endRegion.style.flexGrow = 10;
    endRegion.style.minWidth = 1;
    endRegion.style.maxWidth = 6;
    endRegion.style.backgroundColor = Color.white;
    var middleRegion = new ClipDragRegion(this, SequenceBehavior);
    middleRegion.LastVisibleFrame = MaxFrame;
    middleRegion.UpdateStartFrame = true;
    middleRegion.UpdateEndFrame = true;
    // use reflection to try to access TrackColorAttribute
    var sequenceType = SequenceBehavior.GetType();
    var colorAttribute = (TrackColorAttribute)Attribute.GetCustomAttribute(sequenceType, typeof(TrackColorAttribute));
    middleRegion.style.backgroundColor = colorAttribute != null ? colorAttribute.color : Color.grey;
    middleRegion.style.alignContent = Align.Center;
    middleRegion.style.justifyContent = Justify.Center;
    middleRegion.style.flexGrow = 1;
    middleRegion.style.flexShrink = 1;
    NameLabel = new Label();
    NameLabel.text = b.name;
    NameLabel.style.color = Color.white;
    NameLabel.style.marginLeft = 4;
    NameLabel.style.width = 0; // it displays by overflow
    middleRegion.Add(NameLabel);
    Add(startRegion);
    Add(middleRegion);
    Add(endRegion);
    style.position = Position.Absolute;
    style.height = 20;
    style.top = rowIndex * 24;
    style.alignContent = Align.Center;
    style.flexDirection = FlexDirection.Row;
    RegisterCallback<MouseDownEvent>(e => {
      Editor.detailView.Select(SequenceBehavior);
    });
    Update();
  }

  public void Update() {
    int start = SequenceBehavior.StartFrame;
    int end = SequenceBehavior.EndFrame;
    float pctStart = start / (float)MaxFrame;
    float pctEnd = end / (float)MaxFrame;
    style.top = RowIndex * 24;
    style.left = new Length(pctStart * 100, LengthUnit.Percent);
    style.width = new Length((pctEnd - pctStart) * 100, LengthUnit.Percent);
    NameLabel.text = SequenceBehavior.Name;
  }
}

public class DetailView : VisualElement {
  ConcurrentSequenceBehaviorsEditor Editor;
  VisualElement Container;
  Button DeleteButton;
  SequenceBehavior SelectedBehavior;

  public DetailView(ConcurrentSequenceBehaviorsEditor ed) {
    Editor = ed;
    Container = new VisualElement();
    Add(Container);
    style.marginTop = 10;
    DeleteButton = new Button(DeleteSelectedBehavior) { text = "Delete Behavior" };
    Add(DeleteButton);
  }

  public void Select(SequenceBehavior behavior) {
    SelectedBehavior = behavior;
    Container.Clear();
    if(SelectedBehavior == null) {
      DeleteButton.SetEnabled(false);
    } else {
      DeleteButton.SetEnabled(true);
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
