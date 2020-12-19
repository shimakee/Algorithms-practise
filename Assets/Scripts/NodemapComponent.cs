using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NodemapComponent : MonoBehaviour
{
    [Header("Map details:")]
    [Range(1, 50)] [SerializeField] float mapWidth = 10;
    [Range(1, 50)] [SerializeField] float mapHeight = 10;

    [Range(1, 50)][SerializeField] int numberOfRows = 5; //use resolution as well?
    [Range(1, 50)][SerializeField] int numberOfColumns = 5;

    [Header("Tile details:")]
    [Range(0, 50)][SerializeField] float tileHeight = 0;

    public Node[,] Map { get { return _map; } }

    Vector2 _mapSize;
    Node[,] _map;
    Vector3 _tileSize;

    private void Awake()
    {
        _mapSize = new Vector2(mapWidth, mapHeight);
        _map = new Node[numberOfColumns, numberOfRows];
        _tileSize = AdjustTileSize();
        GenerateNodesOnMap(numberOfColumns, numberOfRows);
    }

    private void Start()
    {
        
    }

    private void FixedUpdate()
    {
        //bool isCollided = Physics2D.BoxCast(pos, new Vector2(_tileSize.x *.9f, _tileSize.y * .9f), 0, new Vector2(0, 0));
    }

    void OnDrawGizmos()
    {
        _map = new Node[numberOfColumns, numberOfRows];
        _mapSize = new Vector2(mapWidth, mapHeight);
        _tileSize = AdjustTileSize();

        for (int x = 0; x < numberOfColumns; x++)
        {
            for (int y = 0; y < numberOfRows; y++)
            {
                var pos = ComputePosition(x, y, (int)transform.position.z, _tileSize);

                bool isCollided = Physics2D.BoxCast(pos, new Vector2(_tileSize.x *.9f, _tileSize.y * .9f), 0, new Vector2(0, 0));

                if (isCollided)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireCube(pos, new Vector3(_tileSize.x, _tileSize.y, _tileSize.y));
                }
                else
                {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawWireCube(pos, new Vector3(_tileSize.x, _tileSize.y, _tileSize.y));
                }

            }
        }
    }

    void GenerateNodesOnMap(int numberOfColumns, int numberOfRows)
    {
        for (int x = 0; x < numberOfColumns; x++)
        {
            for (int y = 0; y < numberOfRows; y++)
            {
                var pos = ComputePosition(x, y, (int)transform.position.z, _tileSize);
                bool canPass = !Physics2D.BoxCast(pos, new Vector2(_tileSize.x * .9f, _tileSize.y * .9f), 0, new Vector2(0, 0));

                _map[x, y] = new Node(x, y, canPass);
            }
        }
    }

    #region Core Methods
    Vector3 AdjustTileSize()
    {
        float tileSizeX = _mapSize.x / numberOfColumns;
        float tileSizeY = _mapSize.y / numberOfRows;

        _tileSize = new Vector3(tileSizeX, tileSizeY, tileHeight);
        return _tileSize;
    }

    Vector3 ComputePosition(int x, int y, int z, Vector3 tilesize)
    {
        float xOffset = tilesize.x / 2;
        float yOffset = tilesize.y / 2;

        var xPos = transform.position.x + xOffset + (tilesize.x * x);
        var yPos = transform.position.y + yOffset + (tilesize.y * y);
        var zPos = transform.position.z;

        Vector3 pos = new Vector3(xPos, yPos, zPos);
        return pos;
    }

    #endregion

    #region Utility Methods

    public void AssignNodeToMap(Vector2 position, Node node)
    {

        //Tilenode should be an enum of premade tilenode types

        if (node == null)
            Debug.Log("Cannot assign a null node to map");

        var mapNode = _map[node.X, node.Y];
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
            _map[node.X, node.Y] = node;
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
        var node = _map[x, y];

        if (node != null)
            node.GCost = cost;
    }
    #endregion
}
