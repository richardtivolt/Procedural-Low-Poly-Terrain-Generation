using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapGenerator))]
public class MapGeneratorEditor : Editor
{
    private MapGenerator _mapGenerator;

    private void OnEnable()
    {
        _mapGenerator = (MapGenerator)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Space(10);
        if (GUILayout.Button("Create Map"))
        {
            _mapGenerator.CreateMap();
        }
    }
}