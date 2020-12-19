﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Pathfinding
{
    public GridMap<Node> Map { get { return _map; } }
    GridMap<Node> _map;
    List<Node> visitedNodes;
    List<Node> unvisitedNodes;

    public Pathfinding(int width, int height)
    {
        _map = new GridMap<Node>(width, height);
        visitedNodes = new List<Node>();
        unvisitedNodes = new List<Node>();
    }

    //    //establish preset info on startin and destination node
    //    EstablishNeighborPathAndCost(first, first, second);
    //    second.HCost = 0;

    //establish node

    //repreat
    //check if node is the destination node
    //generate adjacent coordinates - should not go beyond map -
    //look at all adjacent nodes            -
    //look at specific neighbor         -
    //if neighbor is null - create instance -
    //add to unvisited node        -
    //estimate cost
    //replace to cheaper cost
    //add this as previous node
    //add current node to visited node
    //find neighbor with cheapest cost
    //go to that node

    //repreat
    //check if node is the destination node
    //generate adjacent nodes - should not go beyond map -
    //look at all adjacent nodes          -  
    //look at specific neighbor       -  
    //add as neighbor
    //estimate cost                     -
    //replace to cheaper cost       -
    //add this as previous node     -
    //add to unvisited node             

    //add current node to visited node
    //find neighbor with cheapest cost
    //go to that node

    public List<Vector2> FindPath(Vector2 first, Vector2 second)
    {
        List<Node> path = FindPath(new Node((int)first.x, (int)first.y), new Node((int)second.x, (int)second.y));
        Debug.Log($"Path count {path.Count}");

        List<Vector2> vectorPath = new List<Vector2>();
        for (int i = 0; i < path.Count; i++)
        {
            var node = path[i];
            Debug.Log($"Path x:{node.X}, y:{node.Y}");

            Vector2 vector = new Vector2(node.X, node.Y);
            vectorPath.Add(vector);
        }

        return vectorPath;
    }
    public List<Node> FindPath(Node first, Node second)
    {
        if (first == null || second == null)
            Debug.LogError("Null parameters given");

        if (first.Position == second.Position)
            return null;

        AssignNodeToMap(first);
        AssignNodeToMap(second);
        EstablishNeighborPathAndCost(first, first, second);

        return EvaluatePathToDestination(first, second);
    }

    List<Node> GetNodesPath(Node destination)
    {
        List<Node> path = new List<Node>();

        var currentNode = destination;

        while(currentNode.PreviousNode != null)
        {
            path.Add(currentNode);
            currentNode = currentNode.PreviousNode;

            if (currentNode.PreviousNode == currentNode)
                break;
        }

        return path;
    }

    List<Node> EvaluatePathToDestination(Node current, Node destination)
    {
        if (current == null || destination == null)
            Debug.LogError("destination and current node must not be null");
        if (current == destination || current.Position == destination.Position)
            return GetNodesPath(destination);

        // This could be a call to get neighbors function - 
        // separating neighbor generation from neighbor evaluation
        // The purpose is if we are given a data set that already has a neighbor implemented
        //  then we would olny have to traverse the neighbors bia current.Neighbors instead of generating them ourselves
        // this is uselfull if we want to reuse this path finding algorithm to things other than a tilebased grid system
        for (int x = -1, i = 0; x < 2; x++, i++)
        {
            for (int y = -1, z = 0; y < 2; y++, z++)
            {
                if (x == 0 && y == 0)
                    continue;

                int xCoordinate = current.X + x;
                int yCoordinate = current.Y + y;
                bool isBeyondMap = xCoordinate < 0 || xCoordinate >= Map.width ||
                                    yCoordinate < 0 || yCoordinate >= Map.height;
                if (isBeyondMap)
                    continue;

                Node neighbor = Map[xCoordinate, yCoordinate];
                if (neighbor == null)
                    Map[xCoordinate, yCoordinate] = neighbor = new Node(xCoordinate, yCoordinate);

                current.Neighbors.Add(neighbor);

                ToUnvisited(neighbor);
                EstablishNeighborPathAndCost(current, neighbor, destination);
            }
        }

        toVisisted(current);

        Node lowestUnvisitedNode = FindUnvisitedNodeWithLowestCost();

        return EvaluatePathToDestination(lowestUnvisitedNode, destination);
    }

    void AssignNodeToMap(Node node)
    {
        if (node == null)
            Debug.LogError("node cannot be null");

        var maptile = Map[node.X, node.Y];
        if (maptile != null)
            Debug.LogError("node on map already occupied");

        Map[node.X, node.Y] = node;
    }
    void toVisisted(Node node)
    {
        if (!visitedNodes.Contains(node))
            visitedNodes.Add(node);
        if (unvisitedNodes.Contains(node))
            unvisitedNodes.Remove(node);
    }

    void ToUnvisited(Node node)
    {
        if (!unvisitedNodes.Contains(node) && !visitedNodes.Contains(node))
            unvisitedNodes.Add(node);
    }

    Node FindUnvisitedNodeWithLowestCost()
    {
        return FindLowestCost(unvisitedNodes);
    }
    Node FindLowestCost(IList<Node> nodes)
    {
        if (nodes.Count <= 0)
            return null;

        Node lowestCost = null;
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

    int ComputeCost(Node first, Node second)
    {
        var xDiff = first.X - second.X;
        var yDiff = first.Y - second.Y;

        var cost = Mathf.Sqrt((xDiff * xDiff) + (yDiff * yDiff));

        return (int)(cost * 10);
    }

    void EstablishNeighborPathAndCost(Node current, Node neighbor, Node destination)
    {
        var computedValue = ComputeCost(current, neighbor);
        int gCost = computedValue + current.GCost;
        if(neighbor.GCost > gCost || neighbor.PreviousNode == null)
        {
            neighbor.GCost = gCost;

            //if(neighbor.Position != current.Position)
                neighbor.PreviousNode = current;
        }

        neighbor.HCost = ComputeCost(neighbor, destination);
    }
}