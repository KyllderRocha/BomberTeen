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

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("PUN Basics Tutorial/Launcher:OnJoinRandomFailed() was called by PUN. No random room available, so we create one.\nCalling: PhotonNetwork.CreateRoom");

        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 4 }, TypedLobby.Default);
    }

    //public void EntrarSala()
    //{
    //    PhotonNetwork.JoinOrCreateRoom("room", new RoomOptions { MaxPlayers = 4 }, TypedLobby.Default);
    //}

    public void EntrarSala()
    {
        PhotonNetwork.JoinRandomRoom();
    }

    public void SairSala()
    {
        PhotonNetwork.LeaveRoom();
    }

    public void EsconderSala()
    {
        PhotonNetwork.CurrentRoom.IsVisible = false;
    }

    public void MudaNick(string nickname)
    {
        PhotonNetwork.NickName = nickname;
    }

    public string BuscarNick()
    {
        return  PhotonNetwork.NickName;
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

    public int QuantidadeDeJogadores()
    {
        return PhotonNetwork.PlayerList.Length;
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
