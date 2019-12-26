using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Chunk))]
public class ChunkEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        var t = (Chunk)target;

        if(GUILayout.Button("Recalculate Cubes"))
        {
            t.RecalculateCubes();
        }

        if(GUILayout.Button("Recalculate Mesh"))
        {
            t.RecalculateMesh();
        }
    }
}
