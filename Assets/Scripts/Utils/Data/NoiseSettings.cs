using UnityEngine;

[CreateAssetMenu(fileName = "NoiseSettings", menuName = "ScriptableObjects/NoiseSettings", order = 1)]
public class NoiseSettings : ScriptableObject
{
    public float Scale;
    public Vector2 Offset;
    [Range(1, 8)] public int Octaves;
    public float Persistence;
    [Range(0f, 10f)] public float Lacunarity;
}