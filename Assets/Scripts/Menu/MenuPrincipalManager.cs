using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuPrincipalManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private string nomeDoLevel;
    [SerializeField] private GameObject painelMenuInicial;
    [SerializeField] private GameObject painelOpcoes;
    [SerializeField] private GameObject painelGameOver;
    [SerializeField] private GameObject painelLogin;
    [SerializeField] private GameObject painelLobby;
    [SerializeField] private TextMeshProUGUI nickName;
    [SerializeField] private TextMeshProUGUI lobby;
    [SerializeField] private int quantidadeDeJogadores;

    public static string MenuAtivo;


    private void Start()
    {
        var nick = GestorDeRede.instancia.BuscarNick();
        if (string.IsNullOrEmpty(nick))
            MenuAtivo = "Login";
        else
            MenuAtivo = "MenuInicial";

        AudioManager.instance.PlayMusic("Theme");
        AlterarMenu();
    }

    public override void OnJoinedRoom()
    {
        AtualizarJogadores();

        Debug.Log(GestorDeRede.instancia.QuantidadeDeJogadores());
        if (GestorDeRede.instancia.QuantidadeDeJogadores() == quantidadeDeJogadores)
        {
            GestorDeRede.instancia.EsconderSala();
            GestorDeRede.instancia.photonView.RPC("ComecaJogo", RpcTarget.All, nomeDoLevel);
        }
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        AtualizarJogadores();
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        AtualizarJogadores();
    }

    public void Jogar()
    {
        AudioManager.instance.PlaySFX("Click");
        GestorDeRede.instancia.EsconderSala();
        GestorDeRede.instancia.photonView.RPC("ComecaJogo", RpcTarget.All, nomeDoLevel);
    }

    public void EntrarSala()
    {
        GestorDeRede.instancia.EntrarSala();
        AudioManager.instance.PlaySFX("Click");
        MenuAtivo = "Lobby";
        AlterarMenu();
    }

    public void SairSala()
    {
        GestorDeRede.instancia.SairSala();
        VoltarMenuPrincipal();
    }

    public void AbrirOpcoes()
    {
        AudioManager.instance.PlaySFX("Click");
        MenuAtivo = "Opcoes";
        AlterarMenu();
    }

    public void VoltarMenuPrincipal()
    {
        AudioManager.instance.PlaySFX("Click");
        MenuAtivo = "MenuInicial";
        AlterarMenu();
    }

    public void Logar()
    {
        var nick = nickName.text;
        GestorDeRede.instancia.MudaNick(nick);
        VoltarMenuPrincipal();
    }

    public void AtualizarJogadores()
    {
        lobby.text = GestorDeRede.instancia.ObterListaDeJogadores();
    }

    public void AlterarMenu()
    {
        painelMenuInicial.SetActive(MenuAtivo == "MenuInicial");
        painelOpcoes.SetActive(MenuAtivo == "Opcoes");
        painelGameOver.SetActive(MenuAtivo == "GameOver");
        painelLogin.SetActive(MenuAtivo == "Login");
        painelLobby.SetActive(MenuAtivo == "Lobby");
    }

    public void SairJogo()
    {
        AudioManager.instance.PlaySFX("Click");
        Debug.Log("Sair do Jogo");
        Application.Quit();
    }

}
