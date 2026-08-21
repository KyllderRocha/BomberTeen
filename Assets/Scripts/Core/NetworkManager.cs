using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * NETWORK MANAGER
 * -----------------------------------
 * This script inherits from MonoBehaviourPunCallbacks, which allows listening to native Photon PUN 2 events.
 * It manages the initial connection to the server, room creation, and joining.
 * 
 * NOTE FOR UNITY 6: Photon PUN 2 is supported, but it's considered legacy. In future projects 
 * using this Unity version, consider migrating to Photon Fusion or Netcode for GameObjects (NGO).
 */
public class NetworkManager : MonoBehaviourPunCallbacks
{
    /// <summary>
    /// Singleton to ensure the Manager exists in all scenes and manages the network globally.
    /// </summary>
    public static NetworkManager instance;

    private void Awake()
    {
        // If a manager already exists, destroy the copies created when returning to the initial screen
        if (instance != null && instance != this)
        {
            gameObject.SetActive(false);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject); // Prevents the object from being destroyed during scene transitions
    }

    private void Start()
    {
        // Starts the connection process with the Photon Master Server using the project settings
        PhotonNetwork.ConnectUsingSettings();
    }

    /// <summary>
    /// Native Photon callback called automatically when the game successfully connects to the cloud server.
    /// </summary>
    public override void OnConnectedToMaster()
    {
        Debug.Log("Successfully connected");
    }

    /// <summary>
    /// Callback fired if the player tries to join a random room and cannot find any available.
    /// In this case, they take on the role of Host (Master Client) and create a new room.
    /// </summary>
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("PUN Basics Tutorial/Launcher:OnJoinRandomFailed() was called by PUN. No random room available, so we create one.\nCalling: PhotonNetwork.CreateRoom");

        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 4 }, TypedLobby.Default);
    }

    /// <summary>
    /// Attempts to throw the user into the first room that Photon finds open.
    /// </summary>
    public void JoinRoom()
    {
        PhotonNetwork.JoinRandomRoom();
    }

    /// <summary>
    /// Disconnects the player from the current room and returns to the global lobby.
    /// </summary>
    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    /// <summary>
    /// Makes the room invisible for matchmaking. Used when the room reaches the player limit 
    /// to prevent "ghosts" from trying to connect to the ongoing match.
    /// </summary>
    public void HideRoom()
    {
        PhotonNetwork.CurrentRoom.IsVisible = false;
    }

    /// <summary> Changes the local nickname that will appear on the network. </summary>
    public void ChangeNickname(string nickname)
    {
        PhotonNetwork.NickName = nickname;
    }

    /// <summary> Retrieves the currently configured nickname. </summary>
    public string GetNickname()
    {
        return  PhotonNetwork.NickName;
    }

    /// <summary>
    /// Scans all players inside the current room and builds a string with the names
    /// to display in the Lobby interface.
    /// </summary>
    public string GetPlayerList()
    {
        var lista = "";
        foreach (var player in PhotonNetwork.PlayerList)
        {
            lista += player.NickName + "\n";
        }
        return lista;
    }

    /// <summary> Returns the number of players connected in the same room. </summary>
    public int GetPlayerCount()
    {
        return PhotonNetwork.PlayerList.Length;
    }

    /// <summary> Checks if this client has authority over the room (usually whoever created it). </summary>
    public bool IsRoomOwner()
    {
        return PhotonNetwork.IsMasterClient;
    }

    /// <summary>
    /// RPC that sends the command to all machines on the network to load the next synchronized phase (scene).
    /// </summary>
    [PunRPC]
    public void StartGame(string scene)
    {
        // It's important to use PhotonNetwork.LoadLevel instead of SceneManager, 
        // as it ensures the network pauses everything and doesn't disconnect anyone during the transition.
        PhotonNetwork.LoadLevel(scene);
    }
}
