using UnityEngine;

[CreateAssetMenu(fileName = "Biome", menuName = "ScriptableObjects/Biome", order = 1)]
public class Biome : ScriptableObject
{
    public string Name;
    public float Height;
    public Gradient Gradient;
    public Color Edge;
}