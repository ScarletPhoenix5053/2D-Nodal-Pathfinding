using UnityEditor;
using UnityEngine;

namespace Sierra.Pathfinding
{
    [CustomEditor(typeof(NodalPathfinder))]
    public class NodalPathfinderEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var nodalPathfinder = (NodalPathfinder)target;

            if (GUILayout.Button("Generate new NodeMesh")) nodalPathfinder.GenerateNodeMesh();
            if (GUILayout.Button("Test NodeConnection Equality")) nodalPathfinder.TestNodeConnectionEquality();
        }
    }
}