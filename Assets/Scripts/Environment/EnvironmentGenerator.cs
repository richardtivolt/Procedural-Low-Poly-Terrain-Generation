using System.Collections.Generic;
using UnityEngine;

public class EnvironmentGenerator
{
    private Vector2 _vegetationRegion;
    private Vegetation[] _vegetations;

    private TerrainData _terrainData;

    public EnvironmentGenerator(EnvironmentSettings environmentSettings, TerrainData terrainData)
    {
        _vegetationRegion = environmentSettings.VegetationRegion;
        _vegetations = environmentSettings.Vegetations;

        _terrainData = terrainData;
    }

    public VegetationData[] GenerateVegetationDatas()
    {
        HashSet<Vertex> invalidVertexSet = new HashSet<Vertex>();
        List<VegetationData> vegetationDatas = new List<VegetationData>();
        foreach (Vertex vertex in _terrainData.Vertices)
        {
            if (vertex.Noise < _vegetationRegion.x || vertex.Noise > _vegetationRegion.y)
            {
                continue;
            }

            Vegetation vegetation = GetRandomVegetation(vertex.Noise);
            if (vegetation == null)
            {
                continue;
            }
            if (!invalidVertexSet.Contains(vertex) && vertex.OnSurface() && Random.Range(0f, 1f) < vegetation.SpawnChance)
            {
                Vector2 randomOffset = Random.insideUnitCircle * Random.Range(0f, vegetation.RandomOffset);
                VegetationData vegetationData = new VegetationData(vegetation.Prefab, vertex.ToVector3() + new Vector3(randomOffset.x, 0, randomOffset.y));
                vegetationDatas.Add(vegetationData);

                List<Vertex> neighbours = new List<Vertex>();
                GetVertexNeighbours(vertex, neighbours, 2);
                foreach (Vertex neighbour in neighbours)
                {
                    if (!invalidVertexSet.Contains(neighbour))
                    {
                        invalidVertexSet.Add(neighbour);
                    }
                }
            }
        }
        return vegetationDatas.ToArray();
    }

    public Vector3[] GenerateGrassSpawnPoints(Vegetation grassVegetation)
    {
        List<Vector3> spawnPoints = new List<Vector3>();
        foreach (Vertex vertex in _terrainData.Vertices)
        {
            if (vertex.Noise < grassVegetation.HeightBand.x || vertex.Noise > grassVegetation.HeightBand.y)
            {
                continue;
            }
            if (Random.Range(0f, 1f) < grassVegetation.SpawnChance && vertex.OnSurface())
            {
                Vector2 randomOffset = Random.insideUnitCircle * Random.Range(0f, grassVegetation.RandomOffset);
                spawnPoints.Add(vertex.ToVector3() + new Vector3(randomOffset.x, 0, randomOffset.y));
            }
        }
        return spawnPoints.ToArray();
    }

    private Vegetation GetRandomVegetation(float clusterNoise)
    {
        List<Vegetation> vegetations = new List<Vegetation>();
        foreach (Vegetation vegetation in _vegetations)
        {
            if (clusterNoise >= vegetation.HeightBand.x && clusterNoise <= vegetation.HeightBand.y)
            {
                vegetations.Add(vegetation);
            }
        }
        return vegetations.Count == 0 ? null : vegetations[Random.Range(0, vegetations.Count)];
    }

    private void GetVertexNeighbours(Vertex vertex, List<Vertex> neighbours, int depth)
    {
        if (depth == 0)
        {
            return;
        }
        foreach (Vertex neighbour in vertex.Neighbours)
        {
            if (neighbours.Contains(neighbour))
            {
                continue;
            }
            neighbours.Add(vertex);
            GetVertexNeighbours(neighbour, neighbours, depth - 1);
        }
    }
}