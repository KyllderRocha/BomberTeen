using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

/*
 * GAME MANAGER (Gerenciador da Partida)
 * -------------------------------------
 * Este é um Singleton (Instancia única) responsável pelo estado do jogo (Ex: Condição de Vitória).
 * Quando todos os jogadores se conectam, ele orquestra a instanciação dos Prefabs de jogadores
 * espalhados pelos Spawns usando PhotonNetwork.Instantiate.
 * 
 * MELHORIA FUTURA: Desacoplar a checagem de morte de jogadores daqui e usar Eventos (Observer Pattern).
 */
public class GameManager : MonoBehaviourPunCallbacks
{
    /// <summary>
    /// Instância única global (Singleton) do GameManager. Permite acesso estático por outros scripts.
    /// </summary>
    public static GameManager Instancia { get; private set; }

    [Header("Configurações do Jogador")]
    [Tooltip("Caminho/Nome do Prefab do jogador localizado na pasta Resources (exigência do Photon).")]
    [SerializeField] private string _localizacao;
    
    [Tooltip("Lista de locais predefinidos no mapa onde os jogadores vão nascer.")]
    [SerializeField] private Transform[] _spawns;
    private List<Transform> _spawnsUsados;

    public GameObject[] players;

    // Lista oficial de scripts dos jogadores conectados e inicializados.
    private List<MovimentScript> _jogadores;
    public List<MovimentScript> Jogadores { get => _jogadores; private set => _jogadores = value; }
    
    /// <summary>
    /// Contador interno para controlar quantos jogadores terminaram de carregar a cena e foram adicionados.
    /// </summary>
    private int _jogadoresEmJogo = 0;

    /// <summary>
    /// Tecla de atalho usada para invocar o menu de pausa do jogo.
    /// </summary>
    private KeyCode inputKey = KeyCode.Escape;

    private void Awake()
    {
        // Padrão Singleton: Garante que apenas este GameManager exista na cena.
        if (Instancia != null & Instancia != this)
        {
            gameObject.SetActive(false);
            return;
        }
        Instancia = this;
        //DontDestroyOnLoad(gameObject); // Opcional caso o GameManager deva persistir ao recarregar a fase
        
        _jogadores = new List<MovimentScript>();
        _spawnsUsados = new List<Transform>();
    }

    /// <summary>
    /// É chamado no início. Informa pela rede a todos os jogadores (AllBuffered)
    /// que esse jogador local terminou de carregar e está pronto na sala.
    /// </summary>
    public void Start()
    {
        photonView.RPC("AdicionaJogador", RpcTarget.AllBuffered);
    }

    private void Update()
    {
        // Se a tecla for pressionada, abre o menu de opções.
        if (Input.GetKeyDown(inputKey))
        {
            MenuManager.instancia.Opcoes();
        }
    }

    /// <summary>
    /// Confere constantemente a quantidade de jogadores vivos.
    /// Caso reste apenas 1 (ou zero em caso de empate), chama a tela de Vitória/Fim de jogo.
    /// </summary>
    public void CheckWinState()
    {
        // Busca na lista todos os jogadores cujo GameObject ainda está ativo e habilitado.
        int aliveCount = Jogadores.Where(x => x.isActiveAndEnabled).Count();

        if (aliveCount <= 1)
        {
            MenuManager.instancia.Win();
        }

    }

    //private void NewRound()
    //{
    //    MenuPrincipalManager.MenuAtivo = "GameOver";

    //    SceneManager.LoadScene("MenuInicial");
    //}

    /// <summary>
    /// Metódo RPC executado em rede para todos. Aumenta o contador e checa se
    /// todos os clientes aguardados na rede finalmente apareceram para iniciar a partida.
    /// </summary>
    [PunRPC]
    private void AdicionaJogador()
    {
        _jogadoresEmJogo++;
        
        // Verifica se a quantidade de players conectados localmente bate com o contador esperado da rede.
        if (_jogadoresEmJogo == PhotonNetwork.PlayerList.Length)
        {
            CriarJogador();
        }
    }

    /// <summary>
    /// Determina em qual Spawn o jogador deve nascer e instância o Prefab dele através da rede.
    /// </summary>
    private void CriarJogador()
    {
        var position = 0;

        // Itera na lista de jogadores do Photon para encontrar qual é o índice do nosso jogador local
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            if (PhotonNetwork.PlayerList[i] == PhotonNetwork.LocalPlayer)
            {
                position = i;
                //Debug.Log("Nick: " + PhotonNetwork.PlayerList[i].NickName + " P: " + position);
                break;
            }
        }
        
        // Pega o ponto de nascimento no mapa correspondente à posição (ID) do jogador na sala.
        var spwan = _spawns[position];
        
        // Instancia o avatar do jogador na rede do Photon para que todos consigam enxergá-lo.
        var jogadorObj = PhotonNetwork.Instantiate(_localizacao, spwan.position, Quaternion.identity);
        
        var jogador = jogadorObj.GetComponent<MovimentScript>();
        
        // Dispara um RPC instruindo o script do avatar a salvar localmente as informações do seu dono real.
        jogador.photonView.RPC("Inicializa", RpcTarget.All, PhotonNetwork.LocalPlayer);
    }
}
