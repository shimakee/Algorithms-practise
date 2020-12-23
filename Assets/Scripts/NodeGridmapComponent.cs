using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NodeGridmapComponent : MonoBehaviour
{
    [Header("Map details:")]
    [Range(1, 50)] [SerializeField] float mapWidth = 10;
    [Range(1, 50)][SerializeField] int numberOfColumns = 5;

    [Range(1, 50)] [SerializeField] float mapHeight = 10;
    [Range(1, 50)][SerializeField] int numberOfRows = 5; //use resolution as well?
    [Space(2)]

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
        EstablishNodeConnectionForAll(numberOfColumns, numberOfRows);

    }

    private void Start()
    {
        
    }

    private void FixedUpdate()
    {
    }

    void OnDrawGizmos()
    {
        DrawTilesOnMap();

        ////Path finding
        //var pathFinding = new AstarPathfinding(new GridMap<Node>(_map));
        //var start = _map[0, 0];
        //var end = _map[10, 5];
        //var path = pathFinding.FindPath(start, end);

        //if(path != null)
        //foreach (var item in path)
        //{
        //    Vector2 pos = ComputePosition(item.X, item.Y, (int)transform.position.z, _tileSize);

        //    Gizmos.color = Color.cyan;
        //    Gizmos.DrawCube(pos, _tileSize);
        //}

        //Gizmos.color = Color.green;
        //Gizmos.DrawCube(ComputePosition(start.X, start.Y, (int)transform.position.z, _tileSize), _tileSize);
        //Gizmos.color = Color.blue;
        //Gizmos.DrawCube(ComputePosition(start.X, start.Y, (int)transform.position.z, _tileSize), _tileSize);
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

    void EstablishNodeConnectionForAll(int numberOfColumns, int numberOfRows)
    {
        for (int x = 0; x < numberOfColumns; x++)
        {
            for (int y = 0; y < numberOfRows; y++)
            {
                EstablishGridNodeConnection(_map[x, y]);
            }
        }
    }

    void EstablishGridNodeConnection(Node current)
    {
        if (current == null)
            Debug.LogError("Node cannot be null in order to assign neighbors");

        current.Neighbors.Clear();

        for (int x = -1; x < 2; x++)
        {
            for (int y = -1; y < 2; y++)
            {
                if (x == 0 && y == 0)
                    continue;

                int xCoordinate = current.x + x;
                int yCoordinate = current.y + y;
                bool isBeyondMap = xCoordinate < 0 || xCoordinate >= Map.GetLength(0) ||
                                    yCoordinate < 0 || yCoordinate >= Map.GetLength(1);
                if (isBeyondMap)
                    continue;

                Node neighbor = Map[xCoordinate, yCoordinate];
                if (neighbor == null)
                    continue;

                current.Neighbors.Add(neighbor);
            }
        }
    }

    void DrawTilesOnMap()
    {
        _mapSize = new Vector2(mapWidth, mapHeight);
        _map = new Node[numberOfColumns, numberOfRows];
        _tileSize = AdjustTileSize();

        for (int x = 0; x < numberOfColumns; x++)
        {
            for (int y = 0; y < numberOfRows; y++)
            {
                var pos = ComputePosition(x, y, (int)transform.position.z, _tileSize);
                bool canPass = !Physics2D.BoxCast(pos, new Vector2(_tileSize.x * .9f, _tileSize.y * .9f), 0, new Vector2(0, 0));

                //Gizmos - can remove
                if (canPass)
                {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawWireCube(pos, new Vector3(_tileSize.x * .99f, _tileSize.y * .99f, _tileSize.y));
                }
                else
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireCube(pos, new Vector3(_tileSize.x * .99f, _tileSize.y * .99f, _tileSize.y));
                }
            }
        }
    }

    #endregion

    #region Utility Methods
    public void CheckForObstacles()
    {
        CheckForObstacles(numberOfColumns, numberOfRows);
    }
    void CheckForObstacles(int numberOfColumns, int numberOfRows)
    {
        Debug.Log("Checking obstacles");

        for (int x = 0; x < numberOfColumns; x++)
        {
            for (int y = 0; y < numberOfRows; y++)
            {
                var pos = ComputePosition(x, y, (int)transform.position.z, _tileSize);
                bool canPass = !Physics2D.BoxCast(pos, new Vector2(_tileSize.x * .9f, _tileSize.y * .9f), 0, new Vector2(0, 0));

                var node = _map[x, y];

                if (node != null)
                    node.CanPass = canPass;
            }
        }
    }

    #endregion
}
