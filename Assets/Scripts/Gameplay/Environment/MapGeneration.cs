using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

/*
 * PROCEDURAL MAP GENERATION
 * -------------------------------------------
 * This script handles the procedural creation of destructible blocks.
 * It demonstrates a good network pattern: Only the MasterClient (Room Owner) decides where the blocks spawn
 * and then communicates it to the other players via RPC ("SetDestructibleTile").
 * 
 * NullReference Bug fix included in the Cell verification.
 */
public class MapGeneration : NetworkBehaviour
{
    public static MapGeneration instance { get; private set; }

    [Header("Destructible")]
    public Tilemap destructibleTiles;
    public TileBase tileBrick;
    public GameObject destructiblePrefabNetwork;

    [Header("Indestructible")]
    public Tilemap indestructibleTiles;

    // Deterministic seed synchronized automatically by Netcode to all clients
    private readonly NetworkVariable<int> mapSeed = new NetworkVariable<int>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private bool hasGenerated = false;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            gameObject.SetActive(false);
            return;
        }
        instance = this;
    }

    public override void OnNetworkSpawn()
    {
        mapSeed.OnValueChanged += OnSeedChanged;

        if (IsServer)
        {
            // Pick a non-zero deterministic random seed
            mapSeed.Value = Random.Range(1, 1000000);
            GenerateMap(mapSeed.Value);
        }
        else if (mapSeed.Value != 0 && !hasGenerated)
        {
            GenerateMap(mapSeed.Value);
        }
    }

    public override void OnNetworkDespawn()
    {
        mapSeed.OnValueChanged -= OnSeedChanged;
        hasGenerated = false;
    }

    private void OnSeedChanged(int oldVal, int newVal)
    {
        if (newVal != 0 && (!hasGenerated || newVal != oldVal))
        {
            GenerateMap(newVal);
        }
    }

    private void GenerateMap(int seed)
    {
        if (destructibleTiles == null || indestructibleTiles == null) return;
        hasGenerated = true;

        var rng = new System.Random(seed);
        destructibleTiles.ClearAllTiles();

        BoundsInt bounds = indestructibleTiles.cellBounds;
        int blocksSpawned = 0;

        for (int y = bounds.yMin; y < bounds.yMax; y++)
        {
            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                Vector3Int cell = new Vector3Int(x, y, 0);

                TileBase tileIndes = indestructibleTiles.GetTile(cell);
                bool isIndestructibleBlock = tileIndes != null && tileIndes.name.StartsWith("Block", System.StringComparison.OrdinalIgnoreCase);

                // Destructible blocks only spawn on walkable ground (not on indestructible walls/pillars)
                if (!isIndestructibleBlock && tileIndes != null)
                {
                    if (rng.NextDouble() < 0.75)
                    {
                        destructibleTiles.SetTile(cell, tileBrick);
                        blocksSpawned++;
                    }
                }
            }
        }

        ClearSpawnAreas();
        Debug.Log($"[MapGeneration] Mapa gerado com seed {seed}. {blocksSpawned} blocos destrutíveis criados na arena.");
    }

    private void ClearSpawnAreas()
    {
        if (destructibleTiles == null) return;

        List<Vector3> spawnPositions = new List<Vector3>();

        if (GameManager.instance != null && GameManager.instance.GetSpawns() != null && GameManager.instance.GetSpawns().Length > 0)
        {
            foreach (var spawn in GameManager.instance.GetSpawns())
            {
                if (spawn != null)
                {
                    spawnPositions.Add(spawn.position);
                }
            }
        }
        else
        {
            var spawnsObj = GameObject.Find("Spawns");
            if (spawnsObj != null)
            {
                foreach (Transform child in spawnsObj.transform)
                {
                    if (child != null)
                    {
                        spawnPositions.Add(child.position);
                    }
                }
            }
        }

        // Clear tiles around each spawn corner so players can maneuver and plant bombs safely
        foreach (var pos in spawnPositions)
        {
            Vector3Int centerCell = destructibleTiles.WorldToCell(pos);
            destructibleTiles.SetTile(centerCell, null);
            destructibleTiles.SetTile(centerCell + Vector3Int.up, null);
            destructibleTiles.SetTile(centerCell + Vector3Int.down, null);
            destructibleTiles.SetTile(centerCell + Vector3Int.left, null);
            destructibleTiles.SetTile(centerCell + Vector3Int.right, null);
        }
    }

    /// <summary>
    /// Edits the Tilemap component. The RPC is received by all computers to 
    /// place the map boxes in exactly the same coordinates.
    /// </summary>
    [Rpc(SendTo.ClientsAndHost)]
    public void SetDestructibleTileRpc(int x, int y, string tileText)
    {
        if (destructibleTiles == null) return;

        Vector3Int cell = new Vector3Int(x, y, 0);
        TileBase tile = null;
        if (tileText == "Brick")
        {
            tile = tileBrick;
        }
        destructibleTiles.SetTile(cell, tile);
    }

    /// <summary>
    /// Called by the Bomb (on the server) informing that fire hit a destructible box.
    /// Erases the solid block (Tile) and spawns the animated debris model in its place.
    /// </summary>
    [Rpc(SendTo.Server)]
    public void DestructibleRpc(float worldX, float worldY)
    {
        if (destructibleTiles == null) return;

        Vector3 worldPos = new Vector3(worldX, worldY, 0f);
        Vector3Int cell = destructibleTiles.WorldToCell(worldPos);
        TileBase tile = destructibleTiles.GetTile(cell);
        
        if (tile != null)
        {
            // Remove the tile across all clients and host
            SetDestructibleTileRpc(cell.x, cell.y, "");

            // Spawn the 2D debris animation on the server
            if (destructiblePrefabNetwork != null)
            {
                Vector3 cellCenter = destructibleTiles.GetCellCenterWorld(cell);
                var destructibleObj = Instantiate(destructiblePrefabNetwork, cellCenter, Quaternion.identity);
                var netObj = destructibleObj.GetComponent<NetworkObject>();
                
                if (netObj != null)
                {
                    netObj.Spawn(true);
                }
                else
                {
                    Debug.LogError("[MapGeneration] O Prefab configurado no 'Destructible Prefab Network' não possui um componente NetworkObject!");
                }
            }
        }
    }
}
