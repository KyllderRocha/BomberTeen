using Unity.Netcode;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using BomberTeen;
using UnityEngine.UI;

/// <summary>
/// Manager responsible for opening/closing the Pause (Options), Win, and Defeat screens
/// during gameplay (Different from MainMenuManager which acts only in the Lobby).
/// </summary>
public class MenuManager : MonoBehaviour
{
    public static MenuManager instance;

    [Header("Navigation")]
    [Tooltip("Name of the main menu base scene.")]
    [SerializeField] private string levelName;
    
    [Header("Game Screens")]
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject background;

    /// <summary> Global flag determining which screen is currently open. If empty "", plays normally. </summary>
    public static string activeMenu;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            gameObject.SetActive(false);
            return;
        }

        instance = this;
    }

    private void OnEnable()
    {
        PlayerStatus.OnPlayerDied += HandlePlayerDeath; // Subscribes to the death radio
    }

    private void OnDisable()
    {
        PlayerStatus.OnPlayerDied -= HandlePlayerDeath; // Unsubscribes
    }

    /// <summary> Observer callback. Only shows the Game Over screen if you are the owner of the character that died. </summary>
    private void HandlePlayerDeath(PlayerStatus deadPlayer)
    {
        if (deadPlayer.IsOwner) // Netcode Owner Check
        {
            activeMenu = Constants.Menus.GameOver;
            UpdatePanels();
        }
    }

    private void Start()
    {
        activeMenu = "";
        UpdatePanels();
    }

    private void Update()
    {
        // Se a partida não acabou e o jogador apertar ESC
        if (activeMenu != Constants.Menus.GameOver && activeMenu != Constants.Menus.Win && Input.GetKeyDown(KeyCode.Escape))
        {
            if (activeMenu == Constants.Menus.Options)
                activeMenu = ""; // Clears the flag, returning to the game
            else
                activeMenu = Constants.Menus.Options;
            
            UpdatePanels();
        }
    } 

    /// <summary> Called by GameManager when the player is the last one alive in the match. </summary>
    public void WinGame()
    {
        activeMenu = Constants.Menus.Win;
        UpdatePanels();
    }

    public void ClickReturnToGame()
    {
        AudioManager.instance.PlaySFX(Constants.Audio.Click);
        activeMenu = "";
        UpdatePanels();
    }
    
    public void ShowOptions()
    {
        AudioManager.instance.PlaySFX(Constants.Audio.Click);
        activeMenu = Constants.Menus.Options;
        UpdatePanels();
    }

    public void ClickReturnToMenu()
    {
        AudioManager.instance.PlaySFX(Constants.Audio.Click);
        BomberNetworkManager.instance.LeaveRoom();
        SceneManager.LoadScene(Constants.Menus.MainMenuScene);
    }

    private void UpdatePanels()
    {
        background.SetActive(activeMenu != "");
        optionsPanel.SetActive(activeMenu == Constants.Menus.Options);
        winPanel.SetActive(activeMenu == Constants.Menus.Win);
        gameOverPanel.SetActive(activeMenu == Constants.Menus.GameOver);
    }

}
