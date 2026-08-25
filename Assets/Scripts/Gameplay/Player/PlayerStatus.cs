using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BomberTeen;

[RequireComponent(typeof(PlayerAnimatorSync))]
public class PlayerStatus : NetworkBehaviour
{
    // OBSERVER PATTERN: The player's radio. Other scripts listen when they announce their death.
    public static event System.Action<PlayerStatus> OnPlayerDied;

    private PlayerAnimatorSync animatorSync;

    private void Awake()
    {
        animatorSync = GetComponent<PlayerAnimatorSync>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Server Authority: Somente o MasterClient valida colisões letais
        if (!IsServer) return;

        // Checks if the "ghost" object that this player touched belongs to the bomb's Explosion area
        if (other.gameObject.layer == LayerMask.NameToLayer(Constants.Layers.Explosion))
        {
            // O Servidor manda a sentença de morte para todos os computadores (RPC)
            DeathSequenceRpc();
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void DeathSequenceRpc()
    {
        DeathSequence();
    }

    /// <summary>
    /// Starts the death animation, blocks the player's movement and bomb planting controls.
    /// </summary>
    private void DeathSequence()
    {
        // Turns off Input and Bomb so they can't do anything else
        GetComponent<PlayerInput>().enabled = false;
        GetComponent<BombController>().enabled = false;

        animatorSync.spriteRendererUp.enabled = false;
        animatorSync.spriteRendererDown.enabled = false;
        animatorSync.spriteRendererLeft.enabled = false;
        animatorSync.spriteRendererRight.enabled = false;
        
        animatorSync.spriteRendererDeath.enabled = true;

        // Waits 1.25 seconds for the skull animation to finish playing before deleting the player
        Invoke(nameof(OnDeathSequenceEnded), 1.25f);
    }

    /// <summary>
    /// Hides the player completely and broadcasts the event (Observer) to whoever wants to listen.
    /// </summary>
    private void OnDeathSequenceEnded()
    {
        gameObject.SetActive(false);
        
        // RADIO BROADCAST: "I died!" (Triggers for GameManager and MenuManager to hear)
        OnPlayerDied?.Invoke(this);
    }
}