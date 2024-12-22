using UnityEngine;
using UnityEngine.Timeline;

[TrackBindingType(typeof(Transform))]
[TrackClipType(typeof(VibrateClip))]
public class VibrateTrack : TrackAsset {}