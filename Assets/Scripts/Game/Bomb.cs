using UnityEngine;
using Photon.Pun;

/// <summary>
/// Script anexado ao Prefab da Bomba.
/// Diferente do BombController (que fica no Player e cuida do ato de plantar a bomba), 
/// este script lida apenas com a própria bomba, acionando a detonação antecipada 
/// caso ela seja atingida por outra explosão (Reação em Cadeia).
/// </summary>
public class Bomb : MonoBehaviourPun
{
    /// <summary> Referência ao script do jogador que plantou esta bomba (dono local). </summary>
    public BombController ownerController;
    
    /// <summary> Flag lida pelo BombController do jogador para saber se deve interromper o cronômetro do pavio. </summary>
    public bool shouldExplode = false;
    
    // Trava de segurança para evitar múltiplos gatilhos de explosão acionando repetidamente.
    private bool exploded = false;

    void Update()
    {
        // Só verifica detecção de fogo se a bomba ainda não explodiu e se é o "dono" da bomba checando.
        if (exploded || !photonView.IsMine) return;

        // Prepara um filtro para detectar colisões, incluindo "Triggers" (ex: o fogo fantasma da explosão).
        ContactFilter2D filter = new ContactFilter2D();
        filter.useTriggers = true;
        Collider2D[] hits = new Collider2D[10];
        
        // Joga uma "caixa virtual" em cima da bomba e coleta tudo o que tocou nela neste frame.
        int count = Physics2D.OverlapBox(transform.position, Vector2.one / 2f, 0f, filter, hits);
        
        for (int i = 0; i < count; i++)
        {
            Collider2D hit = hits[i];
            
            // Se o item que encostou na bomba for o fogo de uma explosão vizinha...
            if (hit.GetComponentInParent<Explosion>() != null)
            {
                Debug.Log($"[Bomb] Detectou uma explosão na posição {transform.position}! Enviando RPC para explodir...");
                // Dispara o comando para todas as telas (RPC) avisando que essa bomba explodiu por reação em cadeia.
                photonView.RPC("TriggerExplosion", RpcTarget.All);
                break;
            }
        }
    }

    /// <summary>
    /// Força a detonação da bomba pela rede. É chamado quando ocorre uma reação em cadeia.
    /// Altera a flag para que o BombController do dono perceba a interrupção.
    /// </summary>
    [PunRPC]
    public void TriggerExplosion()
    {
        Debug.Log($"[Bomb] RPC TriggerExplosion recebido!");
        if (exploded) return; // Evita loop infinito caso duas bombas tentem explodir ao mesmo tempo
        exploded = true;
        shouldExplode = true;
        Debug.Log($"[Bomb] shouldExplode definido para true! Aguardando BombController.");
    }
}
