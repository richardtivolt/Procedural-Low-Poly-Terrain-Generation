using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [Header("Terrain Generation")]
    [SerializeField] private MeshFilter _meshFilter;
    [SerializeField] private TerrainSettings _terrainSettings;

    [Header("Environment Generation")]
    [SerializeField] private Transform _environmentParent;
    [SerializeField] private EnvironmentSettings _environmentSettings;

    public void CreateMap()
    {
        for (int i = _environmentParent.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(_environmentParent.GetChild(i).gameObject);
        }

        TerrainGenerator terrainGenerator = new TerrainGenerator(_terrainSettings);
        TerrainData terrainData = terrainGenerator.GenerateTerrainData();
        _meshFilter.sharedMesh = terrainGenerator.GenerateMesh(terrainData);

        EnvironmentGenerator environmentGenerator = new EnvironmentGenerator(_environmentSettings, terrainData);
        if (_environmentSettings.SpawnVegetation)
        {
            VegetationData[] vegetationDatas = environmentGenerator.GenerateVegetationDatas();
            foreach (VegetationData vegetationData in vegetationDatas)
            {
                GameObject prefab = Instantiate(vegetationData.Prefab, Vector3.zero, Quaternion.Euler(0, Random.Range(0f, 360f), 0), _environmentParent);
                prefab.transform.localScale *= Random.Range(0.9f, 1.1f);
                prefab.transform.localPosition = vegetationData.SpawnPoint;
            }
        }
        if (_environmentSettings.SpawnGrass)
        {
            foreach (Vegetation grassVegetation in _environmentSettings.GrassVegetations)
            {
                Vector3[] spawnPoints = environmentGenerator.GenerateGrassSpawnPoints(grassVegetation);
                foreach (Vector3 spawnPoint in spawnPoints)
                {
                    GameObject prefab = Instantiate(grassVegetation.Prefab, Vector3.zero, Quaternion.Euler(0, Random.Range(0f, 360f), 0), _environmentParent);
                    prefab.transform.localPosition = spawnPoint;
                }
            }
        }
    }
}