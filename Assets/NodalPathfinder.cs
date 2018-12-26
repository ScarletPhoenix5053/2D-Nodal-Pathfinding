using System;
using System.Collections.Generic;
using UnityEngine;

namespace Sierra.Pathfinding
{
    public class NodalPathfinder : MonoBehaviour
    {
        public Vector2 Position = Vector2.zero;
        public Vector2 Size = new Vector2(20f, 20f);
        public FieldShape Shape = FieldShape.Square;
        public float NodeSpaceX = 1f;
        public float NodeSpaceY = 1f;
        public LayerMask ObstructiveLayers;
        public bool ShowNodeConnections;
        public bool ShowInvalidNodeConnections;

        private NodeMesh _nodeMesh;
        private Vector2 _startPos { get { return new Vector2(Position.x - Size.x / 2, Position.y - Size.y / 2); } }


        private void Awake()
        {
            GenerateNodeMesh();
        }
        private void OnDrawGizmos()
        {
            DrawPathfindingArea();
            DrawNodeMeshOrigin();
            DrawNodes();
            DrawNodeConnections();
        }

        public void GenerateNodeMesh()
        {
            _nodeMesh = new NodeMesh(_startPos, Size, NodeSpaceX, NodeSpaceY, Shape);
            _nodeMesh.ValidateNodes(GetCollidersObsturctingNodeMesh());
        }
        public Path GetPathTo(Vector2 destination)
        {
            throw new NotImplementedException();
        }

        private Collider[] GetCollidersObsturctingNodeMesh()
        {
            return Physics.OverlapBox(Position, new Vector3(Size.x / 2, Size.y / 2, 1), transform.rotation, ObstructiveLayers);
        }
        private void DrawPathfindingArea()
        {
            Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.2f);
            Gizmos.DrawCube(Position, Size);
        }
        private void DrawNodeMeshOrigin()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(_startPos, 0.2f);
        }
        private void DrawNodes()
        {
            // Nullcheck
            if (_nodeMesh == null) return;

            // Draw all nodes
            foreach (Node[] nodes in _nodeMesh.Nodes)
            {
                foreach (Node node in nodes)
                {
                    if (node.Valid) Gizmos.color = Color.green;
                    else Gizmos.color = Color.red;
                    Gizmos.DrawSphere(new Vector2(node.X, node.Y), 0.1f);
                }
            }
        }
        private void DrawNodeConnections()
        {
            // Get list of connections

            // Check if connection is valid

            // Draw connections, with colour dependant on validity
        }
    }
    public class NodeMesh
    {
        // Jagged array to store nodes
        public Node[][] Nodes;
        public FieldShape Shape = FieldShape.Square;

        public NodeMesh(Vector2 origin, Vector2 size, float xSpacing, float ySpacing, FieldShape fieldShape = FieldShape.Square)
        {
            var row = new List<Node[]>();
            var col = new List<Node>();

            switch (fieldShape)
            {               
                case FieldShape.Square:
                    Shape = FieldShape.Square;
                    // Instantiate all nodes in area
                    row = new List<Node[]>();
                    for (float x = origin.x; x <= origin.x + size.x; x += xSpacing)
                    {
                        // Instantiate collumn by collumn
                        col = new List<Node>();
                        for (float y = origin.y; y <= origin.y + size.y; y += ySpacing)
                        {
                            col.Add(new Node(x, y));
                        }
                        // Convert list to array & store
                        row.Add(col.ToArray());
                    }
                    // Convert list of arrays to jagged array & store
                    Nodes = row.ToArray();
                    break;

                case FieldShape.Diamond:
                    Shape = FieldShape.Diamond;
                    // Instantiate all nodes in area
                    row = new List<Node[]>();
                    for (float x = origin.x; x <= origin.x + size.x; x += xSpacing)
                    {
                        // Instantiate collumns of alternating step. (origin, +1/2, etc.)
                        col = new List<Node>();
                        for (float y = origin.y; y <= origin.y + size.y; y += ySpacing)
                        {
                            col.Add(new Node(x, y));
                        }
                        // Convert list to array & stores
                        row.Add(col.ToArray());

                        // Repeat if alternating row does not exceet bounds
                        if (x + xSpacing / 2 <= origin.x + size.x)
                        {
                            col = new List<Node>();
                            for (float y = origin.y + ySpacing / 2; y <= origin.y + size.y; y += ySpacing)
                            {
                                col.Add(new Node(x + xSpacing / 2, y));
                            }
                            row.Add(col.ToArray());
                        }
                    }
                    // Convert list of arrays to jagged array & store
                    Nodes = row.ToArray();
                    break;
            }
        }

        public void ValidateNodes(Collider[] colliders)
        {
            // for each collider that could overlap a node
            foreach (Collider collider in colliders)
            {
                foreach (Node[] nodeArray in Nodes)
                {
                    foreach (Node node in nodeArray)
                    {
                        // Check if a node overlaps it, and mark invalid if it does.
                        if (collider.bounds.Contains(new Vector2(node.X, node.Y)))
                            node.Valid = false;
                    }
                }
            }
        }
        public void GenerateNodeConnections()
        {
            // For each collumn in row
            for (int x = 0; x < Nodes.Length; x++)
            {
                // For each node in collum
                for (int y = 0; y < Nodes[x].Length; y++)
                {
                    // Assign nearby nodes as connections
                    // x/y values cannot go below 0 or above array size!

                    
                }
            }
        }
    }
    public class Node
    {
        public float X;
        public float Y;
        public bool Valid;
        public Node[] ConnectedNodes;

        /// <summary>
        /// Spawns an invalid node at 0,0
        /// </summary>
        public Node()
        {
            X = 0;
            Y = 0;
            Valid = false;
        }
        /// <summary>
        /// Spawns a vaid node at the specified position
        /// </summary>
        /// <param name="xPos"></param>
        /// <param name="yPos"></param>
        public Node(float xPos, float yPos)
        {
            X = xPos;
            Y = yPos;
            Valid = true;
        }
    }
    public struct NodeConnection
    {
        public Node A;
        public Node B;
        public bool Valid { get { return A.Valid && B.Valid; } }

        public NodeConnection(Node a, Node b)
        {
            A = a;
            B = b;
        }
    }
    public class Path
    {
        public List<Vector2> Coordinates = new List<Vector2>();
    }
    public enum FieldShape { Diamond, Square }
}