using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Collections;
public class TileBreaker : MonoBehaviour
{
    public Tilemap breakableTilemap;

// maps grid position → color
    private Dictionary <Vector3Int, TileColor> colorMap = new();

    public void BreakTileAtWorldPosition(Vector2 worldPos)
    {
        Vector3Int cell = breakableTilemap.WorldToCell(worldPos);

        if (breakableTilemap.HasTile(cell))
        {
            breakableTilemap.SetTile(cell, null);
        }
    }
    void Start()
    {
    BoundsInt bounds = breakableTilemap.cellBounds;

    foreach (Vector3Int pos in bounds.allPositionsWithin)
    {
        if (breakableTilemap.HasTile(pos))
        {
            colorMap[pos] = GetTileColor(pos);
        }
    }
    }
    TileColor GetTileColor(Vector3Int pos)
    {
    Tile tile = breakableTilemap.GetTile<Tile>(pos);

    if (tile == null) return TileColor.None;

    if (tile.name.Contains("Red")) return TileColor.Red;
    if (tile.name.Contains("Blue")) return TileColor.Blue;
    if (tile.name.Contains("Green")) return TileColor.Green;
    if (tile.name.Contains("Yellow")) return TileColor.Yellow;

    return TileColor.None;
    
    }

    public bool WillBreakTile (Vector3 worldPos)
    {
        Vector3Int start = breakableTilemap.WorldToCell(worldPos);
        if (!breakableTilemap.HasTile(start)) return false;
        return true;
    }
    public void BreakTileCluster(Vector3 worldPos)
    {
        Vector3Int start = breakableTilemap.WorldToCell(worldPos);

        if (!breakableTilemap.HasTile(start)) return;

        TileColor targetColor = colorMap[start];

        StartCoroutine(BreakClusterDelayed(start, targetColor));
    }
    IEnumerator BreakClusterDelayed(Vector3Int start, TileColor targetColor)
    {
        Queue<Vector3Int> queue = new Queue<Vector3Int>();
        HashSet<Vector3Int> visited = new HashSet<Vector3Int>();

        queue.Enqueue(start);
        visited.Add(start);

        while (queue.Count > 0)
        {
            int countThisWave = queue.Count;

            for (int i = 0; i < countThisWave; i++)
            {
                Vector3Int current = queue.Dequeue();

                // break tile
                breakableTilemap.SetTile(current, null);
                colorMap.Remove(current);

                foreach (Vector3Int dir in GetNeighbors())
                {
                    Vector3Int next = current + dir;

                    if (visited.Contains(next)) continue;
                    if (!breakableTilemap.HasTile(next)) continue;
                    if (!colorMap.ContainsKey(next)) continue;

                    if (colorMap[next] == targetColor && targetColor != TileColor.None)
                    {
                        queue.Enqueue(next);
                        visited.Add(next);
                    }
                }
            }

            // 👇 THIS creates the chain reaction delay
            yield return new WaitForSeconds(0.08f);
        }
    }
    Vector3Int[] GetNeighbors()
    {
        return new Vector3Int[]
        {
            Vector3Int.up,
            Vector3Int.down,
            Vector3Int.left,
            Vector3Int.right
        };
    }
}