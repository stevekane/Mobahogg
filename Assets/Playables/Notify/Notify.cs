using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

/*
This will be our skeleton class to test this concept of a notify class.

The goal is to see if we can work out how to setup a notify to have ExposedReferences
that correctly can be resolved ( similar to a PlayableGraph with Timeline ) and also
have various events like "Start", "Stop", "Update" etc.

Instances of the class should be stored inside the AnimationMontage and then they
should be used to create playables on the fly which become part of the PlayableGraph
during execution.
*/
[Serializable]
public class Notify : IPlayableAsset {
  public ExposedReference<GameObject> Owner;
  public string Name;
  public int StartFrame;
  public int EndFrame;
  public int FrameDuration => EndFrame-StartFrame;

  public Playable CreatePlayable(PlayableGraph graph, GameObject owner) {
    var playable = ScriptPlayable<NotifyBehavior>.Create(graph, 0);
    playable.SetTime(0);
    playable.SetDuration(duration);
    playable.Pause();
    playable.GetBehaviour().Owner = Owner.Resolve(graph.GetResolver());
    return playable;
  }

  public double duration => (double)FrameDuration/60;
  public IEnumerable<PlayableBinding> outputs => PlayableBinding.None;
}

public class NotifyBehavior : PlayableBehaviour {
  public GameObject Owner;

  public override void OnBehaviourPlay(Playable playable, FrameData info) {
    Debug.Log("Play");
    base.OnBehaviourPlay(playable, info);
  }

  public override void OnBehaviourPause(Playable playable, FrameData info) {
    Debug.Log("Pause");
    base.OnBehaviourPause(playable, info);
  }
}