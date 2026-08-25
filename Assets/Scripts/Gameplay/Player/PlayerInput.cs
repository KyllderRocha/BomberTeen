using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerMovement))] 
[RequireComponent(typeof(PlayerAnimatorSync))] 
public class PlayerInput : NetworkBehaviour
{
    private PlayerMovement playerMovement;
    private PlayerAnimatorSync playerAnimator;
    
    [Header("Controls")]
    public KeyCode keyUp = KeyCode.W;
    public KeyCode keyDown = KeyCode.S;
    public KeyCode keyLeft = KeyCode.A;
    public KeyCode keyRight = KeyCode.D;
    
    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerAnimator = GetComponent<PlayerAnimatorSync>();
    }
    
    void Update()
    {
        // Only checks keyboard commands if you are the true "owner" (LocalPlayer) of this character.
        if (IsSpawned && IsOwner)
        {
            Vector2 newDirection = Vector2.zero;

            if (Input.GetKey(keyUp))
                newDirection = Vector2.up;
            else if (Input.GetKey(keyDown))
                newDirection = Vector2.down;
            else if (Input.GetKey(keyLeft))
                newDirection = Vector2.left;
            else if (Input.GetKey(keyRight))
                newDirection = Vector2.right;

            playerMovement.SetMoveDirection(newDirection);
            playerAnimator.SetDirection(newDirection);
        }
    }
}