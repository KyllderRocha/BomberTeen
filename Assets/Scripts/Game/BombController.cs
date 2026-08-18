using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BombController : MonoBehaviourPunCallbacks
{
    /*
     * Melhorias
     * 
     * Colisão Com player
     * Colisão com bomba fixa
     * Explosão afetar outras bombas
     * Explosão apagar power up
     * 
    */

    [Header("Bomb")]
    [Tooltip("Tecla usada para plantar a bomba.")]
    public KeyCode inputKey = KeyCode.Space;
    public GameObject bombPrefab;
    
    [Tooltip("Tempo em segundos (pavio) até a bomba explodir sozinha.")]
    public float bombFuseTime = 3.0f;
    
    [Tooltip("Quantidade máxima de bombas que o jogador pode ter ao mesmo tempo na tela.")]
    public int bombAmount = 1;
    private int bombsRemaining = 0; // Controle interno de bombas disponíveis para uso

    [Header("Explosion")]
    public Explosion explosionPrefab;
    
    [Tooltip("Diz para a Unity quais Layers (camadas) o fogo da explosão deve interagir/destruir (ex: a layer Destructible).")]
    public LayerMask explosionLayerMask;
    public static float explosianDurationStatic = 1f;
    public float explosianDuration = 1f; // Tempo que o fogo fica aceso na tela
    
    [Tooltip("Tamanho do raio da explosão (quantos blocos o fogo alcança). Aumenta ao pegar power-ups.")]
    public int explosionRadius = 1;


    [Header("Destructible")]
    // Referências antigas ou para sistemas locais (antes da migração para rede).
    public Tilemap destructibleTiles;
    public Destructible destructiblePrefab;

    [Header("Network Prefabs (Photon Resources)")]
    // Estes nomes (strings) precisam bater com os nomes exatos dos Prefabs salvos na pasta 'Resources' do projeto.
    public string _localizacao;
    public string _localizacaoExplosion;
    public string _localizacaoDestructible;

    public MapGeneration mapGeneration;

    private void Awake()
    {
        mapGeneration = GetComponent<MapGeneration>();
    }

    private void OnEnable()
    {
        bombsRemaining = bombAmount;
    }

    private void Update()
    {
        if (photonView.IsMine)
        {
            if(bombsRemaining > 0 && Input.GetKeyDown(inputKey))
            {
                StartCoroutine(PlaceBomb());
            }
        }
        
    }

    /// <summary>
    /// Corrotina responsável por posicionar a bomba na grade, tocar o som, aguardar o tempo do pavio (timer) 
    /// e finalmente executar a explosão pela rede.
    /// </summary>
    private IEnumerator PlaceBomb()
    {
        Vector2 position = transform.position;
        // Arredonda a posição para garantir que a bomba fique perfeitamente alinhada no grid/quadradinho
        position.x = Mathf.Round(position.x);
        position.y = Mathf.Round(position.y);

        AudioManager.instance.PlaySFX("Drop");

        // Cria a bomba sincronizada em todos os PCs (precisa estar na pasta Resources)
        GameObject bomb = PhotonNetwork.Instantiate(_localizacao, position, Quaternion.identity);
        
        Bomb bombScript = bomb.GetComponent<Bomb>();
        if (bombScript != null)
        {
            bombScript.ownerController = this; // Define quem plantou a bomba
        }

        bombsRemaining--; // Gasta uma bomba

        float timer = bombFuseTime;
        // Loop de espera: Roda todo frame até o tempo acabar OU a bomba explodir por contato (reação em cadeia)
        while (timer > 0 && bomb != null)
        {
            // Se o script da bomba foi acionado externamente por um raio de fogo...
            if (bombScript != null && bombScript.shouldExplode)
            {
                Debug.Log("[BombController] Identificou shouldExplode = true. Quebrando o loop do tempo!");
                break; // Corta o tempo de espera e explode agora
            }
            timer -= Time.deltaTime;
            yield return null; // Pausa a execução e volta no próximo frame
        }

        Debug.Log("[BombController] Saindo do loop. Executando a explosão final da bomba!");
        if (bomb != null)
        {
            ExecuteExplosion(bomb.transform.position);
            PhotonNetwork.Destroy(bomb); // O Dono da bomba apaga ela da rede
            bombsRemaining++; // Devolve a carga da bomba para o jogador usar de novo
        }
    }

    /// <summary>
    /// Gera o efeito visual do fogo "Central" onde a bomba estava e dispara raios de explosão
    /// para as 4 direções (Cima, Baixo, Esquerda, Direita).
    /// </summary>
    private void ExecuteExplosion(Vector2 position)
    {
        position.x = Mathf.Round(position.x);
        position.y = Mathf.Round(position.y);

        AudioManager.instance.PlaySFX("Explosion");

        // Cria o fogo central da explosão pela rede
        var explosionObj = PhotonNetwork.Instantiate(_localizacaoExplosion, position, Quaternion.identity);
        var explosion = explosionObj.GetComponent<Explosion>();

        explosion.photonView.RPC("SetActiveRenderer", RpcTarget.All, "start"); // 'start' é a sprite do centro
        explosion.photonView.RPC("DestroyAfter", RpcTarget.All, explosianDuration);

        // Inicia a criação das hastes de fogo para cada lado baseada no tamanho do explosionRadius
        Explode(position, Vector2.up, explosionRadius);
        Explode(position, Vector2.down, explosionRadius);
        Explode(position, Vector2.left, explosionRadius);
        Explode(position, Vector2.right, explosionRadius);
    }

    /// <summary>
    /// Função recursiva responsável por criar um rastro de fogo na direção escolhida.
    /// Ela checa colisões (usando OverlapBox) para destruir caixas ou parar em obstáculos indestrutíveis.
    /// </summary>
    private void Explode(Vector2 position, Vector2 direction, int legth)
    {
        if (legth <= 0)
            return;

        position += direction;

        // Projeta uma "caixa invisível" naquele quadrado para verificar o que tem lá antes de colocar fogo
        Collider2D[] hits = Physics2D.OverlapBoxAll(position, Vector2.one / 2f, 0f);
        bool stopped = false; // Flag para saber se o fogo bateu em algo e deve parar

        foreach (Collider2D hit in hits)
        {
            Debug.Log($"[BombController.Explode] A explosão bateu em: {hit.name} (Layer: {LayerMask.LayerToName(hit.gameObject.layer)}) na posição {position}");

            Bomb hitBomb = hit.GetComponentInParent<Bomb>();
            ItemPickup item = hit.GetComponentInParent<ItemPickup>();

            // Se bateu em uma outra bomba, aciona a reação em cadeia nela
            if (hitBomb != null)
            {
                Debug.Log($"[BombController.Explode] Encontrou a bomba! Enviando RPC para {hitBomb.name}");
                PhotonView pv = hitBomb.GetComponentInParent<PhotonView>();
                if (pv != null)
                {
                    pv.RPC("TriggerExplosion", RpcTarget.All);
                }
                stopped = true;
            }
            // Se bateu em um coletável (power-up), queima ele da rede
            else if (item != null)
            {
                if (PhotonNetwork.IsMasterClient && item.GetComponent<PhotonView>() != null)
                {
                    PhotonNetwork.Destroy(item.gameObject);
                }
            }
            // Se bateu em uma caixa destrutível (comparando LayerMask usando bitwise)
            else if (explosionLayerMask == (explosionLayerMask | (1 << hit.gameObject.layer)))
            {
                // Avisa ao Host (MasterClient) que aquela coordenada do mapa (Tilemap) deve ser destruída
                MapGeneration.Instancia.photonView.RPC("Destructible", RpcTarget.MasterClient, position.x, position.y);
                stopped = true;
            }
        }

        // Se encontrou obstáculo sólido/parede/caixa, não desenha o fogo nesse quadrado nem adiante
        if (stopped)
        {
            return;
        }

        var explosionObj = PhotonNetwork.Instantiate(_localizacaoExplosion, position, Quaternion.identity);
        var explosion = explosionObj.GetComponent<Explosion>();

        // Se ainda faltam blocos para expandir, usa o corpo do raio ("middle"). Senão, usa a ponta do raio ("end").
        explosion.photonView.RPC("SetActiveRenderer", RpcTarget.All, legth > 1 ? "middle" : "end");
        explosion.photonView.RPC("SetDirection", RpcTarget.All, direction);
        explosion.photonView.RPC("DestroyAfter", RpcTarget.All, explosianDuration);

        // Chama a si mesma novamente para o próximo quadrado (Recursão) subtraindo 1 do comprimento restante
        Explode(position, direction, legth - 1);
    }

    private void ClearDestructible(Collider2D hitCollider)
    {
        var position = hitCollider.transform.position;

        Destroy(hitCollider.gameObject);

        //Debug.Log(position);
        ////Debug.Log(destructibleTiles);
        //Vector3Int cell = destructibleTiles.WorldToCell(position);
        ////Debug.Log(cell);
        //TileBase tile = destructibleTiles.GetTile(cell);
        //Debug.Log(tile);

        //if (tile != null)
        //{
        //    //Instantiate(destructiblePrefab, position, Quaternion.identity);
        //    //destructibleTiles.SetTile(cell, null);

        //    //var destructibleObj = PhotonNetwork.Instantiate(_localizacaoDestructible, position, Quaternion.identity);
        //    //var explosion = explosionObj.GetComponent<Explosion>();
        //    destructibleTiles.SetTile(cell, null);

        //}
    }

    //private void ClearDestructible(Vector2 position)
    //{
    //    Debug.Log(position);
    //    //Debug.Log(destructibleTiles);
    //    Vector3Int cell = destructibleTiles.WorldToCell(position);
    //    //Debug.Log(cell);
    //    TileBase tile = destructibleTiles.GetTile(cell);
    //    Debug.Log(tile);

    //    if (tile != null)
    //    {
    //        //Instantiate(destructiblePrefab, position, Quaternion.identity);
    //        //destructibleTiles.SetTile(cell, null);

    //        //var destructibleObj = PhotonNetwork.Instantiate(_localizacaoDestructible, position, Quaternion.identity);
    //        //var explosion = explosionObj.GetComponent<Explosion>();
    //        destructibleTiles.SetTile(cell, null);

    //    }
    //}

    private void SetDestructibleTile(Vector3Int cell)
    {
        destructibleTiles.SetTile(cell, null);
    }

    public void AddBomb()
    {
        bombAmount++;
        bombsRemaining++;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Bomb"))
        {
            other.isTrigger = false;
        }
    }
}
