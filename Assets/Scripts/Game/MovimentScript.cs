using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * MOVIMENT SCRIPT (Controle de Movimento e Sincronização Visual)
 * --------------------------------------------------------------
 * Lida com o input do jogador e move o Rigidbody2D em FixedUpdate.
 * A movimentação é local (apenas quem tem 'photonView.IsMine' se move fisicamente), 
 * e a animação é enviada aos outros clientes via RPC ('changeSprite').
 * 
 * MELHORIA FUTURA: Para o Unity 6, considere o uso da interface IPunObservable para serializar 
 * posição e animações de maneira suave (Client-Side Prediction), minimizando os gargalos de RPCs constantes.
 */
public class MovimentScript : MonoBehaviourPunCallbacks
{
    /// <summary>
    /// Referência local para o motor de física do corpo.
    /// </summary>
    public Rigidbody2D Rigidbody { get; private set; }
    
    // Vetor que guarda para qual lado o personagem está apontando/andando.
    private Vector2 direction = Vector2.down;
    
    [Tooltip("Velocidade de movimento do jogador.")]
    public float speed = 5f;

    [Header("Controles")]
    public KeyCode inputUp = KeyCode.W;
    public KeyCode inputDown = KeyCode.S;
    public KeyCode inputLeft = KeyCode.A;
    public KeyCode inputRight = KeyCode.D;

    [Header("Animações Visuais")]
    public AnimatedSpriteRenderer spriteRendererUp; 
    public AnimatedSpriteRenderer spriteRendererDown; 
    public AnimatedSpriteRenderer spriteRendererLeft; 
    public AnimatedSpriteRenderer spriteRendererRight; 
    public AnimatedSpriteRenderer spriteRendererDeath; 
    
    // Guarda o script de renderização que está sendo usado no momento (ex: Animação de Andar Pra Baixo)
    private AnimatedSpriteRenderer activeSpriteRenderer;

    // Referência de rede que diz a qual player real esse avatar pertence.
    private Player _photonPlayer;
    
    /// <summary>
    /// ID numérico exclusivo deste jogador fornecido pelo Photon na rede.
    /// </summary>
    public int _id;

    /// <summary>
    /// Recebe do GameManager os dados de qual usuário real controla este avatar na rede.
    /// </summary>
    [PunRPC]
    public void Inicializa(Player player)
    {
        _photonPlayer = player;
        _id = player.ActorNumber;
        GameManager.Instancia.Jogadores.Add(this);

        // Trata a gravidade e colisões deste corpo físico caso ele seja apenas o clone na tela de outro PC.
        if(!photonView.IsMine)
            Rigidbody.isKinematic = false;
    }

    private void Awake()
    {
        Rigidbody = GetComponent<Rigidbody2D>();
        activeSpriteRenderer = spriteRendererDown;
        AudioManager.instance.PlayMusic("Battle");
    }

    void Update()
    {
        // Só verifica comandos do teclado se você for o real "dono" (LocalPlayer) desse boneco.
        if (photonView.IsMine)
        {
            if (Input.GetKey(inputUp))
                SetDirection(Vector2.up, spriteRendererUp);
            else if (Input.GetKey(inputDown))
                SetDirection(Vector2.down, spriteRendererDown);
            else if (Input.GetKey(inputLeft))
                SetDirection(Vector2.left, spriteRendererLeft);
            else if (Input.GetKey(inputRight))
                SetDirection(Vector2.right, spriteRendererRight);
            else
                SetDirection(Vector2.zero, activeSpriteRenderer); // Mantém o estado Parado (Idle)
        }
    }

    void FixedUpdate()
    {
        // Aplica a força de translação na engine física (Rigidbody) num ritmo constante e independente de framerate
        Vector2 position = Rigidbody.position;
        Vector2 translation = speed * Time.fixedDeltaTime * direction;

        Rigidbody.MovePosition(position + translation);
    }

    /// <summary>
    /// Envia o comando de movimento pela rede para alterar a animação nas telas dos outros jogadores simultaneamente.
    /// </summary>
    public void SetDirection(Vector2 newDirection, AnimatedSpriteRenderer spriteRenderer)
    {
        photonView.RPC("changeSprite", RpcTarget.All, _id, newDirection);
    }

    /// <summary>
    /// RPC chamado simultaneamente em todos os PCs na sala. 
    /// Processa a mudança visual dos sprites do boneco de acordo com o lado que ele andou.
    /// </summary>
    [PunRPC]
    public void changeSprite(int id, Vector2 newDirection)
    {
        // Garante que só altera a sprite deste player específico
        if (_id == id)
        {
            AnimatedSpriteRenderer spriteRenderer = activeSpriteRenderer;
            direction = newDirection;

            if (newDirection == Vector2.up)
                spriteRenderer = spriteRendererUp;
            else if (newDirection == Vector2.down)
                spriteRenderer = spriteRendererDown;
            else if (newDirection == Vector2.left)
                spriteRenderer = spriteRendererLeft;
            else if (newDirection == Vector2.right)
                spriteRenderer = spriteRendererRight;

            // Desliga as renderizações de animações que não estão ativas na direção correspondente
            spriteRendererUp.enabled = spriteRenderer == spriteRendererUp;
            spriteRendererDown.enabled = spriteRenderer == spriteRendererDown;
            spriteRendererLeft.enabled = spriteRenderer == spriteRendererLeft;
            spriteRendererRight.enabled = spriteRenderer == spriteRendererRight;

            activeSpriteRenderer = spriteRenderer;
            
            // Ativa a versão do boneco parado (idle) caso o vetor direction seja (0, 0)
            activeSpriteRenderer.idle = direction == Vector2.zero;
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Verifica se o objeto "fantasma" que este player encostou pertence à área da Explosão da bomba
        if (other.gameObject.layer == LayerMask.NameToLayer("Explosion"))
        {
            DeathSequence();
        }
    }

    /// <summary>
    /// Inicia a animação de morte, bloqueia os controles de movimento e de plantio de bombas do jogador.
    /// </summary>
    private void DeathSequence()
    {
        enabled = false;
        GetComponent<BombController>().enabled = false;

        spriteRendererUp.enabled = false;
        spriteRendererDown.enabled = false;
        spriteRendererLeft.enabled = false;
        spriteRendererRight.enabled = false;
        
        spriteRendererDeath.enabled = true;

        // Espera 1.25 segundos para a animação da caveira terminar de tocar, antes de apagar o player do mapa
        Invoke(nameof(OnDeathSequenceEnded), 1.25f);
    }

    /// <summary>
    /// Esconde de vez o jogador da cena após o fim da animação e avisa os menus do jogo.
    /// </summary>
    private void OnDeathSequenceEnded()
    {
        gameObject.SetActive(false);
        GameManager.Instancia.CheckWinState();
        
        // A tela de Game Over local só sobe na tela se o boneco que morreu foi o "seu"
        if (photonView.IsMine)
            MenuManager.instancia.GameOver();
    }

    /// <summary>
    /// Evento disparado nativamente pela engine de rede Photon PUN 2 quando qualquer pessoa sai da partida.
    /// Usado aqui para verificar se um adversário "desistiu/quitou" e se sobrou apenas um jogador (Vitória).
    /// </summary>
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        GameManager.Instancia.CheckWinState();
    }
}
