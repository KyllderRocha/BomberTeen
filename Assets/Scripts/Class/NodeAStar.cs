using UnityEngine;

public class NodeAStar
{
    public bool walkable; // Indica se o nó é caminhável ou não
    public Vector3 worldPosition; // Posição do nó no mundo
    public int gridX; // Índice X do nó na matriz do grid
    public int gridY; // Índice Y do nó na matriz do grid
    public int gCost; // Custo do caminho do nó inicial até este nó
    public int hCost; // Heurística do custo estimado deste nó até o nó destino
    public NodeAStar parent; // Nó pai usado para rastrear o caminho

    public int fCost => gCost + hCost; // Custo total (f = g + h)

    public NodeAStar(bool walkable, Vector3 worldPosition, int gridX, int gridY)
    {
        this.walkable = walkable;
        this.worldPosition = worldPosition;
        this.gridX = gridX;
        this.gridY = gridY;
    }
}
