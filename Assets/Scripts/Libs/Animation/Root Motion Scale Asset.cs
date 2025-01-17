using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

[CreateAssetMenu(fileName = "RootMotion Scale", menuName = "Animation/RootMotion Scale")]
public class RootMotionScaleAsset : PlayableAsset {
  public float PositionScale = 1;
  public float RotationScale = 1;
  public override Playable CreatePlayable(PlayableGraph graph, GameObject owner) {
    return AnimationScriptPlayable.Create(graph, new RootMotionScaleJob(PositionScale, RotationScale));
  }
}

public struct RootMotionScaleJob : IAnimationJob {
  public float PositionScale;
  public float RotationScale;

  public RootMotionScaleJob(float scale) {
    PositionScale = scale;
    RotationScale = scale;
  }

  public RootMotionScaleJob(float positionScale = 1, float rotationScale = 1) {
    PositionScale = positionScale;
    RotationScale = rotationScale;
  }

  public void ProcessRootMotion(AnimationStream stream) {
    var inputStream = stream.GetInputStream(0);
    stream.velocity = PositionScale * inputStream.velocity;
    stream.angularVelocity = RotationScale * inputStream.angularVelocity;
  }

  public void ProcessAnimation(AnimationStream stream) {
    stream.CopyAnimationStreamMotion(stream.GetInputStream(0));
  }
}