using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

/*
 * PROCEDURAL MAP GENERATION
 * -------------------------------------------
 * This script handles the procedural creation of destructible blocks.
 * It demonstrates a good network pattern: Only the MasterClient (Room Owner) decides where the blocks spawn
 * and then communicates it to the other players via RPC ("SetDestructibleTile").
 * 
 * NullReference Bug fix included in the Cell verification.
 */
public class MapGeneration : MonoBehaviourPunCallbacks
{
    public static MapGeneration instance { get; private set; }

    [Header("Destructible")]
    public Tilemap destructibleTiles;
    public TileBase tileBrick;
    public string prefabDestructible;

    [Header("Indestructible")]
    public Tilemap indestructibleTiles;

    private void Awake()
    {
        // Singleton Pattern for easy access from anywhere (like the Bomb warning it hit the wall)
        if (instance != null & instance != this)
        {
            gameObject.SetActive(false);
            return;
        }
        instance = this;
    }

    public void Start()
    {
        // Security: The entire map is only generated on the "Host" machine.
        // It will decide where the walls are and send the Blueprint via RPC to the others.
        if (PhotonNetwork.IsMasterClient)
        {
            Vector3Int cell = destructibleTiles.origin;
            TileBase tile = null;
            TileBase tileIndes = null;
            Debug.Log(cell);

            // Scans the entire visual matrix of the map (row by row, column by column)
            for (int y = 0; y < destructibleTiles.size.y; y++)
            {
                cell.x = destructibleTiles.origin.x;

                for (int x = 0; x < destructibleTiles.size.x; x++)
                {
                    tile = destructibleTiles.GetTile(cell);
                    tileIndes = indestructibleTiles.GetTile(cell);

                    // If the cell is empty and it's not an armored iron wall...
                    if (tile == null && tileIndes.name != "Block")
                    {
                        // 75% chance to spawn a destructible box in this hole
                        if (Random.value < 0.75f)
                        {
                            // Sends the order to everyone: Place the "Brick" art at X and Y
                            photonView.RPC("SetDestructibleTile", RpcTarget.All, cell.x, cell.y, "Brick");
                        }
                    }else if(tile != null)
                    {
                        // If there was an improper wall, ensures the entire network clears it
                        photonView.RPC("SetDestructibleTile", RpcTarget.All, cell.x, cell.y, "");
                    }
                    cell.x += 1;
                }
                cell.y += 1;
            }
        }
    }

    /// <summary>
    /// Edits the Tilemap component. The RPC is received by all computers to 
    /// place the map boxes in exactly the same coordinates.
    /// </summary>
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

    /// <summary>
    /// Called by the BombController (from the host's machine) informing that the fire hit a box.
    /// Erases the solid block (Tile) and spawns the animated dust model (Destructible) in its place.
    /// </summary>
    [PunRPC]
    public void Destructible(float x, float y)
    {
        // Tilemap offset correction
        x -= 1;
        y -= 1;
        Vector3Int cell = new Vector3Int((int) x, (int) y);
        TileBase tile = destructibleTiles.GetTile(cell);
        
        if (tile != null)
        {
            // Tells everyone to remove the visual and solid "wall" (Tile)
            photonView.RPC("SetDestructibleTile", RpcTarget.All, cell.x, cell.y, "");

            Vector3Int cellDestructible = new Vector3Int((int)x +1, (int) y +1);
            
            // Physically instantiates the 2D dust animation object via Photon, which will drop the item later
            var destructibleObj = PhotonNetwork.Instantiate(prefabDestructible, cellDestructible, Quaternion.identity);
        }
    }


}
