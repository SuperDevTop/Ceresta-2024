using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MS_GridMapGenerator : MonoBehaviour
{
    public GameObject cellPrefab;
    public GameObject borderCellPrefab;
    public GameObject exitCellPrefab;
    public GameObject[] obstaclePrefab;
    public int gridSizeX;
    public int gridSizeY;
    public float cellSize = 1f;
    public float obstacleProbability = 0.2f; // Probability of spawning an obstacle in each cell
    public int numberOfTetrominos = 3; // Number of tetrominos to generate

    [Space(10)]
    public List<Vector2> playerSpawnPos = new List<Vector2>();

    [HideInInspector] public List<Transform> cellTransform = new List<Transform>();
    [HideInInspector] public List<Vector2> tetrominoTransform = new List<Vector2>();

    public List<Transform> listOfSpawnPos = new List<Transform>();

    public GameObject teleportEntryPrefab;
    public GameObject teleportExitPrefab;
    public int numberOfTeleporters = 5;

    private List<Vector2> teleportEntryPositions = new List<Vector2>();
    private List<Vector2> teleportExitPositions = new List<Vector2>();

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

    void Start()
    {
        if (gridSizeX <= 0) gridSizeX = 5;
        if (gridSizeY <= 0) gridSizeY = 5;

        GenerateGridWithObstacles();

        DisableExtraCells();

        GetActivePos();

        ActivatePlayer();

        GenerateKeyAndFire();

        GenerateTeleporters();
    }

    void GenerateTeleporters()
    {
        GenerateTeleporterPositions(teleportEntryPositions, numberOfTeleporters);
        GenerateTeleporterPositions(teleportExitPositions, numberOfTeleporters);

        for (int i = 0; i < numberOfTeleporters; i++)
        {
            // Spawn teleport entry
            GameObject entryTeleporter = Instantiate(teleportEntryPrefab, teleportEntryPositions[i], Quaternion.identity);
            entryTeleporter.transform.parent = transform;

            // Spawn teleport exit
            GameObject exitTeleporter = Instantiate(teleportExitPrefab, teleportExitPositions[i], Quaternion.identity);
            exitTeleporter.transform.parent = transform;
        }
    }

    void GenerateTeleporterPositions(List<Vector2> positions, int numberOfTeleporters)
    {
        for (int i = 0; i < numberOfTeleporters; i++)
        {
            int randomIndex = Random.Range(0, listOfSpawnPos.Count);
            Vector2 teleportPosition = listOfSpawnPos[randomIndex].position;
            positions.Add(teleportPosition);
            listOfSpawnPos.RemoveAt(randomIndex);
        }
    }

    void GenerateGridWithObstacles()
    {
        Vector2 startPosition = new Vector2(transform.position.x - (gridSizeX / 2) * cellSize, transform.position.y + (gridSizeY / 2) * cellSize);

        // List to store valid positions for exit cell on the top border
        List<int> validExitPositions = new List<int>();

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                GameObject prefabToInstantiate = cellPrefab;

                // Check if it's a border cell
                if (y == 0 || y == gridSizeY - 1 || x == 0 || x == gridSizeX - 1)
                {
                    prefabToInstantiate = borderCellPrefab;
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

                cellTransform.Add(newCell.transform);

                if (newCell.tag == "Road" && (spawnPosition.x <= -10 && spawnPosition.y <= -10)) playerSpawnPos.Add(spawnPosition);
            }
        }

        // Choose random position from valid exit positions
        int exitPosX = validExitPositions[Random.Range(0, validExitPositions.Count)];

        // Place the exit cell
        Vector2 exitSpawnPosition = startPosition + new Vector2(exitPosX * cellSize, -cellSize);
        GameObject exitCell = Instantiate(exitCellPrefab, exitSpawnPosition, Quaternion.identity);
        exitCell.transform.parent = transform;
        MS_GameManager.instance.finishLine = exitCell;
        MS_GameManager.instance.finishLine.SetActive(false);

        // Keep track of occupied positions
        HashSet<Vector2> occupiedPositions = new HashSet<Vector2>();

        // Generate specified number of tetrominos
        for (int i = 0; i < numberOfTetrominos; i++)
        {
            int[][] tetromino = tetrominos[Random.Range(0, tetrominos.Length)];

            bool positionOccupied = true;

            while (positionOccupied)
            {
                // Choose a random position for the tetromino
                int posX = Random.Range(1, gridSizeX - tetromino[0].Length);
                int posY = Random.Range(1, gridSizeY - tetromino.Length);

                positionOccupied = false;

                for (int x = 0; x < tetromino[0].Length; x++)
                {
                    for (int y = 0; y < tetromino.Length; y++)
                    {
                        int gridX = posX + x;
                        int gridY = posY + y;

                        if (gridX >= 0 && gridX < gridSizeX && gridY >= 0 && gridY < gridSizeY)
                        {
                            Vector2 spawnPosition = startPosition + new Vector2((posX + x) * cellSize, -(posY + y) * cellSize);

                            // Check if the position is near the exit cell
                            if (Mathf.Abs(gridX - exitPosX) < 2 && Mathf.Abs(gridY - gridSizeY + 1) < 2)
                            {
                                positionOccupied = true;
                                break;
                            }

                            // Check if the position is occupied by a tetromino or obstacle
                            if (IsPositionOccupied(spawnPosition, tetromino[0].Length, tetromino.Length) || occupiedPositions.Contains(spawnPosition))
                            {
                                positionOccupied = true;
                                break;
                            }
                        }
                    }


                    if (positionOccupied)
                    {
                        // If the position is occupied, break out of the loop and generate a new tetromino
                        break;
                    }
                }

                if (!positionOccupied)
                {
                    // If the position is not occupied, add it to the set of occupied positions
                    for (int x = 0; x < tetromino[0].Length; x++)
                    {
                        for (int y = 0; y < tetromino.Length; y++)
                        {
                            int gridX = posX + x;
                            int gridY = posY + y;

                            if (gridX >= 0 && gridX < gridSizeX && gridY >= 0 && gridY < gridSizeY)
                            {
                                Vector2 spawnPosition = new Vector2((posX + x) * cellSize, -(posY + y) * cellSize);
                                occupiedPositions.Add(spawnPosition);
                            }
                        }
                    }

                    // Spawn obstacles based on tetromino shape
                    for (int x = 0; x < tetromino[0].Length; x++)
                    {
                        for (int y = 0; y < tetromino.Length; y++)
                        {
                            int gridX = posX + x;
                            int gridY = posY + y;

                            if (gridX >= 0 && gridX < gridSizeX && gridY >= 0 && gridY < gridSizeY)
                            {
                                Vector2 spawnPosition = startPosition + new Vector2((posX + x) * cellSize, -(posY + y) * cellSize);
                                if (tetromino[y][x] == 1)
                                {
                                    int randomIndex = Random.Range(0, obstaclePrefab.Length);
                                    GameObject newObstacle = Instantiate(obstaclePrefab[randomIndex], spawnPosition, Quaternion.identity);
                                    newObstacle.transform.parent = transform;

                                    tetrominoTransform.Add(spawnPosition);
                                }
                            }
                        }
                    }
                }
            }
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

    void GetActivePos()
    {
        for (int i = 0; i < cellTransform.Count; i++)
        {
            if (cellTransform[i].gameObject.activeInHierarchy && cellTransform[i].tag != "Wall") listOfSpawnPos.Add(cellTransform[i]);
        }
    }

    void GenerateKeyAndFire()
    {
        int rFire = Random.Range(0, listOfSpawnPos.Count);
        GameObject fireClone = Instantiate(MS_GameManager.instance.firePrefab, listOfSpawnPos[rFire].position, Quaternion.identity);
        listOfSpawnPos.RemoveAt(rFire);

        int rKey = Random.Range(0, listOfSpawnPos.Count);
        GameObject keyClone = Instantiate(MS_GameManager.instance.keyPrefab, listOfSpawnPos[rKey].position, Quaternion.identity);
        listOfSpawnPos.RemoveAt(rKey);
    }

    void ActivatePlayer()
    {
        int r = Random.Range(0, listOfSpawnPos.Count);

        MS_GameManager.instance.player.transform.position = listOfSpawnPos[r].position;

        MS_GameManager.instance.player.SetActive(true);

        listOfSpawnPos.RemoveAt(r);
    }
}
