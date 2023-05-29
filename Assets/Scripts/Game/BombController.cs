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
    public KeyCode inputKey = KeyCode.Space;
    public GameObject bombPrefab;
    public float bombFuseTime = 3.0f;
    public int bombAmount = 1;
    private int bombsRemaining = 0;

    [Header("Explosion")]
    public Explosion explosionPrefab;
    public LayerMask explosionLayerMask;
    public static float explosianDurationStatic = 1f;
    public float explosianDuration = 1f;
    public int explosionRadius = 1;


    [Header("Destructible")]
    public Tilemap destructibleTiles;
    public Destructible destructiblePrefab;

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

    private IEnumerator PlaceBomb()
    {
        Vector2 position = transform.position;
        position.x = Mathf.Round(position.x);
        position.y = Mathf.Round(position.y);

        AudioManager.instance.PlaySFX("Drop");

        GameObject bomb = PhotonNetwork.Instantiate(_localizacao, position, Quaternion.identity);
        bombsRemaining--;

        yield return new WaitForSeconds(bombFuseTime);

        position = bomb.transform.position;
        position.x = Mathf.Round(position.x);
        position.y = Mathf.Round(position.y);

        AudioManager.instance.PlaySFX("Explosion");

        var explosionObj = PhotonNetwork.Instantiate(_localizacaoExplosion, position, Quaternion.identity);
        var explosion = explosionObj.GetComponent<Explosion>();

        //Explosion Explosion = Instantiate(explosionPrefab, position, Quaternion.identity);
        //explosion.SetActiveRenderer(explosion.start);
        //explosion.DestroyAfter(explosianDuration);

        explosion.photonView.RPC("SetActiveRenderer", RpcTarget.All, "start");
        explosion.photonView.RPC("DestroyAfter", RpcTarget.All, explosianDuration);;

        Explode(position, Vector2.up, explosionRadius);
        Explode(position, Vector2.down, explosionRadius);
        Explode(position, Vector2.left, explosionRadius);
        Explode(position, Vector2.right, explosionRadius);

        PhotonNetwork.Destroy(bomb);

        //Destroy(bomb);
        bombsRemaining++;


    }

    private void Explode(Vector2 position, Vector2 direction, int legth)
    {
        if (legth <= 0)
            return;

        position += direction;

        if (Physics2D.OverlapBox(position, Vector2.one / 2f, 0f, explosionLayerMask))
        {
            MapGeneration.Instancia.photonView.RPC("Destructible", RpcTarget.MasterClient, position.x, position.y);
            return;
        }

        //Explosion explosion = Instantiate(explosionPrefab, position, Quaternion.identity);
        //explosion.SetActiveRenderer(legth > 1 ? explosion.middle : explosion.end);

        var explosionObj = PhotonNetwork.Instantiate(_localizacaoExplosion, position, Quaternion.identity);
        var explosion = explosionObj.GetComponent<Explosion>();

        explosion.photonView.RPC("SetActiveRenderer", RpcTarget.All, legth > 1 ? "middle" : "end");

        //explosion.SetDirection(direction);
        //explosion.DestroyAfter(explosianDuration);

        explosion.photonView.RPC("SetDirection", RpcTarget.All, direction);
        explosion.photonView.RPC("DestroyAfter", RpcTarget.All, explosianDuration);

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
