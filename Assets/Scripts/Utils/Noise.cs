using UnityEngine;

public class Noise
{
    private float _scale;
    private Vector2 _offset;
    private int _octaves;
    private float _persistence;
    private float _lacunarity;

    private Vector2 _noiseBoundaries;
    private Vector2 _randomOffset;

    public Noise(NoiseSettings noiseSettings, int seed)
    {
        _scale = noiseSettings.Scale;
        _offset = noiseSettings.Offset;
        _octaves = noiseSettings.Octaves;
        _persistence = noiseSettings.Persistence;
        _lacunarity = noiseSettings.Lacunarity;

        _noiseBoundaries = new Vector2(float.MaxValue, float.MinValue);
        System.Random random = new System.Random(seed);
        _randomOffset = new Vector2((float)(random.NextDouble() - 0.5) * 10000f, (float)(random.NextDouble() - 0.5) * 10000f);
    }

    public float GetHeight(Vector2 point)
    {
        float height = 0f;

        float amplitude = 1f;
        float frequency = 1f;
        for (int i = 0; i < _octaves; i++)
        {
            float sampleX = point.x / _scale * frequency + _offset.x + _randomOffset.x;
            float sampleY = point.y / _scale * frequency + _offset.y + _randomOffset.y;

            float noise = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
            height += noise * amplitude;

            amplitude *= _persistence;
            frequency *= _lacunarity;
        }

        if (height > _noiseBoundaries.y)
        {
            _noiseBoundaries.y = height;
        }
        else if (height < _noiseBoundaries.x)
        {
            _noiseBoundaries.x = height;
        }
        return height;
    }

    public float[] GetHeights(Vector2[] points)
    {
        float[] heights = new float[points.Length];
        for (int i = 0; i < points.Length; i++)
        {
            heights[i] = GetHeight(points[i]);
        }
        return heights;
    }

    public void NormalizeHeights(float[] heights)
    {
        for (int i = 0; i < heights.Length; i++)
        {
            heights[i] = Mathf.InverseLerp(_noiseBoundaries.x, _noiseBoundaries.y, heights[i]);
        }
    }

    public void AddFallOff(Vector2[] points, float[] heights, Vector2 sampleRegion, Vector2 fallOffRegion)
    {
        for (int i = 0; i < points.Length; i++)
        {
            float distance = Mathf.Max(
                Mathf.Abs(points[i].x) / (sampleRegion.x / 2),
                Mathf.Abs(points[i].y) / (sampleRegion.y / 2)
            );
            if (distance >= fallOffRegion.x && distance <= fallOffRegion.y)
            {
                float lerpTime = Mathf.InverseLerp(fallOffRegion.x, fallOffRegion.y, distance);
                heights[i] = Mathf.Lerp(heights[i], 0, lerpTime);
            }
            else if (distance > fallOffRegion.y)
            {
                heights[i] = 0;
            }
        }
    }
}