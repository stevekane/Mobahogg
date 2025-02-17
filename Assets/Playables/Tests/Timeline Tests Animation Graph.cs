using Animation;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class TimelineTestsAnimationGraph : AnimationGraph, IExposedPropertyTable {
  [System.Serializable]
  class ExposedPropertyDict : SerializableDictionary<PropertyName, Object> {}

  [SerializeField, HideInInspector] ExposedPropertyDict References = new();
  [SerializeField] Animator Animator;
  [SerializeField] LocalClock LocalClock;
  [SerializeField] AnimationClip IdleClip;
  [SerializeField, InlineEditor] AnimationMontage AnimationMontage;

  ScriptPlayable<SlotBehavior> SlotPlayable;
  PlayableGraph Graph;

  public void SetReferenceValue(PropertyName id, Object value) {
    References[id] = value;
  }

  public Object GetReferenceValue(PropertyName id, out bool idValid) {
    idValid = References.TryGetValue(id, out Object value);
    return value;
  }

  public void ClearReferenceValue(PropertyName id) {
    References.Remove(id);
  }

  [ContextMenu("Clear Bound References")]
  void ClearBoundReferences() {
    References.Clear();
  }

  public override SlotBehavior SlotBehavior => SlotPlayable.GetBehaviour();
  public override PlayableGraph PlayableGraph => Graph;

  void Awake() {
    Graph = PlayableGraph.Create(name);
    Graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
    Graph.SetResolver(this);
    Graph.Play();
    var idle = AnimationClipPlayable.Create(Graph, IdleClip);
    SlotPlayable = ScriptPlayable<SlotBehavior>.Create(Graph, 1);
    SlotPlayable.GetBehaviour().Connect(idle);
    AnimationPlayableOutput.Create(Graph, name, Animator).SetSourcePlayable(SlotPlayable);
  }

  void OnDestroy() {
    if (Graph.IsValid()) {
      Graph.Destroy();
    }
  }

  void FixedUpdate() {
    Graph.Evaluate(LocalClock.DeltaTime());
  }
}