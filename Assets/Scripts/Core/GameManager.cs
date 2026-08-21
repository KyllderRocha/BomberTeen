using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BomberTeen;
using UnityEngine;
using UnityEngine.SceneManagement;

/*
 * GAME MANAGER
 * -------------------------------------
 * This is a Singleton responsible for the game state (e.g. Win Condition).
 * When all players connect, it orchestrates the instantiation of player Prefabs
 * across the Spawns using PhotonNetwork.Instantiate.
 * 
 * FUTURE IMPROVEMENT: Decouple player death checking from here and use Events (Observer Pattern).
 */
public class GameManager : MonoBehaviourPunCallbacks
{
    /// <summary>
    /// Global unique instance (Singleton) of the GameManager. Allows static access by other scripts.
    /// </summary>
    public static GameManager instance { get; private set; }

    [Header("Player Settings")]
    [Tooltip("Path/Name of the player Prefab located in the Resources folder (Photon requirement).")]
    [SerializeField] private string prefabLocation;
    
    [Tooltip("List of predefined locations on the map where players will spawn.")]
    [SerializeField] private Transform[] spawns;
    private List<Transform> usedSpawns;

    // Official list of initialized and connected player scripts.
    private List<PlayerNetworkManager> _players;
    public List<PlayerNetworkManager> players { get => _players; private set => _players = value; }
    
    /// <summary>
    /// Internal counter to track how many players have finished loading the scene and been added.
    /// </summary>
    private int playersInGame = 0;

    /// <summary>
    /// Shortcut key used to invoke the game pause menu.
    /// </summary>
    private KeyCode inputKey = KeyCode.Escape;

    private void Awake()
    {
        // Singleton Pattern: Ensures only this GameManager exists in the scene.
        if (instance != null & instance != this)
        {
            gameObject.SetActive(false);
            return;
        }
        instance = this;
        
        _players = new List<PlayerNetworkManager>();
        usedSpawns = new List<Transform>();
    }

    public override void OnEnable()
    {
        base.OnEnable();
        PlayerStatus.OnPlayerDied += HandlePlayerDeath; // Subscribes to the Observer
    }

    public override void OnDisable()
    {
        base.OnDisable();
        PlayerStatus.OnPlayerDied -= HandlePlayerDeath; // Unsubscribes from the Observer
    }

    /// <summary> Observer callback called instantly when the death megaphone is triggered. </summary>
    private void HandlePlayerDeath(PlayerStatus deadPlayer)
    {
        CheckWinState();
    }

    /// <summary>
    /// Called at the beginning. Informs all players over the network (AllBuffered)
    /// that this local player has finished loading and is ready in the room.
    /// </summary>
    public void Start()
    {
        photonView.RPC(Constants.RPC.AddPlayer, RpcTarget.AllBuffered);
    }

    private void Update()
    {
        // If the key is pressed, opens the options menu.
        if (Input.GetKeyDown(inputKey))
        {
            MenuManager.instance.ShowOptions();
        }
    }

    /// <summary>
    /// Constantly checks the amount of alive players.
    /// If only 1 remains (or zero in case of a draw), calls the Win/Game Over screen.
    /// </summary>
    public void CheckWinState()
    {
        // Searches the list for all players whose GameObject is still active and enabled.
        int aliveCount = players.Where(x => x.isActiveAndEnabled).Count();

        if (aliveCount <= 1)
        {
            MenuManager.instance.Win();
        }

    }

    //private void NewRound()
    //{
    //    MenuPrincipalManager.MenuAtivo = "GameOver";

    //    SceneManager.LoadScene("MenuInicial");
    //}

    /// <summary>
    /// RPC method executed over the network for all. Increases the counter and checks if
    /// all expected clients on the network have finally appeared to start the match.
    /// </summary>
    [PunRPC]
    private void AddPlayer()
    {
        playersInGame++;
        
        // Checks if the amount of locally connected players matches the expected counter from the network.
        if (playersInGame == PhotonNetwork.PlayerList.Length)
        {
            CreatePlayer();
        }
    }

    /// <summary>
    /// Determines in which Spawn the player should be born and instantiates their Prefab over the network.
    /// </summary>
    private void CreatePlayer()
    {
        var position = 0;

        // Iterates through the Photon player list to find the index of our local player
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            if (PhotonNetwork.PlayerList[i] == PhotonNetwork.LocalPlayer)
            {
                position = i;
                break;
            }
        }
        
        // Gets the spawn point on the map corresponding to the player's position (ID) in the room.
        var spawn = spawns[position];
        
        // Instantiates the player's avatar on the Photon network so everyone can see it.
        var playerObj = PhotonNetwork.Instantiate(prefabLocation, spawn.position, Quaternion.identity);
        
        var player = playerObj.GetComponent<PlayerNetworkManager>();
        
        // Fires an RPC instructing the avatar's script to locally save its true owner's information.
        player.photonView.RPC(Constants.RPC.InitializePlayer, RpcTarget.All, PhotonNetwork.LocalPlayer);
    }
    
    
    /// <summary>
    /// Event triggered natively by the engine when someone gives up and leaves the match.
    /// </summary>
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        CheckWinState();
    }
}
