using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapGenerator))]
public class MapGenerationInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        MapGenerator generator = (MapGenerator)target;

        if (GUILayout.Button("Clear Map"))
        {
            generator.ClearMap();
        }

        if (GUILayout.Button("Generate Map"))
        {
            ThreadStart threadStart = delegate
            {
                lock (generator)
                {
                    generator.GenerateMap();
                }
            };
            threadStart.Invoke();
        }
    }
}
