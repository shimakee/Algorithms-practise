using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AstarPathfinding
{
    public GridMap<Tilenode> Map { get { return _map; } }
    public Queue<Tilenode> VisitedNodes { get { return _visitedNodes; } }
    public List<Tilenode> UnVisitedNodes { get { return _unvisitedNodes; } }

    GridMap<Tilenode> _map;
    Queue<Tilenode> _visitedNodes;
    List<Tilenode> _unvisitedNodes;

    public AstarPathfinding()
    {
        _visitedNodes = new Queue<Tilenode>();
        _unvisitedNodes = new List<Tilenode>();
    }
    public AstarPathfinding(GridMap<Tilenode> Map)
        :this()
    {
        _map = Map;
    }

    public AstarPathfinding(int width, int height)
        :this(new GridMap<Tilenode>(width, height))
    {
    }

    public Queue<Vector2> FindPath(Vector2 first, Vector2 second)
    {
        bool isBeyondMap = first.x < 0 || first.x >= Map.width || first.y < 0 || first.y >= Map.height ||
                            second.x < 0 || second.x >= Map.width || second.y < 0 || second.y >= Map.height;
        if (isBeyondMap)
            Debug.LogError("Vector coordinates for map are out of bounds.");

        Tilenode firstNode = EstablishVectorToMapAsNodes(first);
        Tilenode secondNode = EstablishVectorToMapAsNodes(second);

        Queue<Tilenode> path = FindPath(firstNode, secondNode);

        return ConvertNodesToVector(path);
    }
    public Queue<Tilenode> FindPath(Tilenode first, Tilenode second)
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

            Queue<Tilenode> path = new Queue<Tilenode>();
            path.Enqueue(first);

            return path;
        }

        EvaluateNeighborPathAndCost(first, first, second);

        return EvaluatePathToDestination(first, second, _unvisitedNodes);
    }


    #region Utility Methods

    public void AssignNodeToMap(Vector2 position, Tilenode node)
    {

        //Tilenode should be an enum of premade tilenode types

        if (node == null)
            Debug.Log("Cannot assign a null node to map");

        var mapNode = Map[node.X, node.Y];
        if (mapNode != null)
        {
            for (int i = 0; i < mapNode.Neighbors.Count; i++)
            {
                var neighbor = mapNode.Neighbors[i];

                neighbor.Neighbors.Remove(mapNode);
                neighbor.Neighbors.Add(node);
                node.Neighbors.Add(neighbor);
            }
        }
        else
        {
            Map[node.X, node.Y] = node;
        }
    }

    public void RemoveNodeFromMap(Vector2 node)
    {
        if (node == null)
            Debug.Log("Cannot remove a null node to map");
    }
    public void SetNodesCost(IList<Vector2> coordinates, int cost)
    {
        for (int i = 0; i < coordinates.Count; i++)
        {
            SetNodeGcost(coordinates[i], cost);
        }
    }

    public void SetNodeGcost(Vector2 position, int cost)
    {
        SetNodeGcost((int)position.x, (int)position.y, cost);
    }
    void SetNodeGcost(int x, int y, int cost)
    {
        var node = Map[x, y];

        if(node != null)
            node.GCost = cost;
    }
    #endregion


    #region Private Core Methods
    Queue<Vector2> ConvertNodesToVector(IEnumerable<Tilenode> tilenodes)
    {
        Queue<Vector2> vectorPath = new Queue<Vector2>();
        Tilenode[] nodes = tilenodes.ToArray();

        for (int i = 0; i < nodes.Length; i++)
        {
            var node = nodes[i];
            Vector2 vector = new Vector2(node.X, node.Y);

            vectorPath.Enqueue(vector);
        }

        return vectorPath;
    }
    Tilenode EstablishVectorToMapAsNodes(Vector2 node)
    {
        bool isBeyondMap = node.x < 0 || node.x >= Map.width || node.y < 0 || node.y >= Map.height;
        if (isBeyondMap)
            Debug.LogError("Vector coordinates for map are out of bounds.");

        var firstNode = Map[(int)node.x, (int)node.y];

        if (firstNode == null)
            Map[(int)node.x, (int)node.y] = firstNode = new Tilenode((int)node.x, (int)node.y);

        return firstNode;
    }

    Queue<Tilenode> GetNodesPath(Tilenode destination)
    {
        if (destination == null)
            Debug.LogError("Argument cannot be null");

        Queue<Tilenode> path = new Queue<Tilenode>();
        var currentNode = destination;

        do
        {
            if(!path.Contains(currentNode))
                path.Enqueue(currentNode);
            currentNode = currentNode.PreviousNode;

            bool isPreviousSameAsCurrent = currentNode == currentNode.PreviousNode || currentNode.Position == currentNode.PreviousNode.Position;
            if (isPreviousSameAsCurrent)
                break;

        } while (currentNode.PreviousNode != null || currentNode != currentNode.PreviousNode);

        return new Queue<Tilenode>(path.Reverse());
    }

    Queue<Tilenode> EvaluatePathToDestination(Tilenode current, Tilenode destination, IList<Tilenode> unvisitedNodes)
    {
        if (current == null || destination == null)
            Debug.LogError("destination and current node must not be null");
        if (current == destination || current.Position == destination.Position)
            return GetNodesPath(destination);

        if (unvisitedNodes.Contains(current))
            unvisitedNodes.Remove(current);

        //We are putting add to visited queue method to easily comment it out
        // Not essential for the algorithm to function
        // only placed for convinience, incase we want to display the nodes
        AddNodeToVisitedQueue(current);

        // separating neighbor generation from neighbor evaluation
        // The purpose is if we are given a data set that already has a neighbor implemented
        // We can easily swap out or comment out the neighbor assignment with whatever method
        //current.SetNeighbors(GenerateGridNeighborsForNode(current));
        AssignGridNeighborsForNode(current);

        for (int i = 0; i < current.Neighbors.Count; i++)
        {
            var neighbor = current.Neighbors[i];

            AddNodeToUnvisitedQueue(neighbor);
            //if (!unvisitedNodes.Contains(neighbor))
            //    unvisitedNodes.Add(neighbor);

            EvaluateNeighborPathAndCost(current, neighbor, destination);
        }

        Tilenode lowestCostUnvisitedNode = FindLowestCost(unvisitedNodes);

        return EvaluatePathToDestination(lowestCostUnvisitedNode, destination, unvisitedNodes);
    }

    void AssignGridNeighborsForNode(Tilenode current)
    {
        if (current == null)
            Debug.LogError("Node cannot be null in order to assign neighbors");

        //List<Tilenode> neighbors = new List<Tilenode>();
        current.Neighbors.Clear();

        for (int x = -1; x < 2; x++)
        {
            for (int y = -1; y < 2; y++)
            {
                if (x == 0 && y == 0)
                    continue;

                int xCoordinate = current.X + x;
                int yCoordinate = current.Y + y;
                bool isBeyondMap = xCoordinate < 0 || xCoordinate >= Map.width ||
                                    yCoordinate < 0 || yCoordinate >= Map.height;
                if (isBeyondMap)
                    continue;

                Tilenode neighbor = Map[xCoordinate, yCoordinate];
                if (neighbor == null)
                    Map[xCoordinate, yCoordinate] = neighbor = new Tilenode(xCoordinate, yCoordinate);

                //neighbors.Add(neighbor);
                current.Neighbors.Add(neighbor);

            }
        }

        //return neighbors;
    }

    void AddNodeToUnvisitedQueue(Tilenode node)
    {
        if (!_unvisitedNodes.Contains(node) && !_visitedNodes.Contains(node))
            _unvisitedNodes.Add(node);
    }

    void AddNodeToVisitedQueue(Tilenode node)
    {
        if (!VisitedNodes.Contains(node))
            VisitedNodes.Enqueue(node);
    }

    void ToUnvisited(Tilenode node)
    {
        if (!_unvisitedNodes.Contains(node) && !_visitedNodes.Contains(node))
            _unvisitedNodes.Add(node);
    }

    Tilenode FindLowestCost(IList<Tilenode> nodes)
    {
        if (nodes.Count <= 0)
            return null;

        Tilenode lowestCost = null;
        for (int i = 0; i < nodes.Count; i++)
        {
            var node = nodes[i];

            if (i == 0)
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

    int ComputeCost(Tilenode first, Tilenode second)
    {
        var xDiff = first.X - second.X;
        var yDiff = first.Y - second.Y;

        var cost = Mathf.Sqrt((xDiff * xDiff) + (yDiff * yDiff));

        return (int)(cost * 10);
    }

    void EvaluateNeighborPathAndCost(Tilenode current, Tilenode neighbor, Tilenode destination)
    {
        var computedValue = ComputeCost(current, neighbor);
        int gCost = computedValue + current.GCost;
        if (neighbor.GCost > gCost || neighbor.PreviousNode == null)
        {
            neighbor.GCost = gCost;

            //if (neighbor.Position != current.Position)
                neighbor.PreviousNode = current;
        }

        neighbor.HCost = ComputeCost(neighbor, destination);
    }

    #endregion
}
