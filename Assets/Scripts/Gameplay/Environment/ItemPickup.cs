using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

using Unity.Netcode;
using BomberTeen;

/// <summary>
/// Represents the items (Power-Ups) that the player can collect while walking around the map.
/// </summary>
public class ItemPickup : NetworkBehaviour
{
    public enum ItemType
    {
        ExtraBomb,
        BlastRadius,
        SpeedIncrease,
    }

    [Tooltip("Defines what effect this power-up will apply when collected.")]
    public ItemType type;
    [SerializeField] private AudioSource audioGetItem;

    private bool collected = false;

    /// <summary> Identifies the collision (trigger) with a body that has the 'Player' tag. </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Server Authority: Somente o Servidor decide quem tocou no item primeiro
        if (!IsServer || collected) return;

        if (other.CompareTag(Constants.Tags.Player))
        {
            var playerStatus = other.GetComponent<PlayerStatus>();
            if (playerStatus != null && !playerStatus.isDead)
            {
                collected = true;
                playerStatus.CollectItemRpc(type);

                if (NetworkObject != null && NetworkObject.IsSpawned)
                {
                    NetworkObject.Despawn(true);
                }
            }
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        OnTriggerEnter2D(other);
    }
}
