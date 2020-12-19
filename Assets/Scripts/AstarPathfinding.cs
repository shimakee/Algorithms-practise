using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AstarPathfinding
{
    public GridMap<Node> Map { get { return _map; } }
    public Queue<Node> VisitedNodes { get { return _visitedNodes; } }
    public List<Node> UnVisitedNodes { get { return _unvisitedNodes; } }

    GridMap<Node> _map;
    Queue<Node> _visitedNodes;
    List<Node> _unvisitedNodes;

    public AstarPathfinding()
    {
        _visitedNodes = new Queue<Node>();
        _unvisitedNodes = new List<Node>();
    }
    public AstarPathfinding(GridMap<Node> Map)
        :this()
    {
        _map = Map;
    }

    public AstarPathfinding(int width, int height)
        :this(new GridMap<Node>(width, height))
    {
    }

    public Queue<Vector2> FindPath(Vector2 first, Vector2 second)
    {
        bool isBeyondMap = first.x < 0 || first.x >= Map.width || first.y < 0 || first.y >= Map.height ||
                            second.x < 0 || second.x >= Map.width || second.y < 0 || second.y >= Map.height;
        if (isBeyondMap)
            Debug.LogError("Vector coordinates for map are out of bounds.");

        Node firstNode = EstablishVectorToMapAsNodes(first);
        Node secondNode = EstablishVectorToMapAsNodes(second);

        Queue<Node> path = FindPath(firstNode, secondNode);

        return ConvertNodesToVector(path);
    }
    public Queue<Node> FindPath(Node first, Node second)
    {
        if (first == null || second == null)
            Debug.LogError("Null parameters given");

        bool isBeyondMap = first.X < 0 || first.X >= Map.width || first.Y < 0 || first.Y >= Map.height ||
                            second.X < 0 || second.X >= Map.width || second.Y < 0 || second.Y >= Map.height;
        if (isBeyondMap)
            Debug.LogError("Tilenode coordinates are out of bounds from map.");

        if (first.Position == second.Position)
        {
            Debug.Log("No path to calculate, origin and destination are in the same position.");

            Queue<Node> path = new Queue<Node>();
            path.Enqueue(first);

            return path;
        }

        GetNeighborPathAndCost(first, first, second);

        bool hasPath = EvaluatePathToDestination(first, second, _unvisitedNodes);

        if (hasPath)
            return GetNodesPath(second);
        return null; // return null or path with empty?
    }


    


    #region Private Core Methods
    Queue<Vector2> ConvertNodesToVector(IEnumerable<Node> tilenodes)
    {
        Queue<Vector2> vectorPath = new Queue<Vector2>();
        Node[] nodes = tilenodes.ToArray();

        for (int i = 0; i < nodes.Length; i++)
        {
            var node = nodes[i];
            Vector2 vector = new Vector2(node.X, node.Y);

            vectorPath.Enqueue(vector);
        }

        return vectorPath;
    }
    Node EstablishVectorToMapAsNodes(Vector2 node)
    {
        bool isBeyondMap = node.x < 0 || node.x >= Map.width || node.y < 0 || node.y >= Map.height;
        if (isBeyondMap)
            Debug.LogError("Vector coordinates for map are out of bounds.");

        var firstNode = Map[(int)node.x, (int)node.y];

        if (firstNode == null)
            Map[(int)node.x, (int)node.y] = firstNode = new Node((int)node.x, (int)node.y);

        return firstNode;
    }

    Queue<Node> GetNodesPath(Node destination)
    {
        if (destination == null)
            Debug.LogError("Argument cannot be null");

        Queue<Node> path = new Queue<Node>();
        var currentNode = destination;

        do
        {
            if(!path.Contains(currentNode))
                path.Enqueue(currentNode);

            bool isCurrentNodeNull = currentNode.PreviousNode == null;
            if (isCurrentNodeNull)
                break;
            bool isPreviousSameAsCurrent = currentNode == currentNode.PreviousNode ||
                                            currentNode.Position == currentNode.PreviousNode.Position;
            if (isPreviousSameAsCurrent)
                break;

            currentNode = currentNode.PreviousNode;

        } while (currentNode.PreviousNode != null || currentNode != currentNode.PreviousNode);

        return new Queue<Node>(path.Reverse());
    }

    bool EvaluatePathToDestination(Node current, Node destination, IList<Node> unvisitedNodes)
    {
        if (current == null || destination == null)
            Debug.LogError("destination and current node must not be null");
        if (current == destination || current.Position == destination.Position)
            return true;
            //return GetNodesPath(destination);

        MarkNodeAsVisisted(current);
        EvaluateNeighbors(current, destination);

        if (unvisitedNodes.Count <= 0)
        {
            Debug.Log("No path to destination.");
            return false;
            //return GetNodesPath(destination);
        }

        Node lowestCostUnvisitedNode = FindLowestCost(unvisitedNodes);

        return EvaluatePathToDestination(lowestCostUnvisitedNode, destination, unvisitedNodes);
    }

    void EvaluateNeighbors(Node current, Node destination)
    {
        for (int i = 0; i < current.Neighbors.Count; i++)
        {
            var neighbor = current.Neighbors[i];

            if (neighbor.CanPass)
            {
                MarkNodeAsUnvisited(neighbor);
                GetNeighborPathAndCost(current, neighbor, destination);
            }
            else
            {
                MarkNodeAsVisisted(neighbor);
            }
            //if (!unvisitedNodes.Contains(neighbor))
            //    unvisitedNodes.Add(neighbor);

        }
    }

    void MarkNodeAsUnvisited(Node node)
    {
        if (!_unvisitedNodes.Contains(node) && !_visitedNodes.Contains(node))
            _unvisitedNodes.Add(node);
    }

    void MarkNodeAsVisisted(Node node)
    {
        if (!VisitedNodes.Contains(node))
            VisitedNodes.Enqueue(node);
        if (UnVisitedNodes.Contains(node))
            UnVisitedNodes.Remove(node);
    }

    Node FindLowestCost(IList<Node> nodes)
    {
        if (nodes == null || nodes.Count <= 0)
            return null;

        Node lowestCost = null;
        for (int i = 0; i < nodes.Count; i++)
        {
            var node = nodes[i];

            if (!node.CanPass)
                continue;

            if (lowestCost == null)
            {
                lowestCost = node;
                continue;
            }

            if (lowestCost.FCost == node.FCost)
            {
                lowestCost = (lowestCost.HCost < node.HCost) ? lowestCost : node;
            }
            else
            {
                lowestCost = (lowestCost.FCost < node.FCost) ? lowestCost : node;
            }

        }

        return lowestCost;
    }

    int ComputeCost(Node first, Node second)
    {
        var xDiff = first.X - second.X;
        var yDiff = first.Y - second.Y;

        var cost = Mathf.Sqrt((xDiff * xDiff) + (yDiff * yDiff));

        return (int)(cost * 10);
    }

    void GetNeighborPathAndCost(Node current, Node neighbor, Node destination)
    {
        var computedValue = ComputeCost(current, neighbor);
        int gCost = computedValue + current.GCost;

            if (neighbor.GCost > gCost || neighbor.PreviousNode == null)
            {
                neighbor.GCost = gCost;

                //if (neighbor.Position != current.Position)
                if(current.CanPass && neighbor.CanPass)
                    neighbor.PreviousNode = current;
            }

            neighbor.HCost = ComputeCost(neighbor, destination);

    }

    #endregion
}
