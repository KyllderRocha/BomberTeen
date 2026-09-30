using Unity.Netcode;
using System.Collections.Generic;
using System.Linq;
using BomberTeen;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    public Transform[] GetSpawns() => spawns;

    // Official list of initialized and connected player scripts.
    private List<PlayerNetworkManager> _players;
    public List<PlayerNetworkManager> players { get => _players; set => _players = value; }
    
    private int playersInGame = 0;
    private int maxPlayersInMatch = 0;
    private bool gameEnded = false;
    private readonly HashSet<ulong> spawnedClients = new HashSet<ulong>();

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
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.SceneManager != null)
            {
                NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += HandleSceneLoadCompleted;
                NetworkManager.Singleton.SceneManager.OnSynchronizeComplete += HandleClientSynchronized;
            }

            NetworkManager.Singleton.OnClientDisconnectCallback += HandlePlayerDisconnect;

            // Fallback for editor PlayMode testing (where SceneManager.LoadScene was not called)
            StartCoroutine(SpawnInitialPlayersFallback());
        }
    }

    public override void OnNetworkDespawn()
    {
        PlayerStatus.OnPlayerDied -= HandlePlayerDeath; 

        if (IsServer && NetworkManager.Singleton != null)
        {
            if (NetworkManager.Singleton.SceneManager != null)
            {
                NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= HandleSceneLoadCompleted;
                NetworkManager.Singleton.SceneManager.OnSynchronizeComplete -= HandleClientSynchronized;
            }

            NetworkManager.Singleton.OnClientDisconnectCallback -= HandlePlayerDisconnect;
        }
    }

    private void HandleSceneLoadCompleted(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        if (!IsServer) return;
        Debug.Log($"[GameManager] OnLoadEventCompleted para cena '{sceneName}'. Clientes: {string.Join(", ", clientsCompleted)}");

        foreach (var clientId in clientsCompleted)
        {
            CreatePlayer(clientId);
        }
    }

    private void HandleClientSynchronized(ulong clientId)
    {
        if (!IsServer) return;
        Debug.Log($"[GameManager] OnSynchronizeComplete para ClientId: {clientId}");
        CreatePlayer(clientId);
    }

    private System.Collections.IEnumerator SpawnInitialPlayersFallback()
    {
        // Aguarda meio segundo caso a transição via SceneManager já esteja gerenciando o spawn
        yield return new WaitForSeconds(0.5f);

        if (IsServer && NetworkManager.Singleton != null)
        {
            foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                CreatePlayer(clientId);
            }
        }
    }

    private void HandlePlayerDeath(PlayerStatus deadPlayer)
    {
        if (IsServer)
        {
            CheckWinState();
        }
    }

    public void CheckWinState()
    {
        if (!IsServer || gameEnded) return;

        maxPlayersInMatch = Mathf.Max(maxPlayersInMatch, players.Count);

        // Count how many players are still alive
        var alivePlayers = players.Where(p => p != null && p.GetComponent<PlayerStatus>() != null && !p.GetComponent<PlayerStatus>().isDead).ToList();

        // Only evaluate victory if the match started with more than 1 player
        if (maxPlayersInMatch > 1 && alivePlayers.Count <= 1)
        {
            gameEnded = true;
            ulong winnerClientId = alivePlayers.Count == 1 ? alivePlayers[0].OwnerClientId : ulong.MaxValue;
            AnnounceGameResultRpc(winnerClientId);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void AnnounceGameResultRpc(ulong winnerClientId)
    {
        if (MenuManager.instance == null || NetworkManager.Singleton == null) return;

        ulong localId = NetworkManager.Singleton.LocalClientId;

        if (winnerClientId != ulong.MaxValue && winnerClientId == localId)
        {
            MenuManager.instance.WinGame();
        }
        else
        {
            MenuManager.instance.GameOver();
        }
    }

    /// <summary>
    /// Determines in which Spawn the player should be born and instantiates their Prefab over the network.
    /// In NGO, only the Server can do this.
    /// </summary>
    private void CreatePlayer(ulong clientId)
    {
        if (spawnedClients.Contains(clientId)) return;

        // Avoid spawning duplicate player objects if already present
        if (NetworkManager.Singleton != null && 
            NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client) && 
            client.PlayerObject != null)
        {
            spawnedClients.Add(clientId);
            return;
        }

        Transform spawn = transform;
        if (spawns != null && spawns.Length > 0)
        {
            spawn = spawns[playersInGame % spawns.Length];
        }
        
        playersInGame++;
        spawnedClients.Add(clientId);
        
        var playerObj = Instantiate(playerPrefabNetwork, spawn.position, Quaternion.identity);
        playerObj.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
    }
    
    private void HandlePlayerDisconnect(ulong clientId)
    {
        if (IsServer)
        {
            CheckWinState();
        }
    }
}
