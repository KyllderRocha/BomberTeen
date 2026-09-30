using UnityEngine;
using Unity.Netcode;
using System.Collections;
using BomberTeen;

/// <summary>
/// Script attached to the Bomb Prefab.
/// Manages the bomb's fuse timer on the server, early detonation on chain reactions,
/// shooting explosion rays, and despawning cleanly over Netcode.
/// </summary>
public class Bomb : NetworkBehaviour
{
    public ulong ownerClientId;
    public int explosionRadius = 1;
    public float bombFuseTime = 3.0f;
    public float explosionDuration = 1.0f;
    public LayerMask explosionLayerMask;
    public GameObject explosionPrefabNetwork;

    public bool shouldExplode = false;
    private bool exploded = false;

    private Collider2D[] results = new Collider2D[10];
    private ContactFilter2D filter;
    private Collider2D bombCollider;

    private void Awake()
    {
        filter = new ContactFilter2D();
        filter.useTriggers = true;
        bombCollider = GetComponent<Collider2D>();
    }

    private void Start()
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlaySFX(Constants.Audio.Drop);
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            StartCoroutine(ServerFuseRoutine());
        }
    }

    private IEnumerator ServerFuseRoutine()
    {
        float timer = bombFuseTime;
        while (timer > 0 && !exploded && !shouldExplode)
        {
            timer -= Time.deltaTime;
            yield return null;
        }

        Detonate();
    }

    void Update()
    {
        if (exploded || !IsServer) return;

        int count = Physics2D.OverlapBox(transform.position, Vector2.one / 2f, 0f, filter, results);
        for (int i = 0; i < count; i++)
        {
            Collider2D hit = results[i];
            if (hit != null && hit.GetComponentInParent<Explosion>() != null)
            {
                TriggerExplosionRpc();
                break;
            }
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void TriggerExplosionRpc()
    {
        if (exploded) return;
        shouldExplode = true;
        if (IsServer)
        {
            Detonate();
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void SetSolidRpc()
    {
        if (bombCollider != null)
        {
            bombCollider.isTrigger = false;
        }
    }

    private void Detonate()
    {
        if (exploded || !IsServer) return;
        exploded = true;

        Vector2 position = transform.position;
        position.x = Mathf.Round(position.x);
        position.y = Mathf.Round(position.y);

        ExecuteExplosion(position);

        RefundPlanterBomb();

        if (NetworkObject != null && NetworkObject.IsSpawned)
        {
            NetworkObject.Despawn(true);
        }
    }

    private void RefundPlanterBomb()
    {
        if (NetworkManager.Singleton == null) return;

        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(ownerClientId, out var client))
        {
            if (client.PlayerObject != null)
            {
                var bombController = client.PlayerObject.GetComponent<BombController>();
                if (bombController != null)
                {
                    bombController.RefundBombRpc();
                }
            }
        }
    }

    private void ExecuteExplosion(Vector2 position)
    {
        if (explosionPrefabNetwork == null) return;

        // Central explosion fire
        SpawnExplosionTile(position, Constants.Animations.ExplosionStart, Vector2.zero);

        // Fire rays in 4 directions
        ExplodeRay(position, Vector2.up, explosionRadius);
        ExplodeRay(position, Vector2.down, explosionRadius);
        ExplodeRay(position, Vector2.left, explosionRadius);
        ExplodeRay(position, Vector2.right, explosionRadius);
    }

    private void ExplodeRay(Vector2 position, Vector2 direction, int length)
    {
        if (length <= 0) return;

        position += direction;
        Collider2D[] hits = Physics2D.OverlapBoxAll(position, Vector2.one / 2f, 0f);
        bool stopped = false;

        foreach (Collider2D hit in hits)
        {
            if (hit == null) continue;

            Bomb hitBomb = hit.GetComponentInParent<Bomb>();
            ItemPickup item = hit.GetComponentInParent<ItemPickup>();

            if (hitBomb != null && hitBomb != this)
            {
                hitBomb.TriggerExplosionRpc();
                stopped = true;
            }
            else if (item != null)
            {
                if (IsServer && item.NetworkObject != null && item.NetworkObject.IsSpawned)
                {
                    item.NetworkObject.Despawn(true);
                }
            }
            else if (explosionLayerMask == (explosionLayerMask | (1 << hit.gameObject.layer)))
            {
                if (MapGeneration.instance != null)
                {
                    MapGeneration.instance.DestructibleRpc(position.x, position.y);
                }
                stopped = true;
            }
        }

        if (stopped) return;

        string rendererType = length > 1 ? Constants.Animations.ExplosionMiddle : Constants.Animations.ExplosionEnd;
        SpawnExplosionTile(position, rendererType, direction);

        ExplodeRay(position, direction, length - 1);
    }

    private void SpawnExplosionTile(Vector2 position, string rendererType, Vector2 direction)
    {
        Quaternion rotation = Quaternion.identity;
        if (direction != Vector2.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x);
            rotation = Quaternion.AngleAxis(angle * Mathf.Rad2Deg, Vector3.forward);
        }

        var explosionObj = Instantiate(explosionPrefabNetwork, position, rotation);
        var netObj = explosionObj.GetComponent<NetworkObject>();
        if (netObj != null)
        {
            netObj.Spawn(true);
        }

        var explosion = explosionObj.GetComponent<Explosion>();
        if (explosion != null)
        {
            explosion.SetActiveRendererRpc(rendererType);
            if (direction != Vector2.zero)
            {
                explosion.SetDirectionRpc(direction);
            }
        }
    }
}
