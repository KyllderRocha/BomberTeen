using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.WSA;

public class GridAStar
{
    private Tilemap tilemap;
    private NodeAStar[,] nodes;
    private Vector3 origin;
    private int gridSizeX;
    private int gridSizeY;

    public GridAStar(Tilemap tilemap)
    {
        this.tilemap = tilemap;
    }

    public NodeAStar[,] CreateGrid()
    {
        int width = tilemap.size.x;
        int height = tilemap.size.y;
        gridSizeX = width;
        gridSizeY = height;

        Vector3 cellSize = tilemap.cellSize;
        origin = tilemap.origin;

        nodes = new NodeAStar[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3Int tilePosition = new Vector3Int(x, y, 0) + Vector3Int.FloorToInt(origin);
                Vector3 worldPosition = tilemap.CellToWorld(tilePosition) + cellSize / 2f;

                bool walkable = tilemap.HasTile(tilePosition);
                var tile = tilemap.GetTile(tilePosition);

                walkable = walkable && tile.name != "Block";

                nodes[x, y] = new NodeAStar(walkable, worldPosition, x, y);
            }
        }

        return nodes;
    }

    public NodeAStar NodeFromWorldPosition(Vector3 worldPosition)
    {
        int x = Mathf.RoundToInt(worldPosition.x - (origin.x + 1));
        int y = Mathf.RoundToInt(worldPosition.y - (origin.y + 1));


        return nodes[x, y];
    }

    public Vector3 GetWorldPositionFromNode(NodeAStar node)
    {
        return node.worldPosition;
    }

    //Ajustar para não pegar as diagonais
    public NodeAStar[] GetNeighborNodes(NodeAStar node)
    {
        int startX = Mathf.Max(0, node.gridX - 1);
        int endX = Mathf.Min(tilemap.size.x - 1, node.gridX + 1);
        int startY = Mathf.Max(0, node.gridY - 1);
        int endY = Mathf.Min(tilemap.size.y - 1, node.gridY + 1);

        List<NodeAStar> neighbors = new List<NodeAStar>();

        for (int x = startX; x <= endX; x++)
        {
            for (int y = startY; y <= endY; y++)
            {
                if ((x == node.gridX && y == node.gridY) || (x != node.gridX && y != node.gridY))
                {
                    continue;
                }

                neighbors.Add(nodes[x, y]);
            }
        }

        return neighbors.ToArray();
    }

    public NodeAStar GetNodeFromWorldPosition(Vector3 worldPosition)
    {
        float percentX = Mathf.Clamp01((worldPosition.x + tilemap.size.x / 2) / tilemap.size.x);
        float percentY = Mathf.Clamp01((worldPosition.y + tilemap.size.y / 2) / tilemap.size.y);

        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);

        return nodes[x, y];
    }
}
