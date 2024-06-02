using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using UnityEngine.Networking;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine.AI;
using UnityEngine.Tilemaps;
using System.Linq;

public class GameMapGenerator : MonoBehaviourPunCallbacks, IOnEventCallback
{
    [Serializable]
    public class PositionInfos
    {
        public List<Vector2> positionBricksHor;
        public List<Vector2> positionBricksVer;
        public List<Vector2> positionBricksJoint;
        public List<Vector2> positionBricksTunnelIn;
        public List<Vector2> positionBricksTunnelOut;
        public List<Vector2> positionRoofs;
        public List<Vector2> positionPits;
    }

    [Serializable]
    public class RemovedPosition
    {
        public List<Vector2> positionBricksHor;
        public List<Vector2> positionBricksVer;
        public List<Vector2> positionBricksJoint;
    }

    public static GameMapGenerator instance;
    public GameObject cellPrefab;
    public GameObject borderCellPrefab;
    public GameObject exitCellPrefab;
    public GameObject[] obstaclePrefab;
    public int gridSizeX;
    public int gridSizeY;
    public float cellSize = 1f;
    public float obstacleProbability = 0.2f; // Probability of spawning an obstacle in each cell
    public int numberOfTetrominos = 3; // Number of tetrominos to generate

    public List<Vector2> positionForBricks = new List<Vector2>();
    public List<Vector2> positionForBricksHorizontal = new List<Vector2>();
    public List<Vector2> positionForBricksVertical = new List<Vector2>();
    public List<Vector2> positionForBricksJoint = new List<Vector2>();
    public List<Vector2> positionForRoofs = new List<Vector2>();
    public List<Vector2> positionForPits = new List<Vector2>();
    public List<Vector2> positionForTeleInput = new List<Vector2>();
    public List<Vector2> positionForTeleOutput = new List<Vector2>();
    public List<GameObject> generatedObstaclesHor = new List<GameObject>();
    public List<GameObject> generatedObstaclesVer = new List<GameObject>();
    public List<GameObject> generatedObstaclesJoint = new List<GameObject>();
    public PositionInfos positionInfo;
    public RemovedPosition removedInfo;

    [Space(10)]
    public List<Vector2> playerSpawnPos = new List<Vector2>();

    [HideInInspector] public List<Transform> cellTransform = new List<Transform>(); 
    [HideInInspector] public List<Vector2> tetrominoTransform = new List<Vector2>();    

    public List<Transform> listOfSpawnPos = new List<Transform>();

    public GameObject teleportEntryPrefab;
    public GameObject teleportExitPrefab;
    public int numberOfTeleporters = 5;
    public static bool isStarted;
    public static string gameMapInfos;

    // Path Finding
    public GameObject flag;
    [HideInInspector] public List<Transform> pathTransforms = new List<Transform>();
    [HideInInspector] public List<Vector2Int> pathVectors = new List<Vector2Int>();
    public GameObject tempObject;
    public float timer = 0f;  

    //private List<Vector2> teleportEntryPositions = new List<Vector2>();
    //private List<Vector2> teleportExitPositions = new List<Vector2>();
    //public Text testText;

    // Define tetromino shapes
    private readonly int[][][] tetrominos =
    {
        // I Tetromino
        new int[][] {
            new int[] {1},
            new int[] {1},
            new int[] {1},
            new int[] {1}
        },
        // J Tetromino
        new int[][] {
            new int[] {1, 0},
            new int[] {1, 0},
            new int[] {1, 1}
        },
        // L Tetromino
        new int[][] {
            new int[] {0, 1},
            new int[] {0, 1},
            new int[] {1, 1}
        },
        // O Tetromino
        new int[][] {
            new int[] {1, 1},
            new int[] {1, 1}
        },
        // S Tetromino
        new int[][] {
            new int[] {0, 1, 1},
            new int[] {1, 1, 0}
        },
        // T Tetromino
        new int[][] {
            new int[] {1, 1, 1},
            new int[] {0, 1, 0}
        },
        // Z Tetromino
        new int[][] {
            new int[] {1, 1, 0},
            new int[] {0, 1, 1}
        }
    };
    private LineRenderer lineRenderer;

    void Start()
    {
        instance = this;
        lineRenderer = GetComponent<LineRenderer>();

        if (gridSizeX <= 0) gridSizeX = 5;
        if (gridSizeY <= 0) gridSizeY = 5;

        isStarted = false;

        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(DelayToActivatePlayer());
        }
    }

    public void Update()
    {
              
    }    
    

    // Define a class for representing graph nodes
    private class Node
    {
        public Transform transform;
        public List<Node> neighbors;

        public Node(Transform transform)
        {
            this.transform = transform;
            neighbors = new List<Node>();
        }
    }

    // Perform A* pathfinding
    public List<Transform> FindPath(Transform startTransform, Transform endTransform, List<Transform> partTransform)
    {
        // Create graph nodes from pathTransforms
        Dictionary<Transform, Node> nodeMap = new Dictionary<Transform, Node>();
        foreach (Transform pathTransform in partTransform)
        {
            nodeMap[pathTransform] = new Node(pathTransform);
        }

        // Connect neighboring nodes
        foreach (Transform pathTransform in partTransform)
        {
            Node node = nodeMap[pathTransform];
            foreach (Transform otherTransform in partTransform)
            {
                if (pathTransform != otherTransform && CanConnect(pathTransform, otherTransform))
                {
                    node.neighbors.Add(nodeMap[otherTransform]);
                }
            }
        }

        // Get corresponding nodes for start and end transforms
        Node startNode = nodeMap[startTransform];
        Node endNode = nodeMap[endTransform];

        // Run A* algorithm
        List<Transform> path = AStar(startNode, endNode, nodeMap.Values.ToList());   

        return path;
    }

    // A* algorithm implementation
    private List<Transform> AStar(Node start, Node goal, List<Node> allNodes)
    {
        List<Transform> path = new List<Transform>();

        HashSet<Node> closedSet = new HashSet<Node>();
        Dictionary<Node, Node> cameFrom = new Dictionary<Node, Node>();
        Dictionary<Node, float> gScore = new Dictionary<Node, float>();

        // Initialize scores for all nodes
        foreach (Node node in allNodes)
        {
            gScore[node] = Mathf.Infinity;
        }

        // Initialize starting node
        gScore[start] = 0;

        // Use a simpler heuristic function (distance between nodes)
        float HeuristicCostEstimate(Node from, Node to) => Vector3.Distance(from.transform.position, to.transform.position);

        // A* loop
        while (true)
        {
            // Find node with lowest fScore in open set
            Node current = null;
            float lowestFScore = Mathf.Infinity;
            foreach (Node node in allNodes)
            {
                float fScore = gScore[node] + HeuristicCostEstimate(node, goal);
                if (!closedSet.Contains(node) && fScore < lowestFScore)
                {
                    lowestFScore = fScore;
                    current = node;
                }
            }

            // If current is goal, reconstruct path and return
            if (current == goal)
            {
                path = ReconstructPath(cameFrom, current);
                break;
            }

            // Move current from open set to closed set
            closedSet.Add(current);

            // Update scores for neighboring nodes
            foreach (Node neighbor in current.neighbors)
            {
                if (neighbor == null)
                {
                    Debug.LogError("Neighbor node is null.");
                    continue;
                }

                if (closedSet.Contains(neighbor))
                    continue;

                // Ensure neighbor node exists in the graph
                if (!gScore.ContainsKey(neighbor))
                {
                    Debug.LogError("Neighbor node not found in graph.");
                    continue;
                }

                float tentativeGScore = gScore[current] + Vector3.Distance(current.transform.position, neighbor.transform.position);

                // Check if the tentative score is better than the current score for the neighbor
                if (tentativeGScore < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeGScore;
                }
            }
        }

        return path;
    }

    // Helper function to check if two nodes can be connected
    private bool CanConnect(Transform a, Transform b)
    {
        // You can define your own criteria for determining if two transforms can be connected
        // For example, you might check if they are within a certain distance of each other
        return Vector3.Distance(a.position, b.position) < 1.5f;
    }

    // Helper function to reconstruct the path from the cameFrom dictionary
    private List<Transform> ReconstructPath(Dictionary<Node, Node> cameFrom, Node current)
    {
        List<Transform> path = new List<Transform>();
        path.Add(current.transform);
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Insert(0, current.transform);
        }
        return path;
    }    

    void GenerateGridWithObstacles()
    {
        // Load map data
        string jsonData = "";

        jsonData = PlayerPrefs.GetString("INFOS_ROOM1");               

        positionInfo = JsonUtility.FromJson<PositionInfos>(jsonData);

        // Remove destroyed tiles
        if(gameMapInfos != null)
        {
            removedInfo = JsonUtility.FromJson<RemovedPosition>(gameMapInfos);

            for (int i = 0; i < removedInfo.positionBricksHor.Count; i++)
            {
                positionInfo.positionBricksHor.Remove(removedInfo.positionBricksHor[i]);
            }

            for (int i = 0; i < removedInfo.positionBricksVer.Count; i++)
            {
                positionInfo.positionBricksVer.Remove(removedInfo.positionBricksVer[i]);
            }

            for (int i = 0; i < removedInfo.positionBricksJoint.Count; i++)
            {
                positionInfo.positionBricksJoint.Remove(removedInfo.positionBricksJoint[i]);
            }
        }                

        Vector2 startPosition = new Vector2(transform.position.x - (gridSizeX / 2) * cellSize, transform.position.y + (gridSizeY / 2) * cellSize);

        // List to store valid positions for exit cell on the top border
        List<int> validExitPositions = new List<int>();
        bool isWall = false;

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                GameObject prefabToInstantiate = cellPrefab;
                isWall = false;

                // Check if it's a border cell
                if (y == 0 || y == gridSizeY - 1)
                {
                    prefabToInstantiate = obstaclePrefab[9];
                    isWall = true;
                }
                else if (x == gridSizeX - 1 || x == 0)
                {
                    prefabToInstantiate = obstaclePrefab[10];
                    isWall = true;
                }

                // Check if it's an exit cell
                if (y == gridSizeY - 1 && x != 0 && x != gridSizeX - 1)
                {
                    validExitPositions.Add(x); // Add valid position on the top border
                }

                Vector2 spawnPosition = startPosition + new Vector2(x * cellSize, -y * cellSize);

                // Instantiate cell prefab
                GameObject newCell = Instantiate(prefabToInstantiate, spawnPosition, Quaternion.identity);
                newCell.transform.parent = transform; // Set the grid object as the parent

                if (!isWall)
                {
                    cellTransform.Add(newCell.transform);
                    pathTransforms.Add(newCell.transform);
                    pathVectors.Add(new Vector2Int(x, y));
                }                            

                if (newCell.tag == "Road" && (spawnPosition.x <= -10 && spawnPosition.y <= -10)) playerSpawnPos.Add(spawnPosition);
            }
        }

        // Choose random position from valid exit positions
        //int exitPosX = validExitPositions[UnityEngine.Random.Range(0, validExitPositions.Count)];

        //// Place the exit cell
        //Vector2 exitSpawnPosition = startPosition + new Vector2(exitPosX * cellSize, -cellSize);
        //GameObject exitCell = Instantiate(exitCellPrefab, exitSpawnPosition, Quaternion.identity);
        //exitCell.transform.parent = transform;
        //GameManager.instance.finishLine = exitCell;
        //GameManager.instance.finishLine.SetActive(false);

        // Keep track of occupied positions
        HashSet<Vector2> occupiedPositions = new HashSet<Vector2>();

        // Generate walls
        //for (int i = 0; i < positionForBricksHorizontal.Count; i++)
        //{
        //    Vector2 spawnPositionForBricks = startPosition + new Vector2((positionForBricksHorizontal[i].x) * cellSize, -(positionForBricksHorizontal[i].y) * cellSize);
        //    GameObject newObstacle = Instantiate(obstaclePrefab[5], spawnPositionForBricks, Quaternion.identity);
        //    newObstacle.transform.parent = transform;

        //    tetrominoTransform.Add(spawnPositionForBricks);
        //    occupiedPositions.Add(spawnPositionForBricks);
        //}

        //for (int i = 0; i < positionForBricksVertical.Count; i++)
        //{
        //    Vector2 spawnPositionForBricks = startPosition + new Vector2((positionForBricksVertical[i].x) * cellSize, -(positionForBricksVertical[i].y) * cellSize - 1.4f);
        //    GameObject newObstacle = Instantiate(obstaclePrefab[7], spawnPositionForBricks, Quaternion.identity);
        //    newObstacle.transform.parent = transform;

        //    tetrominoTransform.Add(spawnPositionForBricks);
        //    occupiedPositions.Add(spawnPositionForBricks);
        //}

        //for (int i = 0; i < positionForBricksJoint.Count; i++)
        //{
        //    Vector2 spawnPositionForBricks = startPosition + new Vector2((positionForBricksJoint[i].x) * cellSize, -(positionForBricksJoint[i].y) * cellSize);
        //    GameObject newObstacle = Instantiate(obstaclePrefab[8], spawnPositionForBricks, Quaternion.identity);
        //    newObstacle.transform.parent = transform;

        //    tetrominoTransform.Add(spawnPositionForBricks);
        //    occupiedPositions.Add(spawnPositionForBricks);
        //}

        // Generate walls from json
        for (int i = 0; i < positionInfo.positionBricksHor.Count; i++)
        {
            Vector2 spawnPositionForBricks = startPosition + new Vector2((positionInfo.positionBricksHor[i].x) * cellSize, -(positionInfo.positionBricksHor[i].y) * cellSize);
            //GameObject newObstacle = PhotonNetwork.Instantiate(obstaclePrefab[5].name, spawnPositionForBricks, Quaternion.identity, 0);
            GameObject newObstacle = Instantiate(obstaclePrefab[5], spawnPositionForBricks, Quaternion.identity);
            newObstacle.transform.parent = transform;
            newObstacle.GetComponent<WallInfo>().wallCoordinate = positionInfo.positionBricksHor[i];

            tetrominoTransform.Add(spawnPositionForBricks);
            occupiedPositions.Add(spawnPositionForBricks);
            generatedObstaclesHor.Add(newObstacle);

            int tempIndex = pathVectors.IndexOf(new Vector2Int((int)positionInfo.positionBricksHor[i].x, (int)positionInfo.positionBricksHor[i].y));
            
            if(tempIndex >= 0)
            {
                pathTransforms.RemoveAt(tempIndex);
                pathVectors.RemoveAt(tempIndex);
            }                
        }

        for (int i = 0; i < positionInfo.positionBricksVer.Count; i++)
        {
            Vector2 spawnPositionForBricks = startPosition + new Vector2((positionInfo.positionBricksVer[i].x) * cellSize, -(positionInfo.positionBricksVer[i].y + 1) * cellSize);
            GameObject newObstacle = Instantiate(obstaclePrefab[7], spawnPositionForBricks, Quaternion.identity);
            newObstacle.transform.parent = transform;
            newObstacle.GetComponent<WallInfo>().wallCoordinate = positionInfo.positionBricksVer[i];

            tetrominoTransform.Add(spawnPositionForBricks);
            occupiedPositions.Add(spawnPositionForBricks);
            generatedObstaclesVer.Add(newObstacle);

            int tempIndex = pathVectors.IndexOf(new Vector2Int((int)(positionInfo.positionBricksVer[i].x), (int)(positionInfo.positionBricksVer[i].y + 1)));

            if (tempIndex >= 0)
            {
                pathTransforms.RemoveAt(tempIndex);
                pathVectors.RemoveAt(tempIndex);
            }
        }

        for (int i = 0; i < positionInfo.positionBricksJoint.Count; i++)
        {
            Vector2 spawnPositionForBricks = startPosition + new Vector2((positionInfo.positionBricksJoint[i].x) * cellSize, -(positionInfo.positionBricksJoint[i].y) * cellSize);
            GameObject newObstacle = Instantiate(obstaclePrefab[8], spawnPositionForBricks, Quaternion.identity);
            newObstacle.transform.parent = transform;
            newObstacle.GetComponent<WallInfo>().wallCoordinate = positionInfo.positionBricksJoint[i];

            tetrominoTransform.Add(spawnPositionForBricks);
            occupiedPositions.Add(spawnPositionForBricks);
            generatedObstaclesJoint.Add(newObstacle);      

            int tempIndex = pathVectors.IndexOf(new Vector2Int((int)(positionInfo.positionBricksJoint[i].x), (int)(positionInfo.positionBricksJoint[i].y)));
            
            if(tempIndex >= 0)
            {
                pathTransforms.RemoveAt(tempIndex);
                pathVectors.RemoveAt(tempIndex);
            }
        }

        // Generate roofs
        //for (int i = 0; i < positionForRoofs.Count; i++)
        //{
        //    Vector2 spawnPositionForBricks = startPosition + new Vector2((positionForRoofs[i].x + 1) * cellSize, -(positionForRoofs[i].y + 4) * cellSize);
        //    GameObject newObstacle = Instantiate(obstaclePrefab[2], spawnPositionForBricks, Quaternion.identity);
        //    newObstacle.transform.parent = transform;

        //    tetrominoTransform.Add(spawnPositionForBricks);
        //    occupiedPositions.Add(spawnPositionForBricks);
        //}

        // From json
        for (int i = 0; i < positionInfo.positionRoofs.Count; i++)
        {
            Vector2 spawnPositionForBricks = startPosition + new Vector2((positionInfo.positionRoofs[i].x + 1) * cellSize, -(positionInfo.positionRoofs[i].y + 4) * cellSize);
            GameObject newObstacle = Instantiate(obstaclePrefab[2], spawnPositionForBricks, Quaternion.identity);
            newObstacle.transform.parent = transform;

            tetrominoTransform.Add(spawnPositionForBricks);
            occupiedPositions.Add(spawnPositionForBricks);
        }

        // Generate big pits
        //for (int i = 0; i < 6; i++)
        //{
        //    Vector2 spawnPositionForBricks = startPosition + new Vector2((positionForPits[i].x + 1) * cellSize, -(positionForPits[i].y + 4) * cellSize);
        //    GameObject newObstacle = Instantiate(obstaclePrefab[4], spawnPositionForBricks, Quaternion.identity);
        //    newObstacle.transform.parent = transform;

        //    tetrominoTransform.Add(spawnPositionForBricks);
        //    occupiedPositions.Add(spawnPositionForBricks);
        //}

        for (int i = 0; i < 6; i++)
        {
            Vector2 spawnPositionForBricks = startPosition + new Vector2((positionInfo.positionPits[i].x + 1) * cellSize, -(positionInfo.positionPits[i].y + 4) * cellSize);
            GameObject newObstacle = Instantiate(obstaclePrefab[4], spawnPositionForBricks, Quaternion.identity);
            newObstacle.transform.parent = transform;

            tetrominoTransform.Add(spawnPositionForBricks);
            occupiedPositions.Add(spawnPositionForBricks);
        }

        // Generate small pits
        //for (int i = 6; i < positionForPits.Count; i++)
        //{
        //    Vector2 spawnPositionForBricks = startPosition + new Vector2((positionForPits[i].x + 1) * cellSize, -(positionForPits[i].y + 4) * cellSize);
        //    GameObject newObstacle = Instantiate(obstaclePrefab[6], spawnPositionForBricks, Quaternion.identity);
        //    newObstacle.transform.parent = transform;

        //    tetrominoTransform.Add(spawnPositionForBricks);
        //    occupiedPositions.Add(spawnPositionForBricks);
        //}

        for (int i = 6; i < positionInfo.positionPits.Count; i++)
        {
            Vector2 spawnPositionForBricks = startPosition + new Vector2((positionInfo.positionPits[i].x + 1) * cellSize, -(positionInfo.positionPits[i].y + 4) * cellSize);
            GameObject newObstacle = Instantiate(obstaclePrefab[6], spawnPositionForBricks, Quaternion.identity);
            newObstacle.transform.parent = transform;

            tetrominoTransform.Add(spawnPositionForBricks);
            occupiedPositions.Add(spawnPositionForBricks);
        }

        // Generate teleport areas from Json
        for (int i = 0; i < positionInfo.positionBricksTunnelIn.Count; i++)
        {
            Vector2 spawnPositionForBricks = startPosition + new Vector2((positionInfo.positionBricksTunnelIn[i].x + 1) * cellSize, -(positionInfo.positionBricksTunnelIn[i].y + 1) * cellSize);
            GameObject newObstacle = Instantiate(teleportEntryPrefab, spawnPositionForBricks, Quaternion.identity);
            newObstacle.transform.parent = transform;

            tetrominoTransform.Add(spawnPositionForBricks);
            occupiedPositions.Add(spawnPositionForBricks);
        }

        for (int i = 0; i < positionInfo.positionBricksTunnelOut.Count; i++)
        {
            Vector2 spawnPositionForBricks = startPosition + new Vector2((positionInfo.positionBricksTunnelOut[i].x + 1) * cellSize, -(positionInfo.positionBricksTunnelOut[i].y + 1) * cellSize);
            GameObject newObstacle = Instantiate(teleportExitPrefab, spawnPositionForBricks, Quaternion.identity);
            newObstacle.transform.parent = transform;

            tetrominoTransform.Add(spawnPositionForBricks);
            occupiedPositions.Add(spawnPositionForBricks);
        }

        // Generate teleport
        //for (int i = 0; i < positionForTeleInput.Count; i++)
        //{
        //    Vector2 spawnPositionForBricks = startPosition + new Vector2((positionForTeleInput[i].x + 1) * cellSize, -(positionForTeleInput[i].y + 1) * cellSize);
        //    GameObject newObstacle = Instantiate(teleportEntryPrefab, spawnPositionForBricks, Quaternion.identity);
        //    newObstacle.transform.parent = transform;

        //    tetrominoTransform.Add(spawnPositionForBricks);
        //    occupiedPositions.Add(spawnPositionForBricks);
        //}

        //for (int i = 0; i < positionForTeleOutput.Count; i++)
        //{
        //    Vector2 spawnPositionForBricks = startPosition + new Vector2((positionForTeleOutput[i].x + 1) * cellSize, -(positionForTeleOutput[i].y + 1) * cellSize);
        //    GameObject newObstacle = Instantiate(teleportExitPrefab, spawnPositionForBricks, Quaternion.identity);
        //    newObstacle.transform.parent = transform;

        //    tetrominoTransform.Add(spawnPositionForBricks);
        //    occupiedPositions.Add(spawnPositionForBricks);
        //}        

        for (int i = 0; i < cellTransform.Count - 200; i++)
        {
            if (!occupiedPositions.Contains(cellTransform[i].transform.position))
            {
                listOfSpawnPos.Add(cellTransform[i].transform);
            }
        }

        // Save map data
        //string saveFilePath = Path.Combine(Application.streamingAssetsPath, "PlayerData.json");
        //positionInfo.positionBricksHor = positionForBricksHorizontal;
        //positionInfo.positionBricksVer = positionForBricksVertical;
        //positionInfo.positionBricksJoint = positionForBricksJoint;
        //positionInfo.positionBricksTunnelIn = positionForTeleInput;
        //positionInfo.positionBricksTunnelOut = positionForTeleOutput;
        //positionInfo.positionPits = positionForPits;
        //positionInfo.positionRoofs = positionForRoofs;
        //string saveWallPositions = JsonUtility.ToJson(positionInfo);
        //File.WriteAllText(saveFilePath, saveWallPositions);
    }

    // Click path button
    public void ShowPath()
    {            
        ShowShortPath();
    }

    // Show short path
    public void ShowShortPath()
    {
        List<Transform> targets = new List<Transform>();
        List<Transform> partTransforms = new List<Transform>();

        Transform flagTarget = tempObject.transform;
        Transform playerTarget = tempObject.transform;
        Transform playerPos = tempObject.transform;

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        for (int i = 0; i < players.Length; i++)
        {
            if (players[i].GetComponent<PhotonView>().AmOwner)
            {
                playerPos = players[i].transform;
                break;
            }
        }

        playerTarget = FindNearestTransform(playerPos.gameObject, pathTransforms);
        partTransforms = FindNearestTransforms(playerPos.gameObject, pathTransforms, 450);

        flagTarget = FindNearestTransform(flag, partTransforms);
        playerTarget = FindNearestTransform(playerPos.gameObject, partTransforms);
        //targets.Add(flag.transform);

        List<Transform> paths = FindPath(flagTarget, playerTarget, partTransforms);

        foreach (Transform path in paths)
        {
            targets.Add(path);
        }

        targets.Add(playerPos);

        lineRenderer.positionCount = targets.Count;

        for (int i = 0; i < targets.Count; i++)
        {
            lineRenderer.SetPosition(i, targets[i].position + new Vector3(0, 0, -3f));
        }
    }

    bool IsPositionOccupied(Vector2 position, int width, int height)
    {
        Collider[] colliders = Physics.OverlapBox(position, new Vector3(width * cellSize / 10, height * cellSize / 10, 0.1f));

        foreach (Collider collider in colliders)
        {
            if (collider.tag == "Thorn" || collider.tag == "Grass" || collider.tag == "Pit")
            {
                return true;
            }
        }

        return false;
    }

    void DisableExtraCells()
    {
        for (int i = 0; i < cellTransform.Count; i++)
        {
            Vector2 mPos = new Vector2(cellTransform[i].position.x, cellTransform[i].position.y);

            if (tetrominoTransform.Contains(mPos))
            {
                if (cellTransform[i].gameObject.GetComponent<Collider>()) cellTransform[i].gameObject.GetComponent<Collider>().enabled = false;
                cellTransform[i].gameObject.SetActive(false);
            }
        }
    }

    //void GenerateKeyAndFire()
    //{
    //    int rFire = UnityEngine.Random.Range(0, listOfSpawnPos.Count);
    //    GameObject fireClone = Instantiate(GameManager.instance.firePrefab, listOfSpawnPos[rFire].position, Quaternion.identity);
    //    listOfSpawnPos.RemoveAt(rFire);

    //    int rKey = UnityEngine.Random.Range(0, listOfSpawnPos.Count);
    //    GameObject keyClone = Instantiate(GameManager.instance.keyPrefab, listOfSpawnPos[rKey].position, Quaternion.identity);
    //    listOfSpawnPos.RemoveAt(rKey);
    //}

    // Find the nearest transform from player
    public Transform FindNearestTransform(GameObject player, List<Transform> partTransform)
    {
        if (partTransform == null || partTransform.Count == 0)
        {
            Debug.LogWarning("No cell transforms to search through.");
            return null;
        }

        Transform nearestTransform = null;
        float minDistance = Mathf.Infinity;

        foreach (Transform pathTransform in partTransform)
        {
            float distanceToPlayer = Vector3.Distance(pathTransform.position, player.transform.position);
            
            if (distanceToPlayer < minDistance)
            {
                minDistance = distanceToPlayer;
                nearestTransform = pathTransform;
            }
        }

        return nearestTransform;
    }

    // Find nearest transforms from palyer
    public List<Transform> FindNearestTransforms(GameObject player, List<Transform> partTransforms, int count)
    {
        if (partTransforms == null || partTransforms.Count == 0)
        {
            Debug.LogWarning("No cell transforms to search through.");
            return new List<Transform>();
        }

        List<Transform> nearestTransforms = new List<Transform>();
        Dictionary<Transform, float> transformDistances = new Dictionary<Transform, float>();

        foreach (Transform pathTransform in partTransforms)
        {
            float distanceToPlayer = Vector3.Distance(pathTransform.position, player.transform.position);
            transformDistances.Add(pathTransform, distanceToPlayer);
        }

        // Sort the dictionary by value (distance) in ascending order
        var sortedDistances = transformDistances.OrderBy(x => x.Value);

        // Get the count nearest transforms
        int countClamped = Mathf.Min(count, sortedDistances.Count());
        for (int i = 0; i < countClamped; i++)
        {
            nearestTransforms.Add(sortedDistances.ElementAt(i).Key);
        }

        return nearestTransforms;
    }

    // Show player on the map
    void ActivatePlayer()
    {
        if (!MainPun.isPlaying)
        {
            //int r = UnityEngine.Random.Range(0, listOfSpawnPos.Count);

            //GameManager.instance.player.transform.position = listOfSpawnPos[r].position;

            GameManager.instance.player.GetComponent<SpriteMask>().enabled = true;

            //listOfSpawnPos.RemoveAt(r);

            MainPun.isPlaying = true;
        }                
    }

    IEnumerator DelayToActivatePlayer()
    {
        GenerateGridWithObstacles();

        yield return new WaitForSeconds(0.5f);

        DisableExtraCells();
        
        yield return new WaitForSeconds(0.1f);
        ActivatePlayer();
        isStarted = true;
    }

    public override void OnEnable()
    {
        base.OnEnable();
        PhotonNetwork.AddCallbackTarget(this);
    }

    public override void OnDisable()
    {
        base.OnDisable();
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    public void OnEvent(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;

        if (eventCode == 2)
        {
            object[] infos = (object[])photonEvent.CustomData;

            if (!isStarted)
            {
                string wallInfos = infos[0].ToString();
                gameMapInfos = wallInfos;

                StartCoroutine(DelayToActivatePlayer());
            }            
        }
        else if (eventCode == 5)
        {
            object[] infos = (object[])photonEvent.CustomData;

            switch ((int)infos[1])
            {
                case 0:
                    {
                        int removeIndex = positionInfo.positionBricksHor.IndexOf((Vector2)infos[0]);         
                        generatedObstaclesHor[removeIndex].gameObject.SetActive(false);
                        generatedObstaclesHor[removeIndex].gameObject.transform.position = new Vector3(10000f, 10000f, 10000f);

                        removedInfo.positionBricksHor.Add((Vector2)infos[0]);
                        generatedObstaclesHor.RemoveAt(removeIndex);
                        positionInfo.positionBricksHor.RemoveAt(removeIndex);
                    }
                    
                    break;
                case 1:
                    {
                        int removeIndex = positionInfo.positionBricksVer.IndexOf((Vector2)infos[0]);                     
                        generatedObstaclesVer[removeIndex].gameObject.SetActive(false);
                        generatedObstaclesVer[removeIndex].gameObject.transform.position = new Vector3(10000f, 10000f, 10000f);

                        removedInfo.positionBricksVer.Add((Vector2)infos[0]);
                        generatedObstaclesVer.RemoveAt(removeIndex);
                        positionInfo.positionBricksVer.RemoveAt(removeIndex);
                    }

                    break;
                case 2:
                    {
                        int removeIndex = positionInfo.positionBricksJoint.IndexOf((Vector2)infos[0]);                   
                        generatedObstaclesJoint[removeIndex].gameObject.SetActive(false);
                        generatedObstaclesJoint[removeIndex].gameObject.transform.position = new Vector3(10000f, 10000f, 10000f);

                        removedInfo.positionBricksJoint.Add((Vector2)infos[0]);
                        generatedObstaclesJoint.RemoveAt(removeIndex);
                        positionInfo.positionBricksJoint.RemoveAt(removeIndex);
                    }

                    break;

            }
        }
    }
}
