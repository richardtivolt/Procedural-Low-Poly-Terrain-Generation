using UnityEngine;

[CreateAssetMenu(fileName = "TerrainSettings", menuName = "ScriptableObjects/TerrainSettings", order = 1)]
public class TerrainSettings : ScriptableObject
{
    public int Seed;

    [Header("Poisson Disc Sampling")]
    public float PointRadius;
    public Vector2 SampleRegion;

    [Header("Elevation Settings")]
    public NoiseSettings NoiseSettings;
    public float TerraceHeight;
    public Vector2 FallOffRegion;
    public int ExtensionStepCount;
    public float DetailOffset;

    [Header("Terrain Colors")]
    public Biome[] Biomes;
}