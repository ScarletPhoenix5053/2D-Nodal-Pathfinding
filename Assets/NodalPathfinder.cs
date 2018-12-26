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
            _nodeMesh.GenerateNodeConnections(); 
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
            if (Shape == FieldShape.Diamond)
                throw new NotImplementedException("Diamond connection mesh not implimented yet");

            // For each collumn in row
            for (int x = 0; x < Nodes.Length; x++)
            {
                // For each node in collum
                for (int y = 0; y < Nodes[x].Length; y++)
                {
                    // Assign nearby nodes as connections
                    // x/y values cannot go below 0 or above array size!

                    var node = Nodes[x][y];
                    // Check if at left or bottom edge
                    if (y == 0)
                    {
                        if (x == 0)
                        {
                            // at origin (0,0)
                            node.ConnectedNodes = new Node[]
                            {
                                // Above
                                Nodes[x][y+1],
                                Nodes[x+1][y+1],

                                // To Side
                                Nodes[x+1][y]
                            };
                        }
                        else
                        {
                            // at bottom of collumn (x,0)
                            node.ConnectedNodes = new Node[]
                            {
                                // Above
                                Nodes[x-1][y+1],
                                Nodes[x][y+1],
                                Nodes[x+1][y+1],

                                // To side
                                Nodes[x-1][y],
                                Nodes[x+1][y],
                            };
                        }
                    }
                    else if (x == 0)
                    {
                        // in first collumn (0,y)
                        node.ConnectedNodes = new Node[]
                        {
                            // Above
                            Nodes[x][y+1],
                            Nodes[x+1][y+1],

                            // To Side
                            Nodes[x+1][y],

                            // Below
                            Nodes[x][y-1],
                            Nodes[x+1][y-1]
                        };
                    }
                    // Check if at right or top edge
                    else if (y == Nodes[x].Length - 1)
                    {
                        if (x == Nodes.Length - 1)
                        {
                            // At top right corner (L-1,L-1)
                        node.ConnectedNodes = new Node[]
                        {
                            // To Side
                            Nodes[x-1][y],

                            // Below
                            Nodes[x-1][y-1],
                            Nodes[x][y-1]
                        };
                        }
                        else
                        {
                            // At top edge (x,L-1)  
                            node.ConnectedNodes = new Node[]
                            {
                            // To Side
                            Nodes[x-1][y],
                            Nodes[x+1][y],

                            // Below
                            Nodes[x-1][y-1],
                            Nodes[x][y-1],
                            Nodes[x+1][y-1]
                            };
                        }
                    }
                    else if (x == Nodes.Length - 1)
                    {
                        // At right edge (L-1,y)
                        node.ConnectedNodes = new Node[]
                        {
                            // Above
                            Nodes[x-1][y],
                            Nodes[x][y],

                            // To Side
                            Nodes[x-1][y],

                            // Below
                            Nodes[x-1][y-1],
                            Nodes[x][y-1],
                        };
                    }
                    // Not at any edge!
                    else
                    {
                        node.ConnectedNodes = new Node[]
                        {
                            // Above
                            Nodes[x-1][y],
                            Nodes[x][y],
                            Nodes[x+1][y],

                            // To Side
                            Nodes[x-1][y],
                            Nodes[x+1][y],

                            // Below
                            Nodes[x-1][y-1],
                            Nodes[x][y-1],
                            Nodes[x+1][y-1],
                        };
                    }
                    var connectedString = " found: " + node.ConnectedNodes.Length;
                    for (int i = 0; i < node.ConnectedNodes.Length; i++)
                    {
                        int n = i + 1;
                        connectedString += ". " + n + ": "
                            + node.ConnectedNodes[i].X + ","
                            + node.ConnectedNodes[i].Y;
                    }
                    Debug.Log("Node connections for " + node.X + "," + node.Y + connectedString);
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