using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using BomberTeen;

/*
 * PLAYER SETUP / NETWORK
 * --------------------------------------------------------------
 * This original script was refactored into 4 smaller components to follow the SRP (Single Responsibility) principle.
 * The file was kept so as not to break the compilation of the GameManager, which holds a list of it.
 * Now it serves exclusively to register the local player in the room.
 */
public class PlayerNetworkManager : MonoBehaviourPunCallbacks
{
    private Player _photonPlayer;
    
    /// <summary> Exclusive numerical ID of this player provided by Photon. </summary>
    public int id;

    /// <summary> Receives from GameManager the data of which real user controls this avatar. </summary>
    [PunRPC]
    public void Initialize(Player player)
    {
        _photonPlayer = player;
        id = player.ActorNumber;
        
        GameManager.instance.players.Add(this);
    }
}
