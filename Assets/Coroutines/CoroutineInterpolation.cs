using System.Collections;
using UnityEngine;

public static class CoroutineInterpolation
{
  public static IEnumerator LerpLocal(
  this Transform transform,
  Vector3 localPosition,
  int ticks)
  {
    var initialPosition = transform.localPosition;
    for (var i = 0; i < ticks; i++)
    {
      float t = (i + 1f) / ticks;
      transform.localPosition = Vector3.Lerp(initialPosition, localPosition, t);
      yield return new WaitForFixedUpdate();
    }
    transform.localPosition = localPosition;
  }

  public static IEnumerator Lerp(
  this Transform transform,
  Vector3 position,
  int ticks)
  {
    var initialPosition = transform.position;
    for (var i = 0; i < ticks; i++)
    {
      float t = (i + 1f) / ticks;
      transform.position = Vector3.Lerp(initialPosition, position, t);
      yield return new WaitForFixedUpdate();
    }
    transform.position = position;
  }

  public static IEnumerator SlerpLocalEuler(
  this Transform transform,
  float targetX,
  float targetY,
  float targetZ,
  int ticks)
  {
    var initialRotation = transform.localRotation;
    var targetRotation = Quaternion.Euler(targetX, targetY, targetZ);
    for (int i = 0; i < ticks; i++)
    {
      float t = (i + 1f) / ticks;
      transform.localRotation = Quaternion.Slerp(initialRotation, targetRotation, t);
      yield return new WaitForFixedUpdate();
    }
    transform.localRotation = targetRotation;
  }

  public static IEnumerator SlerpLocalEulerX(
  this Transform transform,
  float targetX,
  int ticks)
  {
    var localRotationEuler = transform.localEulerAngles;
    return SlerpLocalEuler(transform, targetX, localRotationEuler.y, localRotationEuler.z, ticks);
  }

  public static IEnumerator SlerpLocalEulerY(
  this Transform transform,
  float targetY,
  int ticks)
  {
    var localRotationEuler = transform.localEulerAngles;
    return SlerpLocalEuler(transform, localRotationEuler.x, targetY, localRotationEuler.z, ticks);
  }

  public static IEnumerator SlerpLocalEulerZ(
  this Transform transform,
  float targetZ,
  int ticks)
  {
    var localRotationEuler = transform.localEulerAngles;
    return SlerpLocalEuler(transform, localRotationEuler.x, localRotationEuler.y, targetZ, ticks);
  }

  public static IEnumerator SlerpEuler(
  this Transform transform,
  float targetX,
  float targetY,
  float targetZ,
  int ticks)
  {
    var initialRotation = transform.rotation;
    var targetRotation = Quaternion.Euler(targetX, targetY, targetZ);
    for (int i = 0; i < ticks; i++)
    {
      float t = (i + 1f) / ticks;
      transform.rotation = Quaternion.Slerp(initialRotation, targetRotation, t);
      yield return new WaitForFixedUpdate();
    }
    transform.rotation = targetRotation;
  }
}