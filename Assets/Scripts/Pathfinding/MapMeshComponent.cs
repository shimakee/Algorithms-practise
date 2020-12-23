using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

public class MapMeshComponent : MonoBehaviour
{
    [SerializeField] GameObject tileSample; //Convert to tiles[]
    [SerializeField] GameObject player; //Convert to tiles[]
    [SerializeField] int height = 5; //use resolution as well?
    [SerializeField] int width = 10;
    [SerializeField] float speed = 1;

    GridMap<GameObject> Map; //TODO:: will be removed once tilenode implements tile display
    [SerializeField] GameObject nodeMapGameObject;
    NodeGridmapComponent _nodeMap;
    IPathfinding _pathfinding;

    Vector2 startPos;
    Vector2 endPos;

    bool startReady;
    bool endReady;

    Vector2 _direction;
    Vector2 _currentPosition;

    GameObject _player1;
    GameObject _player2;
    GameObject _player3;

    bool isHold;
    float time = 0;
    float holdTime = .5f;

    Node startNode;
    Node endNode;

    Queue<Node> _route;

    private void Awake()
    {
        _nodeMap = nodeMapGameObject.GetComponent<NodeGridmapComponent>();
        _pathfinding = new AstarPathfinding();
    }

    // Start is called before the first frame update
    void Start()
    {
        Map = GenerateGridMap(width, height, tileSample);

        _currentPosition = Vector2.zero;
        _player1 = Instantiate(player);
        ChangeColorOnObject(_player1, Color.cyan);
        _player1.transform.position = _currentPosition;
    }

    // Update is called once per frame
    void Update()
    {
        HandleMovement();
        HandleActions();
        
    }

    private void FixedUpdate()
    {
        if (startNode != null && endNode != null)
            GeneratePath();

        _nodeMap.CheckForObstacles();
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
                var nodeObject = map[x, y] = Instantiate(defaultTile);
                map[x, y].transform.position = new Vector2(x, y);

                var node = _nodeMap.Map[x, y];
                if (node != null && nodeObject != null)
                    WriteInfoOnObject(node, nodeObject);
            }
        }

        return map;
    }

    #endregion


    #region Player controls

    public void OnMove(InputAction.CallbackContext ctx)
    {
        _direction = ctx.ReadValue<Vector2>();

        //stick to the grid
        if(_direction.magnitude > .2)
        {
            if (_direction.x > 0.2)
                _direction.x = 1;
            if (_direction.x < -0.2)
                _direction.x = -1;
            if (_direction.y > 0.2)
                _direction.y = 1;
            if (_direction.y < -0.2)
                _direction.y = -1;
        }

        Debug.Log($"direction: {_direction}");

        if (ctx.started)
        {
            isHold = true;
            _currentPosition += _direction;
        }
        if (ctx.canceled)
        {
            isHold = false;
        }
    }

    public void OnBlock(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            var node = _nodeMap.Map[(int)_currentPosition.x, (int)_currentPosition.y];
                node.CanPass = false;
                node.PreviousNode = null;

            for (int i = 0; i < node.Neighbors.Count; i++)
            {
                var neighbor = node.Neighbors[i];

                if (neighbor.PreviousNode == node)
                    neighbor.PreviousNode = null;
                for (int z = 0; z < neighbor.Neighbors.Count; z++)
                {
                    var nextDoor = neighbor.Neighbors[z];

                    if (nextDoor == node)
                        neighbor.Neighbors.Remove(nextDoor);
                }
            }

            var nodeObject = Map[node.x, node.y];
            if (nodeObject != null && !node.CanPass)
            {
                ChangeColorOnObject(nodeObject, Color.black);
                WriteInfoOnObject(node, nodeObject);
            }

            Debug.Log($"route count : {_route.Count}");

            //_route.Clear();
            ClearNodes();
        }
            
    }
    public void OnStart(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            if(startNode != null)
            {
                var startNodeObject = Map[(int)startNode.x, (int)startNode.y];
                ChangeColorOnObject(startNodeObject, Color.white);
            }
            
            var node = _nodeMap.Map[(int)_currentPosition.x, (int)_currentPosition.y];
            node.CanPass = true;

            var nodeObject = Map[node.x, node.y];
            if (nodeObject != null && node.CanPass)
            {
                ChangeColorOnObject(nodeObject, Color.green);
                WriteInfoOnObject(node, nodeObject);
            }


            startNode = node;
        }
    }
    public void OnGoal(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            if (endNode != null)
            {
                var endNodeObject = Map[(int)endNode.x, (int)endNode.y];
                ChangeColorOnObject(endNodeObject, Color.white);
            }
            
            var node = _nodeMap.Map[(int)_currentPosition.x, (int)_currentPosition.y];
            node.CanPass = true;

            var nodeObject = Map[node.x, node.y];
            if (nodeObject != null && node.CanPass)
            {
                ChangeColorOnObject(nodeObject, Color.blue);
                WriteInfoOnObject(node, nodeObject);
            }


            endNode = node;
        }
    }
    public void OnSlow(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            var node = _nodeMap.Map[(int)_currentPosition.x, (int)_currentPosition.y];
            node.CanPass = true;
            node.baseCost = 50;

            var nodeObject = Map[node.x, node.y];
            if (nodeObject != null && node.CanPass)
            {
                ChangeColorOnObject(nodeObject, Color.red);
                WriteInfoOnObject(node, nodeObject);
            }
        }
    }

    public void OnFindPath(InputAction.CallbackContext callback)
    {
        GeneratePath();
    }    

    void GeneratePath()
    {
        ClearNodes();

        _route = _pathfinding.FindPath(startNode, endNode);

        DisplayVisitedUnVisitedNodes();

        if(_route != null)
        {
            foreach (var path in _route)
            {
                var tile = Map[(int)path.x, (int)path.y];

                ChangeColorOnObject(tile, Color.gray);
                WriteInfoOnObject(path, tile);
            }
        }

        

        if (endNode != null)
        {
            var endNodeObject = Map[(int)endNode.x, (int)endNode.y];
            ChangeColorOnObject(endNodeObject, Color.blue);
            WriteInfoOnObject(endNode, endNodeObject);
        }

        if (startNode != null)
        {
            var startNodeObject = Map[(int)startNode.x, (int)startNode.y];
            ChangeColorOnObject(startNodeObject, Color.green);
            WriteInfoOnObject(startNode, startNodeObject);
        }
    }

    void ClearNodes()
    {
        foreach (var node in _pathfinding.VisitedNodes)
        {
            node.ClearNode();
            var tile = Map[node.x, node.y];

            if (node.baseCost >= 50)
            {
                ChangeColorOnObject(tile, Color.red);
            }
            else if (!node.CanPass)
            {
                ChangeColorOnObject(tile, Color.black);
            }
            else
            {
                ChangeColorOnObject(tile, Color.white);

            }
            WriteInfoOnObject(node, tile);

        }

        foreach (var node in _pathfinding.UnvisitedNodes)
        {
            node.ClearNode();
            if (node.x < 0 || node.x >= Map.width)
                Debug.LogError($"x: {node.x}, y: {node.y}");
            if (node.y < 0 || node.y >= Map.height)
                Debug.LogError($"x: {node.x}, y: {node.y}");

            var tile = Map[node.x, node.y];

            if (node.baseCost >= 50)
            {
                ChangeColorOnObject(tile, Color.red);
            }
            else if(!node.CanPass)
            {
                ChangeColorOnObject(tile, Color.black);
            }
            else
            {
                ChangeColorOnObject(tile, Color.white);

            }
            WriteInfoOnObject(node, tile);
        }
    }

    void DisplayVisitedUnVisitedNodes()
    {
        foreach (var node in _pathfinding.VisitedNodes)
        {
            var tile = Map[node.x, node.y];
            ChangeColorOnObject(tile, Color.magenta);
            WriteInfoOnObject(node, tile);

        }

        foreach (var node in _pathfinding.UnvisitedNodes)
        {
            var tile = Map[node.x, node.y];
            ChangeColorOnObject(tile, Color.yellow);
            WriteInfoOnObject(node, tile);
        }
    }

    Vector2 TransformToIsometric(Vector2 vector)
    {
        float x = (vector.x * .7f) + (vector.y * .7f);
        float y = (vector.x * -.7f) + (vector.y * .7f);

        return new Vector2(x, y);
    }
    void HandleMovement()
    {
        //
        //_player1.transform.position = _currentPosition;
        //_player2.transform.Translate(_direction * speed * Time.deltaTime);
        //_player2.transform.position = Vector2.MoveTowards(_player2.transform.position, _direction, speed * Time.deltaTime);
        //_player1.transform.position = Vector2.MoveTowards(_player1.transform.position, TransformToIsometric(_currentPosition), speed * Time.deltaTime);
        _player1.transform.position = Vector2.MoveTowards(_player1.transform.position, _currentPosition, speed * Time.deltaTime);

        if (isHold)
        {
            time += Time.deltaTime;

            if (time >= holdTime)
            {
                time = 0;
                _currentPosition += _direction;
            }
        }

        Debug.Log($"current position: {_currentPosition}");

    }

    void HandleActions()
    {

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
                        component.text = tile.Hcost.ToString();
                    break;
                case "Gcost":
                        component.text = tile.Gcost.ToString();
                    break;
                case "baseCost":
                    component.text = tile.baseCost.ToString();
                    break;
                case "Fcost":
                        component.text = tile.Fcost.ToString();
                    break;
                case "canPass":
                    component.text = (tile.CanPass) ? "Yes" : "No";
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
