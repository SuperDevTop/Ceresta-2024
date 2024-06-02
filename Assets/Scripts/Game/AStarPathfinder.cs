using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

public class AStarPathfinder
{
    private Tilemap tilemap;
    private int[,] grid;
    private Vector2[] directions = {
        new Vector2(0, 1),
        new Vector2(0, -1),
        new Vector2(1, 0),
        new Vector2(-1, 0)
    };

    public AStarPathfinder(Tilemap tilemap)
    {
        this.tilemap = tilemap;
    }

    public List<Vector2> FindPath(Vector2 startPos, Vector2 endPos)
    {
        // Initialize grid
        InitializeGrid();

        // Initialize lists
        List<Vector2> openList = new List<Vector2>();
        HashSet<Vector2> closedList = new HashSet<Vector2>();

        // Add start node to open list
        openList.Add(startPos);

        // Create dictionaries for storing parent and cost values
        Dictionary<Vector2, Vector2> parentMap = new Dictionary<Vector2, Vector2>();
        Dictionary<Vector2, int> costMap = new Dictionary<Vector2, int>();

        // Initialize cost for start position
        costMap[startPos] = 0;

        while (openList.Count > 0)
        {
            // Find node with lowest cost in open list
            Vector2 currentPos = openList[0];
            for (int i = 1; i < openList.Count; i++)
            {
                if (costMap.ContainsKey(openList[i]) && costMap[openList[i]] < costMap[currentPos])
                {
                    currentPos = openList[i];
                }
            }

            // Move current node from open list to closed list
            openList.Remove(currentPos);
            closedList.Add(currentPos);

            // If we reached the end position, reconstruct and return path
            if (currentPos == endPos)
            {
                return ReconstructPath(parentMap, endPos);
            }

            // Explore neighbors
            foreach (Vector2 direction in directions)
            {
                Vector2 neighborPos = currentPos + direction;

                // Check if neighbor is walkable and not in closed list
                if (IsWalkable(neighborPos) && !closedList.Contains(neighborPos))
                {
                    // Calculate cost from start to neighbor
                    int newCost = costMap[currentPos] + 1; // Assuming uniform cost

                    // If neighbor is not in open list or new cost is lower
                    if (!openList.Contains(neighborPos) || newCost < costMap[neighborPos])
                    {
                        // Update cost and parent
                        costMap[neighborPos] = newCost;
                        parentMap[neighborPos] = currentPos;

                        // Add neighbor to open list
                        if (!openList.Contains(neighborPos))
                        {
                            openList.Add(neighborPos);
                        }
                    }
                }
            }
        }

        // No path found
        return new List<Vector2>();
    }

    private void InitializeGrid()
    {
        BoundsInt bounds = tilemap.cellBounds;
        grid = new int[bounds.size.x, bounds.size.y];

        for (int x = 0; x < bounds.size.x; x++)
        {
            for (int y = 0; y < bounds.size.y; y++)
            {
                Vector3Int pos = new Vector3Int(bounds.x + x, bounds.y + y, 0);
                grid[x, y] = IsWalkable(new Vector2(pos.x, pos.y)) ? 0 : 1;
            }
        }
    }

    private bool IsWalkable(Vector2 pos)
    {
        TileBase tile = tilemap.GetTile(tilemap.WorldToCell(pos));
        return tile != null; // Assuming non-null tiles are walkable
    }

    private List<Vector2> ReconstructPath(Dictionary<Vector2, Vector2> parentMap, Vector2 endPos)
    {
        List<Vector2> path = new List<Vector2>();
        Vector2 currentPos = endPos;

        while (parentMap.ContainsKey(currentPos))
        {
            path.Add(currentPos);
            currentPos = parentMap[currentPos];
        }

        path.Reverse();
        return path;
    }
}