using UnityEngine;

[CreateAssetMenu(fileName = "EnvironmentSettings", menuName = "ScriptableObjects/EnvironmentSettings", order = 1)]
public class EnvironmentSettings : ScriptableObject
{
    public bool SpawnVegetation;
    public Vector2 VegetationRegion;
    public Vegetation[] Vegetations;
    public bool SpawnGrass;
    public Vegetation[] GrassVegetations;
}