using System.Collections.Generic;
using UnityEngine;

public static class AStar
{
    public static bool FindPath(NodeAStar startNode, NodeAStar targetNode, GridAStar grid)
    {
        HashSet<NodeAStar> openSet  = new HashSet<NodeAStar>();
        HashSet<NodeAStar> closedSet = new HashSet<NodeAStar>();
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            NodeAStar currentNode = GetLowestFCostNode(openSet);
            openSet.Remove(currentNode);
            closedSet.Add(currentNode); 

            if (currentNode == targetNode)
            {
                return true;
            }

            foreach (NodeAStar neighborNode in grid.GetNeighborNodes(currentNode))
            {
                if (!neighborNode.walkable || closedSet.Contains(neighborNode))
                {
                    continue;
                }

                int tentativeGCost = currentNode.gCost + GetDistance(currentNode, neighborNode);

                if (tentativeGCost < neighborNode.gCost || !openSet.Contains(neighborNode))
                {
                    neighborNode.gCost = tentativeGCost;
                    neighborNode.hCost = GetDistance(neighborNode, targetNode);
                    neighborNode.parent = currentNode;

                    if (!openSet.Contains(neighborNode))
                    {
                        openSet.Add(neighborNode);
                    }
                }
            }
        }

        return false;
    }

    public static List<NodeAStar> GetPath(NodeAStar targetNode)
    {
        List<NodeAStar> path = new List<NodeAStar>();
        NodeAStar currentNode = targetNode;

        while (currentNode != null)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }

        path.Reverse();
        return path;
    }

    private static int GetDistance(NodeAStar nodeA, NodeAStar nodeB)
    {
        int distanceX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int distanceY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        return distanceX + distanceY;
    }

    private static NodeAStar GetLowestFCostNode(HashSet<NodeAStar> openSet)
    {
        NodeAStar lowestFCostNode = null;

        foreach (NodeAStar node in openSet)
        {
            if (lowestFCostNode == null || node.fCost < lowestFCostNode.fCost)
            {
                lowestFCostNode = node;
            }
        }

        return lowestFCostNode;
    }
}
