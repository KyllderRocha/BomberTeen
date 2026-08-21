using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Script attached to the debris animation of a box that was just exploded.
/// It lives for a short time, disappears from the screen, and has a mathematical chance to drop a Power-Up (Item).
/// </summary>
public class Destructible : MonoBehaviourPunCallbacks
{
    [Tooltip("Time (in seconds) that the breaking box animation lasts before disappearing.")]
    public float destructionTime = 0.9f;

    [Tooltip("Percentage (0 to 1) chance to drop an item on the ground. Ex: 0.2f = 20%")]
    [Range(0f, 1f)]
    public float itemSpawnChance = 0.2f;
    
    [Tooltip("Exact names of the prefabs in the Resources folder that can drop (Ex: SpeedItem, BombItem).")]
    public string[] spawnableItems;

    private void Start()
    {
        // Programs the auto-destruction of this piece of wall after X seconds
        Destroy(gameObject, destructionTime);
    }

    /// <summary> When the destruction animation ends, the engine natively calls OnDestroy. </summary>
    private void OnDestroy()
    {
        // Raffle: If there are registered items, passed the random check, and YOU are the Match Host...
        if (spawnableItems.Length > 0 && Random.value < itemSpawnChance && PhotonNetwork.IsMasterClient)
        {
            // Pulls a random power-up from the list of available items
            int randomIndex = Random.Range( 0, spawnableItems.Length );

            // Instantiates the PowerUp via network so everyone in the room sees the item spawning on the ground
            var itemObj = PhotonNetwork.Instantiate(spawnableItems[randomIndex], transform.position, Quaternion.identity);
        }
    }
}
