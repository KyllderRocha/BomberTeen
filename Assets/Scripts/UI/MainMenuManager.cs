using Unity.Netcode;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using BomberTeen;

public class MainMenuManager : MonoBehaviour
{
    [Header("Game Settings")]
    [Tooltip("Exact name of the scene (Level) that will be loaded when starting the match.")]
    [SerializeField] private string levelName = "GameScene";
    
    [Tooltip("How many players are needed for the match to start automatically.")]
    [SerializeField] private int playerCount = 4;

    [Header("UI Panels (Screens)")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private GameObject lobbyPanel;
    
    [Header("Text Elements")]
    [Tooltip("Text field where the user types their nickname.")]
    [SerializeField] private TextMeshProUGUI nickName;
    [Tooltip("Text field listing who is already waiting in the room.")]
    [SerializeField] private TextMeshProUGUI lobby;

    [Header("Lobby Controls")]
    [Tooltip("Botão para iniciar a partida manualmente (visível apenas para o Host).")]
    [SerializeField] private GameObject startMatchButton;

    /// <summary> Static global variable that dictates which menu screen should be visible at the moment. </summary>
    public static string activeMenu;

    private bool callbacksRegistered = false;

    private void Awake()
    {
        if (startMatchButton == null && lobbyPanel != null)
        {
            var playTransform = lobbyPanel.transform.Find("PlayBtn");
            if (playTransform != null)
            {
                startMatchButton = playTransform.gameObject;
            }
        }
    }

    private void Start()
    {
        var nick = BomberNetworkManager.instance != null ? BomberNetworkManager.instance.GetNickname() : "Player";
        if (nick == "Player" || string.IsNullOrEmpty(nick))
        {
            activeMenu = Constants.Menus.Login; // Força o login se ainda não escolheu um nome
        }
        else
        {
            activeMenu = Constants.Menus.MainMenu;
        }

        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayMusic(Constants.Audio.Theme);
        }

        ChangeMenu();
        RegisterNetworkCallbacks();
    }

    private void OnEnable()
    {
        RegisterNetworkCallbacks();
    }
    
    private void OnDisable()
    {
        UnregisterNetworkCallbacks();
    }

    private void OnDestroy()
    {
        UnregisterNetworkCallbacks();
    }

    private void RegisterNetworkCallbacks()
    {
        if (NetworkManager.Singleton != null && !callbacksRegistered)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnect;
            callbacksRegistered = true;
        }
    }

    private void UnregisterNetworkCallbacks()
    {
        if (NetworkManager.Singleton != null && callbacksRegistered)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnect;
            callbacksRegistered = false;
        }
    }
    
    private void HandleClientConnected(ulong clientId)
    {
        UpdatePlayerList();
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
        {
            if (BomberNetworkManager.instance != null && BomberNetworkManager.instance.GetPlayerCount() >= playerCount)
            {
                NetworkManager.Singleton.SceneManager.LoadScene(levelName, LoadSceneMode.Single);
            }
        }
    }
    
    private void HandleClientDisconnect(ulong clientId)
    {
        UpdatePlayerList();
    }

    /// <summary>
    /// Ponto de entrada inteligente: se já houver um Host rodando nesta máquina na porta 7777,
    /// entra automaticamente como Cliente. Se não houver, cria a sala como Host.
    /// </summary>
    public void ClickPlay()
    {
        if (AudioManager.instance != null) AudioManager.instance.PlaySFX(Constants.Audio.Click);
        activeMenu = Constants.Menus.Lobby;
        ChangeMenu();
        RegisterNetworkCallbacks();

        if (BomberNetworkManager.instance != null)
        {
            if (BomberNetworkManager.instance.IsHostRunningLocally())
            {
                Debug.Log("[BomberTeen] Host local encontrado. Entrando como Cliente...");
                BomberNetworkManager.instance.JoinGame("127.0.0.1");
            }
            else
            {
                Debug.Log("[BomberTeen] Nenhum Host local encontrado. Criando sala como Host...");
                BomberNetworkManager.instance.HostGame();
            }
        }

        UpdatePlayerList();
    }

    /// <summary> Alias chamado pelo RestartBtn na tela de GameOver. </summary>
    public void Play() => ClickPlay();

    public void ClickStartGame()
    {
        if (AudioManager.instance != null) AudioManager.instance.PlaySFX(Constants.Audio.Click);
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.SceneManager.LoadScene(levelName, LoadSceneMode.Single);
        }
    }

    public void ClickHostGame()
    {
        if (AudioManager.instance != null) AudioManager.instance.PlaySFX(Constants.Audio.Click);
        activeMenu = Constants.Menus.Lobby;
        ChangeMenu();
        RegisterNetworkCallbacks();

        if (BomberNetworkManager.instance != null)
        {
            BomberNetworkManager.instance.HostGame();
        }

        UpdatePlayerList();
    }
    
    public void ClickJoinGame()
    {
        if (AudioManager.instance != null) AudioManager.instance.PlaySFX(Constants.Audio.Click);
        activeMenu = Constants.Menus.Lobby;
        ChangeMenu();
        RegisterNetworkCallbacks();

        if (BomberNetworkManager.instance != null)
        {
            BomberNetworkManager.instance.JoinGame();
        }

        UpdatePlayerList();
    }

    public void ClickOptions()
    {
        if (AudioManager.instance != null) AudioManager.instance.PlaySFX(Constants.Audio.Click);
        activeMenu = Constants.Menus.Options;
        ChangeMenu();
    }

    public void ClickReturnMainMenu()
    {
        if (AudioManager.instance != null) AudioManager.instance.PlaySFX(Constants.Audio.Click);
        activeMenu = Constants.Menus.MainMenu;
        if (BomberNetworkManager.instance != null)
        {
            BomberNetworkManager.instance.LeaveRoom();
        }
        ChangeMenu();
    }

    public void LoginBtn()
    {
        if (nickName != null && BomberNetworkManager.instance != null)
        {
            var nick = nickName.text;
            BomberNetworkManager.instance.ChangeNickname(nick);
        }
        BackToMainMenu();
    }
    
    public void BackToMainMenu()
    {
        if (AudioManager.instance != null) AudioManager.instance.PlaySFX(Constants.Audio.Click);
        activeMenu = Constants.Menus.MainMenu;
        ChangeMenu();
    }

    public void UpdatePlayerList()
    {
        if (lobby == null) return;

        if (NetworkManager.Singleton == null)
        {
            lobby.text = "Rede não inicializada.";
            return;
        }

        if (NetworkManager.Singleton.IsServer)
        {
            int connected = NetworkManager.Singleton.ConnectedClientsIds.Count;
            string localIp = BomberNetworkManager.GetLocalIPAddress();
            lobby.text = $"Host da Partida\nIP da Sala: {localIp}\nJogadores: {connected} / {playerCount}\n" +
                         (connected >= playerCount ? "Sala cheia! Iniciando..." : "Aguardando outros jogadores...");
            
            if (startMatchButton != null)
            {
                startMatchButton.SetActive(true);
            }
        }
        else if (NetworkManager.Singleton.IsConnectedClient)
        {
            lobby.text = "Você entrou na sala!\nAguardando o Host iniciar a partida...";
            
            if (startMatchButton != null)
            {
                startMatchButton.SetActive(false);
            }
        }
        else if (NetworkManager.Singleton.IsClient)
        {
            lobby.text = $"Conectando ao Host...\nAguarde um momento.";
            
            if (startMatchButton != null)
            {
                startMatchButton.SetActive(false);
            }
        }
        else
        {
            lobby.text = "Não conectado à sala.\nVerifique se o Host foi criado.";
            
            if (startMatchButton != null)
            {
                startMatchButton.SetActive(false);
            }
        }
    }

    public void ChangeMenu()
    {
        UpdatePanels();
    }

    private void UpdatePanels()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(activeMenu == Constants.Menus.MainMenu);
        if (optionsPanel != null) optionsPanel.SetActive(activeMenu == Constants.Menus.Options);
        if (gameOverPanel != null) gameOverPanel.SetActive(activeMenu == Constants.Menus.GameOver);
        if (loginPanel != null) loginPanel.SetActive(activeMenu == Constants.Menus.Login);
        if (lobbyPanel != null) lobbyPanel.SetActive(activeMenu == Constants.Menus.Lobby);
    }

    public void ClickQuit()
    {
        if (AudioManager.instance != null) AudioManager.instance.PlaySFX(Constants.Audio.Click);
        Debug.Log("Quit Game");
        Application.Quit();
    }
}
