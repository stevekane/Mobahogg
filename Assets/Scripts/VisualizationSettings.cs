using UnityEngine;

[CreateAssetMenu(fileName = "VisualizationSettings", menuName = "Scriptable Objects/VisualizationSettings")]
public class VisualizationSettings : ScriptableObject {
  [Header("Tokens")]
  public Mesh TokenMesh;
  public Material TokenMaterialNeutral;
  public Material TokenMaterialTurtles;
  public Material TokenMaterialRobots;

  [Header("Creeps")]
  public Mesh CreepMesh;
  public Material CreepMaterialTurtles;
  public Material CreepMaterialRobots;

  [Header("Wave")]
  public Mesh WaveChunkMesh;
  public Material WaveMaterialTurtles;
  public Material WaveMaterialRobots;
  public Material WaveMaterialDMZ;
}