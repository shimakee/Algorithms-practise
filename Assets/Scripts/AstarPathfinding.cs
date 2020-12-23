using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AstarPathfinding : IPathfinding
{
    public Queue<Node> VisitedNodes { get { return _visitedNodes; } }
    public List<Node> UnvisitedNodes { get { return _unvisitedNodes; } }

    Queue<Node> _visitedNodes;
    List<Node> _unvisitedNodes;

    public AstarPathfinding()
    {
        _visitedNodes = new Queue<Node>();
        _unvisitedNodes = new List<Node>();
    }

    public Queue<Node> FindPath(Node first, Node second)
    {
        if (first == null || second == null)
            Debug.LogError("Null parameters given");

        if (first.Position == second.Position || first == second)
        {
            Debug.Log("No path to calculate, origin and destination are in the same position.");

            Queue<Node> path = new Queue<Node>();
            path.Enqueue(first);

            return path;
        }

        first.PreviousNode = null;
        second.PreviousNode = null;
        _visitedNodes.Clear();
        _unvisitedNodes.Clear();

        GetNeighborPathAndCost(first, first, second);

        bool hasPath = EvaluatePathToDestination(first, second, _unvisitedNodes);

        if (hasPath)
            return GetNodesPath(first, second);
        return null; // return null or path with empty?
    }


    


    #region Private Core Methods

    Queue<Node> GetNodesPath(Node start, Node destination)
    {
        if (destination == null)
            Debug.LogError("Argument cannot be null");

        Queue<Node> path = new Queue<Node>();
        var currentNode = destination;

        do
        {
            if (!path.Contains(currentNode))
                path.Enqueue(currentNode);

            if (currentNode == start || currentNode.Position == start.Position)
                break;

            if (currentNode.PreviousNode == null)
                break;

            //bool isPreviousSameAsCurrent = currentNode == currentNode.PreviousNode ||
            //                                currentNode.Position == currentNode.PreviousNode.Position;
            //if (isPreviousSameAsCurrent)
            //    break;

            currentNode = currentNode.PreviousNode;
        } while (currentNode != start || currentNode.Position != start.Position);
        //} while (currentNode.PreviousNode != null || currentNode != currentNode.PreviousNode);

        return new Queue<Node>(path.Reverse());
    }

    bool EvaluatePathToDestination(Node current, Node destination, IList<Node> unvisitedNodes)
    {
        if (current == null || destination == null)
            Debug.LogError("destination and current node must not be null");
        if (current == destination || current.Position == destination.Position)
            return true;

        MarkNodeAsVisisted(current);
        EvaluateNeighbors(current, destination);

        if (unvisitedNodes.Count <= 0)
        {
            Debug.Log("No path to destination.");
            return false;
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
        if (UnvisitedNodes.Contains(node))
            UnvisitedNodes.Remove(node);
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

            if (lowestCost.Fcost == node.Fcost)
            {
                lowestCost = (lowestCost.Hcost < node.Hcost) ? lowestCost : node;
            }
            else
            {
                lowestCost = (lowestCost.Fcost < node.Fcost) ? lowestCost : node;
            }

        }

        return lowestCost;
    }

    int ComputeDistance(Node first, Node second)
    {
        var xDiff = first.x - second.x;
        var yDiff = first.y - second.y;

        var cost = Mathf.Sqrt((xDiff * xDiff) + (yDiff * yDiff));

        return (int)(cost * 10);
    }

    void GetNeighborPathAndCost(Node current, Node neighbor, Node destination)
    {
        if (current.CanPass && neighbor.CanPass)
        {
            var distance = ComputeDistance(current, neighbor);
            int gCost = distance + current.Gcost + neighbor.baseCost;

            if (gCost < neighbor.Gcost || neighbor.PreviousNode == null)
            {
                neighbor.Gcost = gCost;
                neighbor.PreviousNode = current;
            }

            ComputeHCost(neighbor, destination);
        }
    }

    void ComputeHCost(Node node, Node destination)
    {
        node.Hcost = ComputeDistance(node, destination);
    }

    #endregion
}
