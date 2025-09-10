using System;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;

public class HPPC_Hookshot : MonoBehaviour
{
  static void RemoveKilled<T>(NativeList<T> living, NativeQueue<int> killed) where T : unmanaged
  {
    var killedCount = killed.Count;
    for (var i = 0; i < killedCount; i++)
    {
      living.RemoveAt(killed.Dequeue());
    }
  }

  const int MAX_JOBS = 64;

  NativeList<HS_HookshotState> HookShotStates;
  NativeList<HS_TravelingState> TravelingStates;
  NativeList<HS_PullingState> PullingStates;
  NativeQueue<int> KilledTravelingIndices;
  NativeQueue<int> KilledPullingIndices;
  NativeQueue<HS_TravelingState> NewTravelingStates;
  NativeQueue<HS_PullingState> NewPullingStates;

  void Start()
  {
    HookShotStates = new NativeList<HS_HookshotState>(MAX_JOBS, Allocator.Persistent);
    TravelingStates = new NativeList<HS_TravelingState>(MAX_JOBS, Allocator.Persistent);
    PullingStates = new NativeList<HS_PullingState>(MAX_JOBS, Allocator.Persistent);
    KilledTravelingIndices = new NativeQueue<int>(Allocator.Persistent);
    KilledPullingIndices = new NativeQueue<int>(Allocator.Persistent);
    NewTravelingStates = new NativeQueue<HS_TravelingState>(Allocator.Persistent);
    NewPullingStates = new NativeQueue<HS_PullingState>(Allocator.Persistent);
    HookShotStates.Add(new HS_HookshotState());
  }

  void OnDestroy()
  {
    HookShotStates.Dispose();
    TravelingStates.Dispose();
    PullingStates.Dispose();
    KilledTravelingIndices.Dispose();
    KilledPullingIndices.Dispose();
    NewTravelingStates.Dispose();
    NewPullingStates.Dispose();
  }

  void FixedUpdate()
  {
    RemoveKilled(TravelingStates, KilledTravelingIndices);
    RemoveKilled(PullingStates, KilledPullingIndices);
    var travelingJob = new HS_TravelingJob
    {
      Travelings = TravelingStates.AsArray(),
      NewPullings = NewPullingStates.AsParallelWriter(),
      KilledTravelingIndices = KilledTravelingIndices.AsParallelWriter()
    };
    var pullingJob = new HS_PullingJob
    {
      Pullings = PullingStates.AsArray()
    };
    var travelingJobHandle = travelingJob.Schedule(TravelingStates.Length, innerloopBatchCount: 1);
    var pullingJobHandle = pullingJob.Schedule(PullingStates.Length, innerloopBatchCount: 1);
    JobHandle.CombineDependencies(travelingJobHandle, pullingJobHandle).Complete();
    DoEffects();
  }

  void DoEffects()
  {
    for (var i = 0; i < TravelingStates.Length; i++)
    {
      Debug.Log($"Traveling for: {TravelingStates[i].Frames} Frames");
    }
    for (var i = 0; i < PullingStates.Length; i++)
    {
      Debug.Log($"Pulling for: {PullingStates[i].Frames} Frames");
    }
  }

  void OnGUI()
  {
    GUI.color = Color.white;
    GUILayout.Label($"TravelingStates.Length = {TravelingStates.Length}");
    GUILayout.Label($"PullingStates.Length = {PullingStates.Length}");
    GUILayout.Label($"KilledTravelingIndices.Count = {KilledTravelingIndices.Count}");
    GUILayout.Label($"NewPullingStates.Count = {NewPullingStates.Count}");
  }
}

[BurstCompile]
public struct HS_HookshotState
{
}

[BurstCompile]
public struct HS_TravelingState
{
  public int Frames;
}

[BurstCompile]
public struct HS_PullingState
{
  public int Frames;
}

public struct HS_Hookshot : IJobParallelFor
{
  public NativeQueue<HS_TravelingState>.ParallelWriter NewTravelings;

  public void Execute(int index)
  {
    //
  }
}

[BurstCompile, Serializable]
public struct HS_TravelingJob : IJobParallelFor
{
  public NativeArray<HS_TravelingState> Travelings;
  public NativeQueue<HS_PullingState>.ParallelWriter NewPullings;
  public NativeQueue<int>.ParallelWriter KilledTravelingIndices;

  public void Execute(int index)
  {
    var traveling = Travelings[index];
    traveling.Frames++;
    Travelings[index] = traveling;
    if (traveling.Frames >= 25)
    {
      NewPullings.Enqueue(new HS_PullingState { Frames = 0 });
      KilledTravelingIndices.Enqueue(index);
    }
  }
}

[BurstCompile, Serializable]
public struct HS_PullingJob : IJobParallelFor
{
  public NativeArray<HS_PullingState> Pullings;

  public void Execute(int index)
  {
    var pulling = Pullings[index];
    pulling.Frames++;
    Pullings[index] = pulling;
  }
}