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
    [SerializeField] private string levelName;
    
    [Tooltip("How many players are needed for the match to start automatically.")]
    [SerializeField] private int playerCount;

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

    /// <summary> Static global variable that dictates which menu screen should be visible at the moment. </summary>
    public static string activeMenu;


    private void Start()
    {
        var nick = BomberNetworkManager.instance.GetNickname();
        if (nick == "Player" || string.IsNullOrEmpty(nick))
        {
            activeMenu = Constants.Menus.Login; // Força o login se ainda não escolheu um nome
        }
        else
        {
            activeMenu = Constants.Menus.MainMenu;
        }
        AudioManager.instance.PlayMusic(Constants.Audio.Theme);
        ChangeMenu();
    }

    private void OnEnable()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnect;
        }
    }
    
    private void OnDisable()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnect;
        }
    }
    
    private void HandleClientConnected(ulong clientId)
    {
        UpdatePlayerList();
        if (BomberNetworkManager.instance.GetPlayerCount() >= playerCount)
        {
           if (NetworkManager.Singleton.IsServer)
           {
               NetworkManager.Singleton.SceneManager.LoadScene(levelName, LoadSceneMode.Single);
           }
        }
    }
    
    private void HandleClientDisconnect(ulong clientId)
    {
        UpdatePlayerList();
    }

    public void ClickStartGame()
    {
        AudioManager.instance.PlaySFX(Constants.Audio.Click);
        if (NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.SceneManager.LoadScene(levelName, LoadSceneMode.Single);
        }
    }

    public void ClickHostGame()
    {
        AudioManager.instance.PlaySFX(Constants.Audio.Click);
        activeMenu = Constants.Menus.Lobby;
        BomberNetworkManager.instance.HostGame();
        ChangeMenu();
        UpdatePlayerList();
    }
    
    public void ClickJoinGame()
    {
        AudioManager.instance.PlaySFX(Constants.Audio.Click);
        activeMenu = Constants.Menus.Lobby;
        BomberNetworkManager.instance.JoinGame();
        ChangeMenu();
        UpdatePlayerList();
    }

    public void ClickOptions()
    {
        AudioManager.instance.PlaySFX(Constants.Audio.Click);
        activeMenu = Constants.Menus.Options;
        ChangeMenu();
    }

    public void ClickReturnMainMenu()
    {
        AudioManager.instance.PlaySFX(Constants.Audio.Click);
        activeMenu = Constants.Menus.MainMenu;
        BomberNetworkManager.instance.LeaveRoom();
        ChangeMenu();
    }

    public void LoginBtn()
    {
        var nick = nickName.text;
        BomberNetworkManager.instance.ChangeNickname(nick);
        BackToMainMenu();
    }
    
    public void BackToMainMenu()
    {
        AudioManager.instance.PlaySFX(Constants.Audio.Click);
        activeMenu = Constants.Menus.MainMenu;
        ChangeMenu();
    }

    public void UpdatePlayerList()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            if (NetworkManager.Singleton.IsServer)
            {
                lobby.text = "Host da Partida\nJogadores conectados: " + NetworkManager.Singleton.ConnectedClientsIds.Count + " / " + playerCount;
            }
            else
            {
                lobby.text = "Você entrou na sala!\nAguardando o Host iniciar...";
            }
        }
        else
        {
            lobby.text = "Conectando...";
        }
    }

    public void ChangeMenu()
    {
        UpdatePanels();
    }

    private void UpdatePanels()
    {
        mainMenuPanel.SetActive(activeMenu == Constants.Menus.MainMenu);
        optionsPanel.SetActive(activeMenu == Constants.Menus.Options);
        gameOverPanel.SetActive(activeMenu == Constants.Menus.GameOver);
        loginPanel.SetActive(activeMenu == Constants.Menus.Login);
        lobbyPanel.SetActive(activeMenu == Constants.Menus.Lobby);
    }

    public void ClickQuit()
    {
        AudioManager.instance.PlaySFX(Constants.Audio.Click);
        Debug.Log("Quit Game");
        Application.Quit();
    }
}
