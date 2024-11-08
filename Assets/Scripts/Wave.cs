using System;
using UnityEngine;

[ExecuteInEditMode]
public class Wave : MonoBehaviour {
  /*
  This is a visual aid to help design the game and layout the map.
  The purpose of this system isn't to really function but rather to
  make it simple to visualize game states, layouts, etc.

  There are three facilities of this system:

    Display all possible battlefront locations
    Display creeps for each team surrounding the current battlefront location
    Color each battlefront location based on the team that owns it

    Display resources for all creeps that are "dead" in a given wave.
  */

  [Serializable]
  public class Team {
    public Material Material;
    public int TotalCreeps = 10;
    public int LivingCreeps = 7;
  }

  public Material DMZMaterial;
  public Team Turtles = new();
  public Team Robots = new();
  [Range(-32,32)]
  public int BattleFrontIndex;

  public Mesh ChunkMesh;
  [Range(0, 100)]
  [Tooltip("Number of battlefront locations on either side of the center")]
  public int Count = 5;
  [Tooltip("Horizontal size of whole battlefront")]
  [Range(0, 100)]
  public int Width = 30;
  [Range(0, 100)]
  [Tooltip("Vertical size of whole battlefront")]
  public int Height = 10;
  [Range(0, 1)]
  [Tooltip("Visual Space between battlefront locations")]
  public float Gap = .25f;

  void Update() {
    var count = 2*Count+1;
    var width = (float)Width;
    var dp = width / count * Vector3.right;
    var position = new Vector3(-width / 2, 0, 0) + dp / 2;
    var scale = new Vector3(width / count - Gap, .05f, Height);
    for (var i = -Count; i <= Count; i++) {
      var material = BattleFrontIndex < i
        ? Turtles.Material
        : BattleFrontIndex > i
          ? Robots.Material
          : DMZMaterial;
      var matrix = Matrix4x4.TRS(position, Quaternion.identity, scale);
      Graphics.DrawMesh(ChunkMesh, matrix, material, 0);
      position += dp;
    }
  }
}