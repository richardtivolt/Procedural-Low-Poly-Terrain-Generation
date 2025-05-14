using UnityEngine;

[CreateAssetMenu(fileName = "Vegetation", menuName = "ScriptableObjects/Vegetation", order = 1)]
public class Vegetation : ScriptableObject
{
    public string Name;
    public GameObject Prefab;
    public Vector2 HeightBand;
    [Range(0f, 1f)] public float SpawnChance;
    public float RandomOffset;
}