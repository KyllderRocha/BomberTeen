using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.Tilemaps;

public class BombermanAI : MonoBehaviour
{

    public AnimatedSpriteRenderer spriteRendererUp;
    public AnimatedSpriteRenderer spriteRendererDown;
    public AnimatedSpriteRenderer spriteRendererLeft;
    public AnimatedSpriteRenderer spriteRendererRight;
    public AnimatedSpriteRenderer spriteRendererDeath;
    public AnimatedSpriteRenderer activeSpriteRenderer;
    private Vector2 direction = Vector2.down;

    public Tilemap tilemap; // O Tilemap utilizado
    public GameObject inimigo; // O inimigo
    public GameObject character; // O Personagem

    private GridAStar grid; // O grid do Tilemap
    private NodeAStar[,] nodes; // Matriz de nós do grid
    private List<NodeAStar> path; // Caminho para o inimigo
    private int currentPathIndex; // Índice do nó atual no caminho

    private int moveSpeed = 1; 
    private float checkInterval = 0.5f; // Intervalo de verificação de bombas próximas
    private int layerBomb;

    private void Start()
    {
        layerBomb = LayerMask.NameToLayer("Bomb");
        grid = new GridAStar(tilemap);
        nodes = grid.CreateGrid();

        // Encontrar e seguir caminho até o inimigo
        StartCoroutine(FindPathToEnemy());

        // Iniciar a verificação regular de bombas próximas
        InvokeRepeating("CheckNearbyBombs", 0f, checkInterval);
    }

    private void Update()
    {
        if (path != null && currentPathIndex < path.Count)
        {
            // Obter a posição do próximo nó do caminho
            Vector3 targetPosition = grid.GetWorldPositionFromNode(path[currentPathIndex]);

            // Movimentar o personagem em direção ao próximo nó do caminho
            MoveToPosition(targetPosition);
        }
    }

    //private void FixedUpdate()
    //{

    //    if (path != null && currentPathIndex < path.Count)
    //    {
    //        // Obter a posição do próximo nó do caminho
    //        Vector3 targetPosition = grid.GetWorldPositionFromNode(path[currentPathIndex]);

    //        // Movimentar o personagem em direção ao próximo nó do caminho
    //        MoveToPosition(targetPosition);
    //    }
    //}

    private IEnumerator FindPathToEnemy()
    {
        // Obter o nó inicial a partir da posição atual do Bomberman
        NodeAStar startNode = grid.NodeFromWorldPosition(transform.position);

        // Obter o nó alvo a partir da posição do inimigo
        NodeAStar targetNode = grid.NodeFromWorldPosition(inimigo.transform.position);

        // Verificar se o alvo está no mesmo nó do Bomberman
        if (startNode == targetNode)
        {
            Debug.Log("Cheguei até o inimigo!");
            yield break;
        }

        bool pathFound = AStar.FindPath(startNode, targetNode, grid);

        if (pathFound)
        {
            // Obter o caminho encontrado
            path = AStar.GetPath(targetNode);
            currentPathIndex = 0;
        }
        else
        {
            Debug.Log("Caminho não encontrado!");
        }

    }

    private void MoveToPosition(Vector3 position)
    {
        // Calcular a direção do movimento
        Vector3 direction = (position - transform.position).normalized;

        // Movimentar o personagem em uma velocidade proporcional à sua velocidade máxima
        character.transform.position += direction * moveSpeed * Time.fixedDeltaTime;
        changeSprite(direction);

        // Verificar se o personagem alcançou o próximo nó do caminho
        if (Vector3.Distance(transform.position, position) < 0.1f)
        {
            currentPathIndex++;

            // Verificar se alcançou o destino
            if (currentPathIndex >= path.Count)
            {
                var bombControllerIA = GetComponent<BombControllerIA>();
                bombControllerIA.PlacedBomber();
                Debug.Log("Cheguei até o inimigo!");
                return;
            }
        }
    }

    public void changeSprite(Vector2 newDirection)
    {
        AnimatedSpriteRenderer spriteRenderer = activeSpriteRenderer;
        direction = newDirection;

        if (newDirection.y > 0.9)
            spriteRenderer = spriteRendererUp;
        else if (newDirection.y < -0.9)
            spriteRenderer = spriteRendererDown;
        else if (newDirection.x < -0.9)
            spriteRenderer = spriteRendererLeft;
        else if (newDirection.x > 0.9)
            spriteRenderer = spriteRendererRight;
        else
            spriteRenderer = activeSpriteRenderer;

        spriteRendererUp.enabled = spriteRenderer == spriteRendererUp;
        spriteRendererDown.enabled = spriteRenderer == spriteRendererDown;
        spriteRendererLeft.enabled = spriteRenderer == spriteRendererLeft;
        spriteRendererRight.enabled = spriteRenderer == spriteRendererRight;

        activeSpriteRenderer = spriteRenderer;
        activeSpriteRenderer.idle = direction == Vector2.zero;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Explosion"))
        {
            DeathSequence();
        }
    }

    private void DeathSequence()
    {
        enabled = false;
        GetComponent<BombControllerIA>().enabled = false;

        spriteRendererUp.enabled = false;
        spriteRendererDown.enabled = false;
        spriteRendererLeft.enabled = false;
        spriteRendererRight.enabled = false;
        spriteRendererDeath.enabled = true;

        Invoke(nameof(OnDeathSequenceEnded), 1.25f);
    }

    private void OnDeathSequenceEnded()
    {
        gameObject.SetActive(false);
        //GameManager.Instancia.CheckWinState();
    }

    private void CheckNearbyBombs()
    {
        NodeAStar currentNode = grid.NodeFromWorldPosition(transform.position);

        // Verificar se há bombas próximas ao nó atual
        if (HasNearbyBombs(currentNode))
        {
            Vector3 avoidanceDirection = GetAvoidanceDirection(currentNode);
            transform.position += avoidanceDirection * moveSpeed * Time.fixedDeltaTime;
        }
    }

    private bool HasNearbyBombs(NodeAStar node)
    {
        // Defina a distância máxima para considerar uma bomba como "próxima"
        float maxDistance = 3f;

        GameObject[] bombs = GameObject.FindObjectsOfType<GameObject>()
            .Where(obj => obj.layer == layerBomb)
            .ToArray();


        // Percorra todas as bombas e verifique a distância em relação ao nó atual
        foreach (GameObject bomb in bombs)
        {
            // Calcule a distância entre o nó atual e a posição da bomba
            float distance = Vector3.Distance(grid.GetWorldPositionFromNode(node), bomb.transform.position);

            // Verifique se a distância é menor ou igual à distância máxima
            if (distance <= maxDistance)
            {
                return true; // Há uma bomba próxima
            }
        }

        return false; // Não há bombas próximas
    }

    private Vector3 GetAvoidanceDirection(NodeAStar currentNode)
    {
        // Defina a distância máxima para considerar uma bomba como "próxima"
        float maxDistance = 2f;

        GameObject[] bombs = GameObject.FindObjectsOfType<GameObject>()
           .Where(obj => obj.layer == layerBomb)
           .ToArray();

        Vector3 avoidanceDirection = Vector3.zero;

        // Percorra todas as bombas e calcule a direção de desvio em relação ao nó atual
        foreach (GameObject bomb in bombs)
        {
            // Calcule a distância entre o nó atual e a posição da bomba
            float distance = Vector3.Distance(grid.GetWorldPositionFromNode(currentNode), bomb.transform.position);

            // Verifique se a distância é menor ou igual à distância máxima
            if (distance <= maxDistance)
            {
                // Calcule a direção de desvio em relação à posição da bomba
                Vector3 directionToAvoid = (grid.GetWorldPositionFromNode(currentNode) - bomb.transform.position).normalized;

                // Some a direção de desvio à direção total de desvio
                avoidanceDirection += directionToAvoid;
            }
        }

        // Normalize a direção total de desvio
        avoidanceDirection.Normalize();

        return avoidanceDirection;
    }
}
