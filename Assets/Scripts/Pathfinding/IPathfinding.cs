using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPathfinding
{
    Queue<Node> FindPath(Node origin, Node destination);
    Queue<Node> VisitedNodes { get; }
    List<Node> UnvisitedNodes { get; }
}
