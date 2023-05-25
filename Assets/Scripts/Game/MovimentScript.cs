using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovimentScript : MonoBehaviourPunCallbacks
{
    public Rigidbody2D Rigidbody { get; private set; }
    private Vector2 direction = Vector2.down;
    public float speed = 5f;

    public KeyCode inputUp = KeyCode.W;
    public KeyCode inputDown = KeyCode.S;
    public KeyCode inputLeft = KeyCode.A;
    public KeyCode inputRight = KeyCode.D;

    public AnimatedSpriteRenderer spriteRendererUp; 
    public AnimatedSpriteRenderer spriteRendererDown; 
    public AnimatedSpriteRenderer spriteRendererLeft; 
    public AnimatedSpriteRenderer spriteRendererRight; 
    public AnimatedSpriteRenderer spriteRendererDeath; 
    private AnimatedSpriteRenderer activeSpriteRenderer;

    private Player _photonPlayer;
    public int _id;

    [PunRPC]
    public void Inicializa(Player player)
    {
        _photonPlayer = player;
        _id = player.ActorNumber;
        GameManager.Instancia.Jogadores.Add(this);

        if(!photonView.IsMine)
            Rigidbody.isKinematic = false;
    }

    private void Awake()
    {
        Rigidbody = GetComponent<Rigidbody2D>();
        activeSpriteRenderer = spriteRendererDown;
        AudioManager.instance.PlayMusic("Battle");
    }

    void Update()
    {
        if (photonView.IsMine)
        {
            if (Input.GetKey(inputUp))
                SetDirection(Vector2.up, spriteRendererUp);
            else if (Input.GetKey(inputDown))
                SetDirection(Vector2.down, spriteRendererDown);
            else if (Input.GetKey(inputLeft))
                SetDirection(Vector2.left, spriteRendererLeft);
            else if (Input.GetKey(inputRight))
                SetDirection(Vector2.right, spriteRendererRight);
            else
                SetDirection(Vector2.zero, activeSpriteRenderer);
        }
    }

    void FixedUpdate()
    {
        Vector2 position = Rigidbody.position;
        Vector2 translation = speed * Time.fixedDeltaTime * direction;

        Rigidbody.MovePosition(position + translation);
    }

    public void SetDirection(Vector2 newDirection, AnimatedSpriteRenderer spriteRenderer)
    {
        photonView.RPC("changeSprite", RpcTarget.All, _id, newDirection);
    }

    [PunRPC]
    public void changeSprite(int id, Vector2 newDirection)
    {
        if (_id == id)
        {
            AnimatedSpriteRenderer spriteRenderer = activeSpriteRenderer;
            direction = newDirection;

            if (newDirection == Vector2.up)
                spriteRenderer = spriteRendererUp;
            else if (newDirection == Vector2.down)
                spriteRenderer = spriteRendererDown;
            else if (newDirection == Vector2.left)
                spriteRenderer = spriteRendererLeft;
            else if (newDirection == Vector2.right)
                spriteRenderer = spriteRendererRight;

            spriteRendererUp.enabled = spriteRenderer == spriteRendererUp;
            spriteRendererDown.enabled = spriteRenderer == spriteRendererDown;
            spriteRendererLeft.enabled = spriteRenderer == spriteRendererLeft;
            spriteRendererRight.enabled = spriteRenderer == spriteRendererRight;

            activeSpriteRenderer = spriteRenderer;
            activeSpriteRenderer.idle = direction == Vector2.zero;
        }
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
        GetComponent<BombController>().enabled = false;

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
        GameManager.Instancia.CheckWinState();
    }
}
