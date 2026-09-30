using Unity.Netcode;
using System.Collections;
using UnityEngine;
using BomberTeen;
using UnityEngine.Tilemaps;

public class BombController : NetworkBehaviour
{
    /*
     * Improvements
     * 
     * Collision with player
     * Collision with static bomb
     * Explosion affecting other bombs
     * Explosion erasing power up
     * 
    */

    [Header("Bomb")]
    [Tooltip("Key used to plant the bomb.")]
    public KeyCode plantKey = KeyCode.Space;
    public GameObject bombPrefab;
    
    [Tooltip("Time in seconds (fuse) until the bomb explodes by itself.")]
    public float bombFuseTime = 3.0f;
    
    [Tooltip("Maximum amount of bombs the player can have at the same time on the screen.")]
    public int bombAmount = 1;
    private int bombsRemaining = 0; // Internal control of bombs available to use

    [Header("Explosion")]
    public Explosion explosionPrefab;
    
    [Tooltip("Tells Unity which Layers the explosion fire should interact/destroy (e.g. the Destructible layer).")]
    public LayerMask explosionLayerMask;
    public static float explosionDurationStatic = 1f;
    public float explosionDuration = 1f; // Time the fire stays lit on the screen
    
    [Tooltip("Size of the explosion radius (how many blocks the fire reaches). Increases when picking up power-ups.")]
    public int explosionRadius = 1;


    [Header("Destructible")]
    // Old references or for local systems (before migrating to network).
    public Tilemap destructibleTiles;
    public Destructible destructiblePrefab;

    [Header("Network Prefabs (Photon Resources)")]
    // These names (strings) must exactly match the names of the Prefabs saved in the 'Resources' folder of the project.
    public GameObject bombPrefabNetwork;
    public GameObject explosionPrefabNetwork;
    public GameObject destructiblePrefabNetwork;

    public MapGeneration mapGeneration;

    private void Awake()
    {
        mapGeneration = GetComponent<MapGeneration>();
    }

    private void OnEnable()
    {
        bombsRemaining = bombAmount;
    }

    private void Update()
    {
        if (IsSpawned && IsOwner)
        {
            if (bombsRemaining > 0 && Input.GetKeyDown(plantKey))
            {
                Vector2 position = transform.position;
                position.x = Mathf.Round(position.x);
                position.y = Mathf.Round(position.y);

                // Prevent placing multiple bombs on the exact same tile
                Collider2D existingBomb = Physics2D.OverlapBox(position, Vector2.one * 0.5f, 0f, LayerMask.GetMask("Bomb"));
                if (existingBomb != null) return;
                
                bombsRemaining--; // Desconta localmente na hora para feedback imediato
                
                // Pede permissão e execução ao Servidor
                RequestPlaceBombRpc(position, explosionRadius);
            }
        }
    }

    [Rpc(SendTo.Server)]
    public void RequestPlaceBombRpc(Vector2 position, int radius)
    {
        if (bombPrefabNetwork == null) return;

        // Double check on server to prevent overlapping bombs or race conditions
        Collider2D existingBomb = Physics2D.OverlapBox(position, Vector2.one * 0.5f, 0f, LayerMask.GetMask("Bomb"));
        if (existingBomb != null)
        {
            RefundBombRpc();
            return;
        }

        GameObject bomb = Instantiate(bombPrefabNetwork, position, Quaternion.identity);
        var netObj = bomb.GetComponent<NetworkObject>();
        if (netObj != null)
        {
            netObj.Spawn(true);
        }
        
        Bomb bombScript = bomb.GetComponent<Bomb>();
        if (bombScript != null)
        {
            bombScript.ownerClientId = OwnerClientId;
            bombScript.explosionRadius = radius;
            bombScript.bombFuseTime = bombFuseTime;
            bombScript.explosionDuration = explosionDuration;
            bombScript.explosionLayerMask = explosionLayerMask;
            bombScript.explosionPrefabNetwork = explosionPrefabNetwork;
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void RefundBombRpc()
    {
        if (IsOwner)
        {
            bombsRemaining = Mathf.Min(bombsRemaining + 1, bombAmount);
        }
    }

    public void AddBomb()
    {
        bombAmount++;
        bombsRemaining++;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Bomb"))
        {
            var bomb = other.GetComponent<Bomb>();
            if (bomb != null)
            {
                bomb.SetSolidRpc();
            }
            else
            {
                other.isTrigger = false;
            }
        }
    }
}
