using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    /// <summary>
    /// Local reference to the body's physics engine.
    /// </summary>
    public Rigidbody2D Rigidbody { get; private set; }
    
    // Vector that stores the direction the character is pointing/walking towards.
    private Vector2 direction = Vector2.down;
    
    [Tooltip("Movement speed of the player.")]
    public float speed = 5f;
    
    private void Awake()
    {
        Rigidbody = GetComponent<Rigidbody2D>();

    }

    public override void OnNetworkSpawn()
    {
        // Handles gravity and collisions of this physical body in case it's just a clone on another PC's screen.
        if(!IsOwner)
            Rigidbody.isKinematic = false;
    }

    void FixedUpdate()
    {
        // Applies translation force to the physics engine (Rigidbody) at a constant, framerate-independent pace

        Vector2 position = Rigidbody.position;
        Vector2 translation = speed * Time.fixedDeltaTime * direction;

        Rigidbody.MovePosition(position + translation);
    }

    public void SetMoveDirection(Vector2 newDirection)
    {
        direction = newDirection;
    }
}