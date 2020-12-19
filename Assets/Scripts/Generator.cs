using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Generator : MonoBehaviour
{
    [SerializeField] Vector2 mapSize = new Vector2(5, 5);
    [SerializeField] int numberOfRows = 5; //use resolution as well?
    [SerializeField] int numberOfColumns = 5;
    [SerializeField] float tileHeight = 0;

    Tilenode[,] _map;
    Vector3 _tileSize;

    private void Awake()
    {
        _map = new Tilenode[numberOfColumns, numberOfRows];

        AdjustTileSize();

    }

    private void Start()
    {
        
    }

    void OnDrawGizmos()
    {
        _map = new Tilenode[numberOfColumns, numberOfRows];

        AdjustTileSize();

        for (int x = 0; x < numberOfColumns; x++)
        {
            for (int y = 0; y < numberOfRows; y++)
            {
                var pos = ComputePosition(x, y, (int)transform.position.z);

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
                _map[x, y] = new Tilenode(x, y);
            }
        }
    }

    void LoadMapData(Tilenode[,] tilenodes)
    {
        if (tilenodes == null)
            Debug.LogError("Cannot load map data of null");

        numberOfColumns = tilenodes.GetLength(0);
        numberOfRows = tilenodes.GetLength(1);

        _map = tilenodes;
    }

    void AdjustTileSize()
    {
        float tileSizeX = mapSize.x / numberOfColumns;
        float tileSizeY = mapSize.y / numberOfRows;

        _tileSize = new Vector3(tileSizeX, tileSizeY, tileHeight);
    }

    Vector3 ComputePosition(int x, int y, int z)
    {
        Vector3 pos = new Vector3(_tileSize.x * x,
                                            _tileSize.y * y,
                                            _tileSize.z);
        return pos;
    }

    #region Utility Methods

    public void AssignNodeToMap(Vector2 position, Tilenode node)
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
