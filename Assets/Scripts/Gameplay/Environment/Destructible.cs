using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Script attached to the debris animation of a box that was just exploded.
/// It lives for a short time, disappears from the screen, and has a mathematical chance to drop a Power-Up (Item).
/// </summary>
public class Destructible : NetworkBehaviour
{
    [Tooltip("Time (in seconds) that the breaking box animation lasts before disappearing.")]
    public float destructionTime = 0.9f;

    [Tooltip("Percentage (0 to 1) chance to drop an item on the ground. Ex: 0.2f = 20%")]
    [Range(0f, 1f)]
    public float itemSpawnChance = 0.2f;
    
    [Tooltip("Exact references to the network prefabs that can drop.")]
    public GameObject[] spawnableItemsNetwork;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            StartCoroutine(DespawnRoutine());
        }
    }

    private IEnumerator DespawnRoutine()
    {
        var animRenderer = GetComponent<AnimatedSpriteRenderer>();
        float waitTime = destructionTime;
        if (animRenderer != null && animRenderer.animationSprites != null && animRenderer.animationSprites.Length > 0)
        {
            waitTime = animRenderer.animationSprites.Length * animRenderer.animationTime;
        }

        yield return new WaitForSeconds(waitTime);

        if (spawnableItemsNetwork == null || spawnableItemsNetwork.Length == 0)
        {
            Debug.LogWarning("[Destructible] A lista 'Spawnable Items Network' está vazia! Arraste os prefabs de itens no Inspector.");
        }
        else
        {
            float sorteio = Random.value;
            Debug.Log($"[Destructible] Tentando dropar item. Sorteio: {sorteio:F2} | Chance Necessária: menor que {itemSpawnChance:F2}");

            if (sorteio < itemSpawnChance)
            {
                int randomIndex = Random.Range(0, spawnableItemsNetwork.Length);
                var itemToSpawn = spawnableItemsNetwork[randomIndex];

                if (itemToSpawn != null)
                {
                    Debug.Log($"[Destructible] Sucesso! Sorteou o item: {itemToSpawn.name}");
                    var itemObj = Instantiate(itemToSpawn, transform.position, Quaternion.identity);
                    itemObj.GetComponent<NetworkObject>().Spawn(true);
                }
                else
                {
                    Debug.LogError("[Destructible] O item sorteado está nulo! Há um espaço vazio (None) no Array do Inspector.");
                }
            }
        }

        // Host despawns the block safely
        if (NetworkObject != null && NetworkObject.IsSpawned)
            NetworkObject.Despawn(true);
    }
}
