using UnityEngine;

public struct VegetationData
{
    public GameObject Prefab;
    public Vector3 SpawnPoint;

    public VegetationData(GameObject prefab, Vector3 spawnPoint)
    {
        Prefab = prefab;
        SpawnPoint = spawnPoint;
    }
}