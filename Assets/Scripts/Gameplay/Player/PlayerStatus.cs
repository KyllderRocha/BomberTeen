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

    public bool isDead { get; private set; } = false;

    private PlayerAnimatorSync animatorSync;

    private void Awake()
    {
        animatorSync = GetComponent<PlayerAnimatorSync>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Server Authority: Somente o Servidor valida colisões letais
        if (!IsServer || isDead) return;

        // Checks if the object that this player touched belongs to the bomb's Explosion area
        if (other.gameObject.layer == LayerMask.NameToLayer(Constants.Layers.Explosion))
        {
            isDead = true;
            // O Servidor manda a sentença de morte para todos os computadores (RPC)
            DeathSequenceRpc();
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        OnTriggerEnter2D(other);
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
        isDead = true;

        // Turns off Input, Movement, and Bomb controls
        var input = GetComponent<PlayerInput>();
        if (input != null) input.enabled = false;

        var bomb = GetComponent<BombController>();
        if (bomb != null) bomb.enabled = false;

        var movement = GetComponent<PlayerMovement>();
        if (movement != null) movement.enabled = false;

        var rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;

        if (animatorSync != null)
        {
            if (animatorSync.spriteRendererUp != null) animatorSync.spriteRendererUp.enabled = false;
            if (animatorSync.spriteRendererDown != null) animatorSync.spriteRendererDown.enabled = false;
            if (animatorSync.spriteRendererLeft != null) animatorSync.spriteRendererLeft.enabled = false;
            if (animatorSync.spriteRendererRight != null) animatorSync.spriteRendererRight.enabled = false;
            if (animatorSync.spriteRendererDeath != null) animatorSync.spriteRendererDeath.enabled = true;
        }

        // Waits 1.25 seconds for the skull animation to finish playing
        Invoke(nameof(OnDeathSequenceEnded), 1.25f);
    }

    /// <summary>
    /// Hides the player visuals and collider (without deactivating the GameObject to avoid NGO lifecycle errors),
    /// and broadcasts the death event.
    /// </summary>
    private void OnDeathSequenceEnded()
    {
        if (animatorSync != null && animatorSync.spriteRendererDeath != null)
        {
            animatorSync.spriteRendererDeath.enabled = false;
        }

        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // RADIO BROADCAST: "I died!" (Triggers for GameManager and MenuManager to hear)
        OnPlayerDied?.Invoke(this);

        if (IsServer && GameManager.instance != null)
        {
            GameManager.instance.CheckWinState();
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void CollectItemRpc(ItemPickup.ItemType itemType)
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlaySFX(Constants.Audio.GetItem);
        }

        switch (itemType)
        {
            case ItemPickup.ItemType.ExtraBomb:
                GetComponent<BombController>()?.AddBomb();
                break;

            case ItemPickup.ItemType.BlastRadius:
                var bc = GetComponent<BombController>();
                if (bc != null) bc.explosionRadius++;
                break;

            case ItemPickup.ItemType.SpeedIncrease:
                var pm = GetComponent<PlayerMovement>();
                if (pm != null) pm.speed++;
                break;
        }
    }
}