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
            if(bombsRemaining > 0 && Input.GetKeyDown(plantKey))
            {
                bombsRemaining--; // Desconta localmente na hora para feedback imediato
                
                Vector2 position = transform.position;
                position.x = Mathf.Round(position.x);
                position.y = Mathf.Round(position.y);
                
                // Pede permissão e execução ao Host (Server Authority)
                RequestPlaceBombRpc(position);
            }
        }
        
    }

    [Rpc(SendTo.Server)]
    public void RequestPlaceBombRpc(Vector2 position)
    {
        GameObject bomb = Instantiate(bombPrefabNetwork, position, Quaternion.identity);
        bomb.GetComponent<NetworkObject>().Spawn(true);
        
        Bomb bombScript = bomb.GetComponent<Bomb>();
        if (bombScript != null)
        {
            bombScript.ownerViewID = OwnerClientId; // Guarda quem foi o dono para devolver depois
        }

        // Inicia o timer da bomba exclusivamente no Servidor
        StartCoroutine(ServerPlaceBomb(bomb, position));
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void RefundBombRpc()
    {
        if (IsOwner)
        {
            bombsRemaining++; // Devolve a bomba pro jogador
        }
    }

    /// <summary>
    /// Corrotina rodando SOMENTE no MasterClient. Conta o tempo, explode e apaga a bomba.
    /// </summary>
    private IEnumerator ServerPlaceBomb(GameObject bomb, Vector2 position)
    {
        // position is already passed in parameters, so we just use it directly

        // AudioManager.instance.PlaySFX("Drop"); // Som movido para o Start do Bomb.cs para tocar em rede

        // GameObject bomb = PhotonNetwork.Instantiate(prefabLocation, position, Quaternion.identity);
        
        Bomb bombScript = bomb.GetComponent<Bomb>();

        float timer = bombFuseTime;
        // Wait loop: Runs every frame until time runs out OR the bomb explodes by contact (chain reaction)
        while (timer > 0 && bomb != null)
        {
            // If the bomb script was triggered externally by a fire ray...
            if (bombScript != null && bombScript.shouldExplode)
            {
                Debug.Log("[BombController] Identified shouldExplode = true. Breaking the timer loop!");
                break; // Cuts the waiting time and explodes now
            }
            timer -= Time.deltaTime;
            yield return null; // Pauses execution and returns on the next frame
        }

        Debug.Log("[BombController] Exiting loop. Executing the final bomb explosion!");
        if (bomb != null)
        {
            ExecuteExplosion(bomb.transform.position);
            bomb.GetComponent<NetworkObject>().Despawn();

            // Envia RPC avisando o dono original para recuperar sua bomba
            RefundBombRpc();
        }
    }

    /// <summary>
    /// Generates the visual effect of the "Central" fire where the bomb was and shoots explosion rays
    /// into the 4 directions (Up, Down, Left, Right).
    /// </summary>
    private void ExecuteExplosion(Vector2 position)
    {
        if (!IsServer) return;

        position.x = Mathf.Round(position.x);
        position.y = Mathf.Round(position.y);

        AudioManager.instance.PlaySFX("Explosion");

        // Creates the central fire of the explosion over the network
        var explosionObj = Instantiate(explosionPrefabNetwork, position, Quaternion.identity);
        explosionObj.GetComponent<NetworkObject>().Spawn(true);
        var explosion = explosionObj.GetComponent<Explosion>();

        explosion.SetActiveRendererRpc(Constants.Animations.ExplosionStart); // 'start' is the center sprite
        explosion.DestroyAfterRpc(explosionDuration);

        // Starts the creation of fire rays for each side based on the explosionRadius size
        Explode(position, Vector2.up, explosionRadius);
        Explode(position, Vector2.down, explosionRadius);
        Explode(position, Vector2.left, explosionRadius);
        Explode(position, Vector2.right, explosionRadius);
    }

    /// <summary>
    /// Recursive function responsible for creating a fire trail in the chosen direction.
    /// It checks collisions (using OverlapBox) to destroy boxes or stop at indestructible obstacles.
    /// </summary>
    private void Explode(Vector2 position, Vector2 direction, int length)
    {
        if (length <= 0)
            return;

        position += direction;

        // Projects an "invisible box" on that square to check what's there before placing fire
        Collider2D[] hits = Physics2D.OverlapBoxAll(position, Vector2.one / 2f, 0f);
        bool stopped = false; // Flag to know if the fire hit something and should stop

        foreach (Collider2D hit in hits)
        {
            Debug.Log($"[BombController.Explode] The explosion hit: {hit.name} (Layer: {LayerMask.LayerToName(hit.gameObject.layer)}) at position {position}");

            Bomb hitBomb = hit.GetComponentInParent<Bomb>();
            ItemPickup item = hit.GetComponentInParent<ItemPickup>();

            // If it hit another bomb, triggers a chain reaction on it
                if (hitBomb != null)
            {
                hitBomb.TriggerExplosionRpc();
                stopped = true;
            }
            // If it hit a collectible (power-up), burns it from the network
            else if (item != null)
            {
                if (IsServer && item.GetComponent<NetworkObject>() != null)
                {
                    item.GetComponent<NetworkObject>().Despawn();
                }
            }
            // If it hit a destructible box (comparing LayerMask using bitwise)
            else if (explosionLayerMask == (explosionLayerMask | (1 << hit.gameObject.layer)))
            {
                // Warns the Host (MasterClient) that the map coordinate (Tilemap) should be destroyed
                MapGeneration.instance.DestructibleRpc(position.x, position.y);
                stopped = true;
            }
        }

        // If it found a solid obstacle/wall/box, it doesn't draw fire on this square nor further
        if (stopped)
        {
            return;
        }

        var explosionObj = Instantiate(explosionPrefabNetwork, position, Quaternion.identity);
        explosionObj.GetComponent<NetworkObject>().Spawn(true);
        var explosion = explosionObj.GetComponent<Explosion>();

        // If there are still blocks to expand, uses the body of the ray ("middle"). Otherwise, uses the tip ("end").
        explosion.SetActiveRendererRpc(length > 1 ? Constants.Animations.ExplosionMiddle : Constants.Animations.ExplosionEnd);
        explosion.SetDirectionRpc(direction);
        explosion.DestroyAfterRpc(explosionDuration);

        // Calls itself again for the next square (Recursion) subtracting 1 from the remaining length
        Explode(position, direction, length - 1);
    }

    private void ClearDestructible(Collider2D hitCollider)
    {
        var position = hitCollider.transform.position;

        Destroy(hitCollider.gameObject);

        //Debug.Log(position);
        ////Debug.Log(destructibleTiles);
        //Vector3Int cell = destructibleTiles.WorldToCell(position);
        ////Debug.Log(cell);
        //TileBase tile = destructibleTiles.GetTile(cell);
        //Debug.Log(tile);

        //if (tile != null)
        //{
        //    //Instantiate(destructiblePrefab, position, Quaternion.identity);
        //    //destructibleTiles.SetTile(cell, null);

        //    //var destructibleObj = PhotonNetwork.Instantiate(_localizacaoDestructible, position, Quaternion.identity);
        //    //var explosion = explosionObj.GetComponent<Explosion>();
        //    destructibleTiles.SetTile(cell, null);

        //}
    }

    //private void ClearDestructible(Vector2 position)
    //{
    //    Debug.Log(position);
    //    //Debug.Log(destructibleTiles);
    //    Vector3Int cell = destructibleTiles.WorldToCell(position);
    //    //Debug.Log(cell);
    //    TileBase tile = destructibleTiles.GetTile(cell);
    //    Debug.Log(tile);

    //    if (tile != null)
    //    {
    //        //Instantiate(destructiblePrefab, position, Quaternion.identity);
    //        //destructibleTiles.SetTile(cell, null);

    //        //var destructibleObj = PhotonNetwork.Instantiate(_localizacaoDestructible, position, Quaternion.identity);
    //        //var explosion = explosionObj.GetComponent<Explosion>();
    //        destructibleTiles.SetTile(cell, null);

    //    }
    //}

    private void SetDestructibleTile(Vector3Int cell)
    {
        destructibleTiles.SetTile(cell, null);
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
            other.isTrigger = false;
        }
    }
}
