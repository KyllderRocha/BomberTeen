using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using BomberTeen;

public class MainMenuManager : MonoBehaviourPunCallbacks
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
        // Checks if the player already has a nickname saved/chosen in the session
        var nick = NetworkManager.instance.GetNickname();
        if (PhotonNetwork.IsConnected)
        {
            activeMenu = Constants.Menus.Login; // Forces the player to go through the login screen first
        }
        else
        {
            activeMenu = Constants.Menus.MainMenu;
        }
        AudioManager.instance.PlayMusic(Constants.Audio.Theme);
        ChangeMenu();
    }

    /// <summary>
    /// Automatic Photon event called as soon as the player successfully joins a room.
    /// </summary>
    public override void OnJoinedRoom()
    {
        UpdatePlayerList();

        Debug.Log(NetworkManager.instance.GetPlayerCount());
        
        // Autostart: If the room fills up with the expected maximum capacity, starts the game automatically.
        if (NetworkManager.instance.GetPlayerCount() == playerCount)
        {
           if (PhotonNetwork.IsMasterClient)
           {
               NetworkManager.instance.photonView.RPC(Constants.RPC.StartGame, RpcTarget.All, levelName);
           }
        }
    }

    /// <summary> Photon Event: Someone new entered the same room you were already in. </summary>
    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        UpdatePlayerList();
    }

    /// <summary> Photon Event: Someone left/dropped from the room you are in. </summary>
    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        UpdatePlayerList();
    }

    /// <summary>
    /// Button to force the match to start locally (even if the room is not full).
    /// </summary>
    public void ClickStartGame()
    {
        AudioManager.instance.PlaySFX(Constants.Audio.Click);
        // Only the host (MasterClient) has the authority to load the map and start the logic
        NetworkManager.instance.photonView.RPC(Constants.RPC.StartGame, RpcTarget.All, levelName);
    }

    /// <summary> Matchmaking search button. </summary>
    public void ClickPlay()
    {
        AudioManager.instance.PlaySFX(Constants.Audio.Click);
        activeMenu = Constants.Menus.Lobby; // Changes the UI to the waiting screen (Lobby)
        
        // Connects to a random room or creates a new one
        NetworkManager.instance.JoinRoom();
        ChangeMenu();
    }

    /// <summary> Button to cancel the search and return to the main menu. </summary>
    public void LeaveRoomBtn()
    {
        NetworkManager.instance.LeaveRoom();
        BackToMainMenu();
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
        NetworkManager.instance.LeaveRoom();
        ChangeMenu();
    }

    /// <summary>
    /// Triggered by clicking "Save" on the login screen. Registers the name the user typed.
    /// </summary>
    public void LoginBtn()
    {
        var nick = nickName.text;
        NetworkManager.instance.ChangeNickname(nick);
        BackToMainMenu();
    }

    /// <summary> Gets the formatted string in NetworkManager and updates the visual Lobby text. </summary>
    public void UpdatePlayerList()
    {
        lobby.text = NetworkManager.instance.GetPlayerList();
    }

    /// <summary>
    /// Updates all UI panels. Based on the current value of the 'activeMenu' string, 
    /// only the corresponding panel will be activated (true), while the others are hidden (false).
    /// </summary>
    public void ChangeMenu()
    {
        UpdatePanels();
    }

    private void UpdatePanels()
    {
        // Activates the panel only if its name matches the current activeMenu state
        mainMenuPanel.SetActive(activeMenu == Constants.Menus.MainMenu);
        optionsPanel.SetActive(activeMenu == Constants.Menus.Options);
        gameOverPanel.SetActive(activeMenu == Constants.Menus.GameOver);
        loginPanel.SetActive(activeMenu == Constants.Menus.Login);
        lobbyPanel.SetActive(activeMenu == Constants.Menus.Lobby);
    }

    /// <summary> Closes the game (only works when the build is exported/compiled to a .exe/.apk). </summary>
    public void ClickQuit()
    {
        AudioManager.instance.PlaySFX(Constants.Audio.Click);
        Debug.Log("Quit Game");
        Application.Quit();
    }

}
