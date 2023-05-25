using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GestorDeRede : MonoBehaviourPunCallbacks
{
    public static GestorDeRede instancia;

    private void Awake()
    {
        if (instancia != null && instancia != this)
        {
            gameObject.SetActive(false);
            return;
        }

        instancia = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Conectado com sucesso");
    }

    public void CriarSala(string nomeSala)
    {
        var criou = PhotonNetwork.CreateRoom(nomeSala);
        Debug.Log("Criou: "+criou);
    }

    public void EntrarSala(string nomeSala)
    {
        PhotonNetwork.JoinRoom(nomeSala);
    }

    public void EntrarSala()
    {
        PhotonNetwork.JoinOrCreateRoom("room", new RoomOptions { MaxPlayers = 4 }, TypedLobby.Default);
    }

    public void SairSala()
    {
        PhotonNetwork.LeaveRoom();
    }

    public void MudaNick(string nickname)
    {
        PhotonNetwork.NickName = nickname;
    }

    public string ObterListaDeJogadores()
    {
        var lista = "";
        foreach (var player in PhotonNetwork.PlayerList)
        {
            lista += player.NickName + "\n";
        }
        return lista;
    }

    public bool DonoDaSala()
    {
        return PhotonNetwork.IsMasterClient;
    }

    [PunRPC]
    public void ComecaJogo(string cena)
    {
        PhotonNetwork.LoadLevel(cena);
    }
}
