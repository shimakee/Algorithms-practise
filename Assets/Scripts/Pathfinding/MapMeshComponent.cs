using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MapMeshComponent : MonoBehaviour
{
    [SerializeField] GameObject tileSample; //Convert to tiles[]
    [SerializeField] int height = 5; //use resolution as well?
    [SerializeField] int width = 10;

    GridMap<GameObject> Map; //TODO:: will be removed once tilenode implements tile display
    GridMap<Node> _map;
    AstarPathfinding _pathfinding;

    Vector2 startPos;
    Vector2 endPos;

    bool startReady;
    bool endReady;

    private void Awake()
    {
        Map = GenerateGridMap(width, height, tileSample);
        _pathfinding = new AstarPathfinding(width, height);
    }

    // Start is called before the first frame update
    void Start()
    {
  
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("mouse down");
            Vector3 position = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Input.mousePosition.z);
            Vector2 ray = Camera.main.ScreenToWorldPoint(position);
            RaycastHit2D hit = Physics2D.Raycast(ray, Vector2.zero);

            if(hit.collider.gameObject != null)
            {
                Debug.Log("hit");

                var gameObject = hit.collider.gameObject;
                ChangeColorOnObject(gameObject, Color.cyan);

                if (!startReady)
                {
                    startReady = true;
                    startPos = gameObject.transform.position;
                    ChangeColorOnObject(gameObject, Color.green);
                }
                else if(!endReady)
                {
                    endReady = true;
                    endPos = gameObject.transform.position;

                    var path = _pathfinding.FindPath(startPos, endPos);

                    foreach (var item in path)
                    {
                        
                        ChangeColorOnObject(Map[(int)item.x, (int)item.y], Color.gray);

                    }

                    ChangeColorOnObject(gameObject, Color.blue);
                }
            }
        }
    }

    #region Private Core Methods

    GridMap<GameObject> GenerateGridMap(int w, int h, GameObject defaultTile)
    {
        if (w <= 0 || h <= 0)
            Debug.LogError("width or height must be atleast 1");

        var map = new GridMap<GameObject>(w, h);
        for (int x = 0; x < map.width; x++)
        {
            for (int y = 0; y < map.height; y++)
            {
                map[x, y] = Instantiate(defaultTile);
                map[x, y].transform.position = new Vector2(x, y);
            }
        }

        return map;
    }

    void GenerateGridAdjacentNeighborsForNode(Node current)
    {
        if (current == null)
            Debug.LogError("Node cannot be null in order to Find neighbors");

        if(current.Neighbors == null)
            current.SetNeighbors(new List<Node>());

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

                Node neighbor = _map[xCoordinate, yCoordinate];
                if (neighbor == null)
                    _map[xCoordinate, yCoordinate] = neighbor = new Node(xCoordinate, yCoordinate);

                //neighbors.Add(neighbor);
                current.Neighbors.Add(neighbor);
            }
        }

        //return neighbors;
    }


    #endregion

    #region For now


    void WriteInfoOnObject(Node tile, GameObject gameObject)
    {
        var components = gameObject.GetComponentsInChildren<TextMeshProUGUI>();

        for (int i = 0; i < components.Length; i++)
        {
            var component = components[i];
            switch (component.name)
            {
                case "Hcost":
                        component.text = tile.HCost.ToString();
                    break;
                case "Gcost":
                        component.text = tile.GCost.ToString();
                    break;
                case "Fcost":
                        component.text = tile.FCost.ToString();
                    break;
                default:
                    break;
            }
        }
    }

    void ChangeColorOnObject(GameObject gameObject, Color color)
    {
        var component = gameObject.GetComponentInChildren<SpriteRenderer>();

        if (component != null)
        {
            component.color = color;
        }
        else
        {
            Debug.Log("component was empty");
        }
    }

    #endregion
}
