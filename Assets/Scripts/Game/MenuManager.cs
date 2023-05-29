using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviourPunCallbacks
{
    public static MenuManager instancia;

    [SerializeField] private string nomeDoLevel;
    [SerializeField] private GameObject painelOpcoes;
    [SerializeField] private GameObject painelGameOver;
    [SerializeField] private GameObject painelWin;
    [SerializeField] private GameObject backgroud;

    public static string MenuAtivo;

    private void Awake()
    {
        if (instancia != null && instancia != this)
        {
            gameObject.SetActive(false);
            return;
        }

        instancia = this;
        //DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        MenuAtivo = "";
        AlterarMenu();
    }

    public void Opcoes()
    {
        if (MenuAtivo == "Opcoes")
            MenuAtivo = "";
        else
            MenuAtivo = "Opcoes";

        AlterarMenu();
    }

    public void Win()
    {
        MenuAtivo = "Win";
        AlterarMenu();
    }

    public void GameOver()
    {
        MenuAtivo = "GameOver";
        AlterarMenu();
    }

    public void Minimizar()
    {
        AudioManager.instance.PlaySFX("Click");
        MenuAtivo = "";
        AlterarMenu();
    }

    public void MenuInicial()
    {
        AudioManager.instance.PlaySFX("Click");
        PhotonNetwork.LeaveRoom(this); 
        SceneManager.LoadScene("MenuInicial");
    }

    public void AlterarMenu()
    {
        backgroud.SetActive(MenuAtivo != "");
        painelOpcoes.SetActive(MenuAtivo == "Opcoes");
        painelWin.SetActive(MenuAtivo == "Win");
        painelGameOver.SetActive(MenuAtivo == "GameOver");
    }

}
