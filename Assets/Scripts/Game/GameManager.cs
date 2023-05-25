using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instancia { get; private set; }

    [SerializeField] private string _localizacao;
    [SerializeField] private Transform[] _spawns;
    private List<Transform> _spawnsUsados;

    public GameObject[] players;
    private List<MovimentScript> _jogadores;
    public List<MovimentScript> Jogadores { get => _jogadores; private set => _jogadores = value; }
    private int _jogadoresEmJogo = 0;

    private void Awake()
    {
        if (Instancia != null & Instancia != this)
        {
            gameObject.SetActive(false);
            return;
        }
        Instancia = this;
        DontDestroyOnLoad(gameObject);
        _jogadores = new List<MovimentScript>();
        _spawnsUsados = new List<Transform>();
    }

    public void Start()
    {
        photonView.RPC("AdicionaJogador", RpcTarget.AllBuffered);
    }


    public void CheckWinState()
    {
        int aliveCount = 0;

        foreach (GameObject player in players)
        {
            if (player.activeSelf)
            {
                aliveCount++;
            }
        }

        if (aliveCount <= 1)
        {
            Invoke(nameof(NewRound), 1.5f);
        }

    }

    private void NewRound()
    {
        MenuPrincipalManager.MenuAtivo = "GameOver";

        SceneManager.LoadScene("MenuInicial");
    }

    [PunRPC]
    private void AdicionaJogador()
    {
        _jogadoresEmJogo++;
        if (_jogadoresEmJogo == PhotonNetwork.PlayerList.Length)
        {
            CriarJogador();
        }
    }

    private void CriarJogador()
    {
        var position = 0;

        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            if (PhotonNetwork.PlayerList[i] == PhotonNetwork.LocalPlayer)
            {
                position = i;
                //Debug.Log("Nick: " + PhotonNetwork.PlayerList[i].NickName + " P: " + position);
                break;
            }
        }
        var spwan = _spawns[position];
        var jogadorObj = PhotonNetwork.Instantiate(_localizacao, spwan.position, Quaternion.identity);
        var jogador = jogadorObj.GetComponent<MovimentScript>();
        jogador.photonView.RPC("Inicializa", RpcTarget.All, PhotonNetwork.LocalPlayer);
    }
}
