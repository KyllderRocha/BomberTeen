using Unity.Netcode;
using UnityEngine;
using BomberTeen;

/*
 * PLAYER SETUP / NETWORK
 * --------------------------------------------------------------
 * This original script was refactored into 4 smaller components to follow the SRP (Single Responsibility) principle.
 * The file was kept so as not to break the compilation of the GameManager, which holds a list of it.
 * Now it serves exclusively to register the local player in the room.
 */
public class PlayerNetworkManager : NetworkBehaviour
{
    /// <summary> Exclusive numerical ID of this player provided by Netcode. </summary>
    public ulong id;

    public override void OnNetworkSpawn()
    {
        id = OwnerClientId;
        if (GameManager.instance != null && GameManager.instance.players != null)
        {
            if (!GameManager.instance.players.Contains(this))
            {
                GameManager.instance.players.Add(this);
            }
        }
    }

    public override void OnNetworkDespawn()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.players.Remove(this);
        }
    }
}
