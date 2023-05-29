using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Destructible : MonoBehaviourPunCallbacks
{
    public float destructionTime = 0.9f;

    [Range(0f, 1f)]
    public float itemSpawnChance = 0.2f;
    //public GameObject[] spawnableItems;
    public string[] spawnableItems;

    private void Start()
    {
        Destroy(gameObject, destructionTime);
    }

    private void OnDestroy()
    {
        if (spawnableItems.Length > 0 && Random.value < itemSpawnChance && PhotonNetwork.IsMasterClient)
        {
            int randomIndex = Random.Range( 0, spawnableItems.Length );
            //Instantiate(spawnableItems[randomIndex], transform.position, Quaternion.identity);

            var itemObj = PhotonNetwork.Instantiate(spawnableItems[randomIndex], transform.position, Quaternion.identity);
        }
    }
}
