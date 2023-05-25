using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapGeneration : MonoBehaviourPunCallbacks
{

    [Header("Destructible")]
    public Tilemap destructibleTiles;
    public TileBase tileBrick;

    [Header("Indestructible")]
    public Tilemap indestructibleTiles;

    public void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Vector3Int cell = destructibleTiles.origin;
            TileBase tile = null;
            TileBase tileIndes = null;

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
                            photonView.RPC("SetDestructibleTile", RpcTarget.AllBuffered, cell.x, cell.y, "Brick");
                        }
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
}
