using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class MapGeneration : MonoBehaviourPunCallbacks
{
    public static MapGeneration Instancia { get; private set; }

    [Header("Destructible")]
    public Tilemap destructibleTiles;
    public TileBase tileBrick;
    public string _prefabDestructible;

    [Header("Indestructible")]
    public Tilemap indestructibleTiles;

    private void Awake()
    {
        if (Instancia != null & Instancia != this)
        {
            gameObject.SetActive(false);
            return;
        }
        Instancia = this;
        //DontDestroyOnLoad(gameObject);
    }

    public void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Vector3Int cell = destructibleTiles.origin;
            TileBase tile = null;
            TileBase tileIndes = null;
            Debug.Log(cell);

            for (int y = 0; y < destructibleTiles.size.y; y++)
            {
                cell.x = destructibleTiles.origin.x;

                for (int x = 0; x < destructibleTiles.size.x; x++)
                {
                    tile = destructibleTiles.GetTile(cell);
                    tileIndes = indestructibleTiles.GetTile(cell);

                    if (tile == null && tileIndes.name != "Block")
                    {
                        if (Random.value < 0.75f)
                        {
                            //destructibleTiles.SetTile(cell, tileBrick);
                            photonView.RPC("SetDestructibleTile", RpcTarget.All, cell.x, cell.y, "Brick");
                        }
                    }else if(tile != null)
                    {
                        photonView.RPC("SetDestructibleTile", RpcTarget.All, cell.x, cell.y, "");
                    }
                    cell.x += 1;
                }
                cell.y += 1;
            }
        }
    }

    [PunRPC]
    public void SetDestructibleTile(int x, int y, string tileText)
    {
        Vector3Int cell = new Vector3Int(x, y);
        TileBase tile = null;
        if(tileText == "Brick")
        {
            tile = tileBrick;
        }
        destructibleTiles.SetTile(cell, tile);
    }

    [PunRPC]
    public void Destructible(float x, float y)
    {
        x -= 1;
        y -= 1;
        Vector3Int cell = new Vector3Int((int) x, (int) y);
        TileBase tile = destructibleTiles.GetTile(cell);
        if (tile != null)
        {
            photonView.RPC("SetDestructibleTile", RpcTarget.All, cell.x, cell.y, "");

            Vector3Int cellDestructible = new Vector3Int((int)x +1, (int) y +1);
            var destructibleObj = PhotonNetwork.Instantiate(_prefabDestructible, cellDestructible, Quaternion.identity);
            //var destructible = destructibleObj.GetComponent<Destructible>();
        }
    }


}
