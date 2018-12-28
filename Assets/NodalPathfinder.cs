using System;
using System.Collections.Generic;
using System.Linq;
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
        private NodeConnection[] _nodeConnections;
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
            _nodeMesh.AssignNodeConnections();
            _nodeConnections = _nodeMesh.GetNodeConnections();
            _nodeMesh.InvalidateBadNodes();
            //_nodeMesh.DebugNode(7,5);
        }
        /// <summary>
        /// Debugging method for testing value equality between two <see cref="NodeConnection"/> objects.
        /// </summary>
        public void TestNodeConnectionEquality()
        {
            var nodeA = new Node(45, 23);
            var nodeB = new Node(23, 67);
            var nodeC = new Node(45, 23);

            NodeConnection connectionN = null;
            var connectionAB = new NodeConnection(nodeA, nodeB);
            var connectionAC = new NodeConnection(nodeA, nodeC);
            var connectionBC = new NodeConnection(nodeA, nodeC);
            var connectionABDuplicate = new NodeConnection(nodeA, nodeB);
            var connectionABInverse = new NodeConnection(nodeA, nodeB);

            Debug.Log("c.AB-AB| Expecting T, Got: " + (connectionAB == connectionAB));
            Debug.Log("c.AB-AC| Expecting F, Got: " + (connectionAB == connectionAC));
            Debug.Log("c.BC-AB| Expecting F, Got: " + (connectionBC == connectionAB));
            Debug.Log("c.AB-AB(d)| Expecting T, Got: " + (connectionAB == connectionABDuplicate));
            Debug.Log("c.AB-AB(i)| Expecting T, Got: " + (connectionAB == connectionABInverse));
            Debug.Log("c.N-AC)| Expecting F, Got: " + (connectionAC == connectionN));
            Debug.Log("c.N-Null)| Expecting T, Got: " + (null == connectionN));
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
            if (_nodeConnections == null) return;
            if (!ShowNodeConnections) return;

            // for each nodeConnection
            foreach (NodeConnection connection in _nodeConnections)
            {
                if (connection.Valid)
                {
                    // draw connection as green
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(
                        new Vector2(connection.A.X, connection.A.Y),
                        new Vector2(connection.B.X, connection.B.Y));
                }
                else
                {
                    if (ShowInvalidNodeConnections)
                    {
                        // draw connection as red
                        Gizmos.color = Color.red;
                        Gizmos.DrawLine(
                            new Vector2(connection.A.X, connection.A.Y),
                            new Vector2(connection.B.X, connection.B.Y));
                    }
                }
            }
        }
    }
    public class NodeMesh
    {
        // Jagged array to store nodes
        public Node[][] Nodes;
        public FieldShape Shape = FieldShape.Square;
        public bool DebugNodeConnections = false;

        private Node c_Node;
        private int c_X;
        private int c_Y;

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
        private enum Pos
        {
            Centre,
            BottomLeftCorner, BottomRightCorner, TopLeftCorner, TopRightCorner,
            LeftEdge, RightEdge, BottomEdge, TopEdge
        }
        private enum InDirection
        {
            AboveLeft, Above, AboveRight, Left, Right, BelowLeft, Below, BelowRight
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
        /// <summary>
        /// Checks that each <see cref="Node"/> is connected to a minimum of three nodes, where <see cref="Node.Valid"/> is <see cref="true"/>. 
        /// </summary>
        public void InvalidateBadNodes()
        {
            foreach (Node[] nodeArray in Nodes)
            {
                foreach (Node node in nodeArray)
                {
                    var validNodeCount = 0;
                    Debug.Log("valid node");
                    Debug.Log("pre" +validNodeCount);
                    foreach (Node connectedNode in node.ConnectedNodes)
                    {
                        if (connectedNode.Valid) validNodeCount++;
                    }
                    Debug.Log("post"+validNodeCount);
                    if (validNodeCount < 3)
                    {
                        Debug.Log("hi");
                        node.Valid = false;
                    }
                }
            }
            
        }
        /// <summary>
        /// Fills out <see cref="Node.ConnectedNodes"/> in each node in the nodemesh. Fails if mesh is too small or does not exist.
        /// </summary>
        public void AssignNodeConnections()
        {
            if (Nodes == null) throw new NullReferenceException(
                "Nodes array in NodeMesh is unnasigned! " +
                "Please generate nodemesh before attempting to assign connections.");
            if (Nodes.Length <= 3 || Nodes[0].Length <= 3) throw new UnityException(
                "Please ensure nodemesh contains more than 3 nodes along each axis");

            switch (Shape)
            {
                case FieldShape.Square:                    
                    for (int x = 0; x < Nodes.Length; x++)
                    {
                        for (int y = 0; y < Nodes[x].Length; y++)
                        {
                            c_Node = Nodes[x][y];
                            c_X = x;
                            c_Y = y;

                            if (At(Pos.BottomEdge) && At(Pos.LeftEdge)) AssignNodes(Pos.BottomLeftCorner);
                            else if (At(Pos.TopEdge) && At(Pos.LeftEdge)) AssignNodes(Pos.TopLeftCorner);
                            else if (At(Pos.TopEdge) && At(Pos.RightEdge)) AssignNodes(Pos.TopRightCorner);
                            else if (At(Pos.BottomEdge) && At(Pos.RightEdge)) AssignNodes(Pos.BottomRightCorner);

                            else if (At(Pos.LeftEdge)) AssignNodes(Pos.LeftEdge);
                            else if (At(Pos.TopEdge)) AssignNodes(Pos.TopEdge);
                            else if (At(Pos.RightEdge)) AssignNodes(Pos.RightEdge);
                            else if (At(Pos.BottomEdge)) AssignNodes(Pos.BottomEdge);

                            else AssignNodes(Pos.Centre);

                            if (DebugNodeConnections) LogNodeConnections(c_Node);
                        }
                    }
                    break;

                default:
                    throw new NotImplementedException("Connections for node mesh shape " + Shape + " are not yet implimented");
            }           
        }
        /// <summary>
        /// Creates one <see cref="NodeConnection"/> for each unique connection. This method takes a while to run so use it sparingly.
        /// </summary>
        public NodeConnection[] GetNodeConnections()
        {
            var connections = new List<NodeConnection>();

            // for each collumn of nodes
            for (int x = 0; x < Nodes.Length; x++)
            {
                // for each node
                for (int y = 0; y < Nodes[x].Length; y++)
                {
                    // for each connected node
                    for (int n = 0; n < Nodes[x][y].ConnectedNodes.Length; n++)
                    {
                        // generate  connection
                        var newConnection = new NodeConnection(Nodes[x][y], Nodes[x][y].ConnectedNodes[n]);
                        var isDuplicate = false;

                        // check if first connection
                        if (!connections.Any())
                        {
                            connections.Add(newConnection);
                        }
                        else
                        {
                            // check if duplicate connection
                            for (int o = 0; o < connections.Count; o++)
                            {
                                var oldConnection = connections[o];
                                if (oldConnection.Equals(newConnection))
                                {
                                    isDuplicate = true;
                                    break; 
                                }
                            }

                            // add to list if not duplicate connection
                            if (!isDuplicate)
                            {
                                connections.Add(newConnection);
                            }
                        }
                    }
                }
            }
            // return list as array
            return connections.ToArray();
        }

        public void DebugNode(int x, int y)
        {
            var node = Nodes[x][y];
            var validNodes = 0;
            Debug.Log("Connectd nodes for node at " + x + "," + y + ": " + node.ConnectedNodes.Length);
            foreach (Node connected in node.ConnectedNodes)
            {
                Debug.Log(connected.X + "," + connected.Y + " " + connected.Valid);
                if (connected.Valid) validNodes++;
            }
            Debug.Log(validNodes);
            if (validNodes < 3)
            {
                Debug.Log("bad node!");
                node.Valid = false;
            }
        }
        private Node GetNode(int x, int y, InDirection pos)
        {
            switch (pos)
            {
                case InDirection.AboveLeft:
                    return Nodes[x - 1][y + 1];
                case InDirection.Above:
                    return Nodes[x][y + 1];
                case InDirection.AboveRight:
                    return Nodes[x + 1][y + 1];
                case InDirection.Left:
                    return Nodes[x - 1][y];
                case InDirection.Right:
                    return Nodes[x + 1][y];
                case InDirection.BelowLeft:
                    return Nodes[x - 1][y - 1];
                case InDirection.Below:
                    return Nodes[x][y - 1];
                case InDirection.BelowRight:
                    return Nodes[x + 1][y - 1];
                default:
                    throw new NullReferenceException("The position requested does not exist!");
            }
        }
        private bool At(Pos edge)
        {
            switch (edge)
            {
                case Pos.LeftEdge:
                    return c_X == 0;
                case Pos.RightEdge:
                    return c_X == Nodes.Length - 1;
                case Pos.BottomEdge:
                    return c_Y == 0;
                case Pos.TopEdge:
                    return c_Y == Nodes[c_X].Length - 1;
                default:
                    throw new NullReferenceException("Please use an EDGE enumeration of enum AtMesh, such as LeftEdge or TopEdge.");
            }
        }

        private void AssignNodes(Pos atMesh)
        {
            switch (atMesh)
            {
                case Pos.Centre:
                    c_Node.ConnectedNodes = new Node[]
                    {
                        GetNode(c_X,c_Y,InDirection.AboveLeft),
                        GetNode(c_X,c_Y,InDirection.Above),
                        GetNode(c_X,c_Y,InDirection.AboveRight),
                        GetNode(c_X,c_Y,InDirection.Left),
                        GetNode(c_X,c_Y,InDirection.Right),
                        GetNode(c_X,c_Y,InDirection.BelowLeft),
                        GetNode(c_X,c_Y,InDirection.Below),
                        GetNode(c_X,c_Y,InDirection.BelowRight)
                    };
                    break;

                case Pos.BottomLeftCorner:
                    c_Node.ConnectedNodes = new Node[]
                    {
                        GetNode(c_X,c_Y,InDirection.Above),
                        GetNode(c_X,c_Y,InDirection.AboveRight),
                        GetNode(c_X,c_Y,InDirection.Right)
                    };
                    break;

                case Pos.BottomRightCorner:
                    c_Node.ConnectedNodes = new Node[]
                    {
                        GetNode(c_X,c_Y,InDirection.AboveLeft),
                        GetNode(c_X,c_Y,InDirection.Above),
                        GetNode(c_X,c_Y,InDirection.Left)
                    };
                    break;

                case Pos.TopLeftCorner:
                    c_Node.ConnectedNodes = new Node[]
                    {
                        GetNode(c_X,c_Y,InDirection.Right),
                        GetNode(c_X,c_Y,InDirection.Below),
                        GetNode(c_X,c_Y,InDirection.BelowRight)
                    };
                    break;

                case Pos.TopRightCorner:
                    c_Node.ConnectedNodes = new Node[]
                    {
                        GetNode(c_X,c_Y,InDirection.Left),
                        GetNode(c_X,c_Y,InDirection.BelowLeft),
                        GetNode(c_X,c_Y,InDirection.Below)
                    };
                    break;
                case Pos.LeftEdge:
                    c_Node.ConnectedNodes = new Node[]
                    {
                        GetNode(c_X,c_Y,InDirection.Above),
                        GetNode(c_X,c_Y,InDirection.AboveRight),
                        GetNode(c_X,c_Y,InDirection.Right),
                        GetNode(c_X,c_Y,InDirection.Below),
                        GetNode(c_X,c_Y,InDirection.BelowRight)
                    };
                    break;
                case Pos.RightEdge:
                    c_Node.ConnectedNodes = new Node[]
                    {
                        GetNode(c_X,c_Y,InDirection.AboveLeft),
                        GetNode(c_X,c_Y,InDirection.Above),
                        GetNode(c_X,c_Y,InDirection.Left),
                        GetNode(c_X,c_Y,InDirection.BelowLeft),
                        GetNode(c_X,c_Y,InDirection.Below)
                    };
                    break;
                case Pos.BottomEdge:
                    c_Node.ConnectedNodes = new Node[]
                    {
                        GetNode(c_X,c_Y,InDirection.AboveLeft),
                        GetNode(c_X,c_Y,InDirection.Above),
                        GetNode(c_X,c_Y,InDirection.AboveRight),
                        GetNode(c_X,c_Y,InDirection.Left),
                        GetNode(c_X,c_Y,InDirection.Right)
                    };
                    break;
                case Pos.TopEdge:
                    c_Node.ConnectedNodes = new Node[]
                    {
                        GetNode(c_X,c_Y,InDirection.Left),
                        GetNode(c_X,c_Y,InDirection.Right),
                        GetNode(c_X,c_Y,InDirection.BelowLeft),
                        GetNode(c_X,c_Y,InDirection.Below),
                        GetNode(c_X,c_Y,InDirection.BelowRight)
                    };
                    break;
                default:
                    break;
            }
        }
        private void LogNodeConnections(Node node)
        {
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

        public override bool Equals(object obj)
        {
            Node that = obj as Node;

            return !ReferenceEquals(null, that)
                && int.Equals(this.X, that.X)
                && int.Equals(this.Y, that.Y);
        }
        public override int GetHashCode()
        {
            unchecked
            {
                // Choose large primes to avoid hashing collisions
                const int HashingBase = (int)2166136261;
                const int HashingMultiplier = 16777619;

                int hash = HashingBase;
                hash = (hash * HashingMultiplier) ^ (!ReferenceEquals(null, X) ? X.GetHashCode() : 0);
                hash = (hash * HashingMultiplier) ^ (!ReferenceEquals(null, Y) ? Y.GetHashCode() : 0);
                return hash;
            }
        }
        public static bool operator ==(Node nodeA, Node nodeB)
        {
            if (ReferenceEquals(nodeA, nodeB))
            {
                return true;
            }

            if (ReferenceEquals(null, nodeA))
            {
                return false;
            }

            return (nodeA.Equals(nodeB));
        }
        public static bool operator !=(Node nodeA, Node nodeB)
        {
            return !(nodeA == nodeB);
        }
    }
    public class Path
    {
        public List<Vector2> Coordinates = new List<Vector2>();
    }
    public class NodeConnection
    {
        public Node A;
        public Node B;
        public bool Valid { get { return A.Valid && B.Valid; } }

        public NodeConnection(Node a, Node b)
        {
            A = a;
            B = b;
        }

        public override bool Equals(object value)
        {
            NodeConnection that = value as NodeConnection;

            return !ReferenceEquals(null, that) &&
                ((Node.ReferenceEquals(A, that.A) && Node.ReferenceEquals(B, that.B)) ||
                (Node.ReferenceEquals(A, that.B) && Node.ReferenceEquals(B, that.A)));
        }
        public override int GetHashCode()
        {
            unchecked
            {
                // Choose large primes to avoid hashing collisions
                const int HashingBase = (int)2166136261;
                const int HashingMultiplier = 16777619;

                int hash = HashingBase;
                hash = (hash * HashingMultiplier) ^ 
                    ((!ReferenceEquals(null, A) ? A.GetHashCode() : 0) ^ 
                    (!ReferenceEquals(null, B) ? B.GetHashCode() : 0));
                return hash;
            }
        }
        public static bool operator ==(NodeConnection connectionA, NodeConnection connectionB)
        {
            if (ReferenceEquals(connectionA, connectionB))
            {
                return true;
            }

            if (ReferenceEquals(null, connectionA))
            {
                return false;
            }

            return (connectionA.Equals(connectionB));
        }
        public static bool operator !=(NodeConnection connectionA, NodeConnection connectionB)
        {
            return !(connectionA == connectionB);
        }
    }
    public enum FieldShape { Diamond, Square }
}