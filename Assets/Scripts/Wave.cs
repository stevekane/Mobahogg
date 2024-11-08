using System;
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

    RenderScattered(Turtles.LivingCreeps, Settings.CreepMesh, Settings.CreepMaterialTurtles, ChunkOffset(BattleFrontIndex+1), Width, Seed+1);
    RenderScattered(Robots.LivingCreeps, Settings.CreepMesh, Settings.CreepMaterialRobots, ChunkOffset(BattleFrontIndex-1), Width, Seed-1);
  }

  /*

  */
  Vector3 ChunkOffset(int index) {
    return new Vector3(Width*index-.5f*Width, 0, -.5f*Height);
  }

  void RenderScattered(
  int count,
  Mesh mesh,
  Material material,
  Vector3 Offset,
  float width,
  int seed) {
    var maxIndex = Width*Height-1;
    var randomGenerator = new System.Random(seed);
    for (var i = 0; i < count; i++) {
      var index = randomGenerator.Next(0, maxIndex);
      var z = index / width;
      var x = index % width;
      var position = new Vector3(x, 0, z);
      var matrix = Matrix4x4.TRS(Offset+position, Quaternion.identity, Vector3.one);
      Graphics.DrawMesh(mesh, matrix, material, 0);
    }
  }
}