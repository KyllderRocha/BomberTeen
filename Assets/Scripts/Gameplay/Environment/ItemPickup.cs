using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

using Photon.Pun;

/// <summary>
/// Represents the items (Power-Ups) that the player can collect while walking around the map.
/// </summary>
public class ItemPickup : MonoBehaviourPun
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

    /// <summary> Applies the bonus to the player and deletes the item from the screen. </summary>
    private void OnItemPickup(GameObject player)
    {
        AudioManager.instance.PlaySFX("GetItem");

        switch (type)
        {
            case ItemType.ExtraBomb:
                player.GetComponent<BombController>().AddBomb(); // Increases the maximum amount of bombs
                break;

            case ItemType.BlastRadius:
                player.GetComponent<BombController>().explosionRadius++; // Increases the size of the fire cross
                break;

            case ItemType.SpeedIncrease:
                player.GetComponent<PlayerMovement>().speed++; // Makes the character faster
                break;
        }

        // The destruction is now handled in the RPC directly
        // Destroy(gameObject);
    }

    /// <summary> Identifies the collision (trigger) with a body that has the 'Player' tag. </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Server Authority: Somente o Servidor decide quem tocou no item primeiro
        if (!PhotonNetwork.IsMasterClient) return;

        if (other.CompareTag(Constants.Tags.Player))
        {
            PhotonView playerPv = other.GetComponent<PhotonView>();
            if (playerPv != null)
            {
                // Dispara o RPC para todos os computadores aplicarem o buff no jogador específico
                photonView.RPC(Constants.RPC.ApplyItem, RpcTarget.All, playerPv.ViewID);
            }
        }
    }

    [PunRPC]
    private void RPC_ApplyItem(int playerViewID)
    {
        // Encontra o jogador na rede usando o ID único dele
        PhotonView playerPv = PhotonView.Find(playerViewID);
        if (playerPv != null)
        {
            OnItemPickup(playerPv.gameObject); // Aplica o buff visual e lógico
        }
        
        // Apenas o MasterClient remove o objeto de forma sincronizada da rede
        if (PhotonNetwork.IsMasterClient && gameObject != null)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }
}
