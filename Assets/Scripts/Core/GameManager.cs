using Unity.Netcode;
using System.Collections.Generic;
using System.Linq;
using BomberTeen;
using UnityEngine;

/*
 * GAME MANAGER
 * -------------------------------------
 * This is a Singleton responsible for the game state (e.g. Win Condition).
 * In NGO, only the Server is allowed to spawn NetworkObjects. 
 * Therefore, the Server listens for connections and spawns the Player Prefab at the correct location.
 */
public class GameManager : NetworkBehaviour
{
    public static GameManager instance { get; private set; }

    [Header("Player Settings")]
    [Tooltip("The Player Prefab that will be spawned over the network.")]
    [SerializeField] private GameObject playerPrefabNetwork;
    
    [Tooltip("List of predefined locations on the map where players will spawn.")]
    [SerializeField] private Transform[] spawns;

    // Official list of initialized and connected player scripts.
    private List<PlayerNetworkManager> _players;
    public List<PlayerNetworkManager> players { get => _players; set => _players = value; }
    
    private int playersInGame = 0;
    private KeyCode inputKey = KeyCode.Escape;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            gameObject.SetActive(false);
            return;
        }
        instance = this;
        _players = new List<PlayerNetworkManager>();
    }

    public override void OnNetworkSpawn()
    {
        PlayerStatus.OnPlayerDied += HandlePlayerDeath; // Subscribes to the Observer

        if (IsServer)
        {
            // Spawn players that are already connected (Host + early clients)
            foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                CreatePlayer(clientId);
            }
            
            // Listen for future clients connecting
            NetworkManager.Singleton.OnClientConnectedCallback += CreatePlayer;
            NetworkManager.Singleton.OnClientDisconnectCallback += HandlePlayerDisconnect;
        }
    }

    public override void OnNetworkDespawn()
    {
        PlayerStatus.OnPlayerDied -= HandlePlayerDeath; 

        if (IsServer && NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= CreatePlayer;
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandlePlayerDisconnect;
        }
    }

    private void HandlePlayerDeath(PlayerStatus deadPlayer)
    {
        CheckWinState();
    }

    private void Update()
    {
        // If the key is pressed, opens the options menu.
        if (Input.GetKeyDown(inputKey))
        {
            // O MenuManager antigo cuidava disso. (Deixamos como estava)
        }
    }

    public void CheckWinState()
    {
        // Searches the list for all players whose GameObject is still active and enabled.
        int aliveCount = players.Where(x => x.isActiveAndEnabled).Count();

        if (aliveCount <= 1 && players.Count > 1) // Garante que a partida tinha mais de 1 antes de dar vitória instantanea
        {
            MenuManager.instance.WinGame();
        }
    }

    /// <summary>
    /// Determines in which Spawn the player should be born and instantiates their Prefab over the network.
    /// In NGO, only the Server can do this.
    /// </summary>
    private void CreatePlayer(ulong clientId)
    {
        Transform spawn = transform; // Fallback para a própria posição do GameManager se esquecerem de configurar
        if (spawns != null && spawns.Length > 0)
        {
            spawn = spawns[playersInGame % spawns.Length];
        }
        
        playersInGame++;
        
        // Instantiates the player's avatar on the network so everyone can see it.
        var playerObj = Instantiate(playerPrefabNetwork, spawn.position, Quaternion.identity);
        
        // Spawns with ownership assigned to the specific client
        playerObj.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
    }
    
    private void HandlePlayerDisconnect(ulong clientId)
    {
        CheckWinState();
    }
}
