using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Wave : MonoBehaviour {
  [Serializable]
  public class Team {
    public int LivingCreeps = 7;
    public int DeadCreeps = 3;
  }

  public VisualizationSettings Settings;
  public Team Turtles = new();
  public Team Robots = new();
  public int Seed;
  [Range(-32,32)]
  public int BattleFrontIndex;

  public Mesh ChunkMesh;
  [Range(0, 100)]
  public int ChunksPerTeam = 5;
  [Range(0, 1)]
  public float Gap = .25f;

  [Header("Battlefield Chunks")]
  [Range(1, 64)]
  public int Width = 30;
  [Range(1, 64)]
  public int Height = 10;
  [Range(0,32)]
  public int Count = 5;

  List<int> TurtleIndices = new(256);
  List<int> RobotIndices = new(256);

  void Update() {
    var count = 2*Count+1;
    var totalWidth = count*Width;
    var dp = Width*Vector3.right;
    var position = new Vector3(-totalWidth/2f, 0, 0)+dp/2;
    var scale = new Vector3(Width-Gap, .05f, Height);
    for (var i = -Count; i <= Count; i++) {
      var material = BattleFrontIndex < i
        ? Settings.WaveMaterialTurtles
        : BattleFrontIndex > i
          ? Settings.WaveMaterialRobots
          : Settings.WaveMaterialDMZ;
      var matrix = Matrix4x4.TRS(position, Quaternion.identity, scale);
      Graphics.DrawMesh(ChunkMesh, matrix, material, 0);
      position += dp;
    }

    RenderCreeps(Turtles, TurtleIndices, BattleFrontIndex+1, Settings.CreepMesh, Settings.CreepMaterialTurtles, Settings.TokenMesh, Settings.TokenMaterialTurtles, Seed+1);
    RenderCreeps(Robots, RobotIndices, BattleFrontIndex-1, Settings.CreepMesh, Settings.CreepMaterialRobots, Settings.TokenMesh, Settings.TokenMaterialRobots, Seed-1);
  }

  // TODO: Turtles are hard-coded here
  void RenderCreeps(
  Team team,
  List<int> indices,
  int chunkIndex,
  Mesh creepMesh,
  Material creepMaterial,
  Mesh tokenMesh,
  Material tokenMaterial,
  int seed) {
    var offset = ChunkOffset(chunkIndex);
    Shuffle(indices, Width*Height, seed);
    for (var i = 0; i < team.DeadCreeps; i++)
      RenderScattered(indices[i], tokenMesh, tokenMaterial, offset, Width);
    for (var i = 0; i < team.LivingCreeps; i++)
      RenderScattered(indices[i+team.DeadCreeps], creepMesh, creepMaterial, offset, Width);
  }

  // In-place operation
  void Shuffle(List<int> xs, int count, int seed) {
    var gen = new System.Random(seed);
    xs.Clear();
    for (var i = 0; i < count; i++)
      xs.Add(i);
    for (var i = 0; i < count; i++)  {
      int randomIndex = gen.Next(0, count);
      int temp = xs[i];
      xs[i] = xs[randomIndex];
      xs[randomIndex] = temp;
    }
  }

  Vector3 ChunkOffset(float index) {
    return new Vector3((index-.5f)*Width+.5f, 0, -Height/2f+.5f);
  }

  void RenderScattered(int index, Mesh mesh, Material material, Vector3 Offset, float width) {
    var z = index / (int)width;
    var x = index % width;
    var position = Offset+new Vector3(x, 0, z);
    var matrix = Matrix4x4.TRS(position, Quaternion.identity, Vector3.one);
    Graphics.DrawMesh(mesh, matrix, material, 0);
  }
}