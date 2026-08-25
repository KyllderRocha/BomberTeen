using UnityEngine;
using Unity.Netcode;
using BomberTeen;

/// <summary>
/// Script attached to the Bomb Prefab.
/// Unlike BombController (which is on the Player and handles the act of planting the bomb), 
/// this script deals only with the bomb itself, triggering early detonation 
/// if it is hit by another explosion (Chain Reaction).
/// </summary>
public class Bomb : NetworkBehaviour
{
    /// <summary> PhotonView ID of the player who planted this bomb (to refund). </summary>
    public ulong ownerViewID;
    
    /// <summary> Flag read by the player's BombController to know if the fuse timer should be interrupted. </summary>
    public bool shouldExplode = false;
    
    // Safety lock to prevent multiple explosion triggers from firing repeatedly.
    private bool exploded = false;
	
    // MEMORY CACHE (Prevents garbage allocation/Garbage Collector in Update)
	private Collider2D[] results = new Collider2D[10]; 
    private ContactFilter2D filter;

    private void Awake()
    {
        // Prepares the collision filter once at initialization
        filter = new ContactFilter2D();
        filter.useTriggers = true;
    }

    private void Start()
    {
        // Toca o som localmente em todas as telas quando a bomba nasce na rede
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlaySFX(Constants.Audio.Drop);
        }
    }

    void Update()
    {
        // Server Authority: Somente o MasterClient monitora colisão com fogo
        if (exploded || !IsServer) return;

        // Throws a "virtual box" over the bomb and collects everything it touched using the pre-allocated cache.
        int count = Physics2D.OverlapBox(transform.position, Vector2.one / 2f, 0f, filter, results);
        
        for (int i = 0; i < count; i++)
        {
            Collider2D hit = results[i];
            
            // If the item that touched the bomb is fire from a neighboring explosion...
            if (hit.GetComponentInParent<Explosion>() != null)
            {
                Debug.Log($"[Bomb] Detected an explosion at position {transform.position}! Sending RPC to explode...");
                // Dispara o comando apenas para o MasterClient (que já está executando, mas mantém a arquitetura limpa)
                TriggerExplosionRpc();
                break;
            }
        }
    }

    /// <summary>
    /// Forces the bomb detonation over the network. Called when a chain reaction occurs.
    /// Changes the flag so the owner's BombController notices the interruption.
    /// </summary>
    [Rpc(SendTo.ClientsAndHost)]
    public void TriggerExplosionRpc()
    {
        Debug.Log($"[Bomb] RPC TriggerExplosion received!");
        if (exploded) return; // Prevents infinite loop in case two bombs try to explode at the same time
        exploded = true;
        shouldExplode = true;
        Debug.Log($"[Bomb] shouldExplode set to true! Waiting for BombController.");
    }
}
