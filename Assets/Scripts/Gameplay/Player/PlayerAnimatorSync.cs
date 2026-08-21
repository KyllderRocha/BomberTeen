using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BomberTeen;

public class PlayerAnimatorSync : MonoBehaviourPunCallbacks
{
    [Header("Visual Animations")]
    public AnimatedSpriteRenderer spriteRendererUp; 
    public AnimatedSpriteRenderer spriteRendererDown; 
    public AnimatedSpriteRenderer spriteRendererLeft; 
    public AnimatedSpriteRenderer spriteRendererRight; 
    public AnimatedSpriteRenderer spriteRendererDeath; 
    
    // Stores the rendering script that is currently being used (e.g. Walk Down Animation)
    private AnimatedSpriteRenderer activeSpriteRenderer;
    private Vector2 direction = Vector2.down;
    
    private void Awake()
    {
        activeSpriteRenderer = spriteRendererDown;
    }

    /// <summary>
    /// Sends the movement command over the network to change the animation on other players' screens.
    /// </summary>
    public void SetDirection(Vector2 newDirection)
    {
        // Only sends to the network if the direction actually changed (Optimization)
        if (direction == newDirection) return;
        
        photonView.RPC(Constants.RPC.ChangeSprite, RpcTarget.All, newDirection);
    }

    /// <summary>
    /// RPC called simultaneously on all PCs in the room. 
    /// Processes the visual change of the character's sprites according to the direction they walked.
    /// </summary>
    [PunRPC]
    public void ChangeSprite(Vector2 newDirection)
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

            // Turns off the rendering of animations that are not active in the corresponding direction
            spriteRendererUp.enabled = spriteRenderer == spriteRendererUp;
            spriteRendererDown.enabled = spriteRenderer == spriteRendererDown;
            spriteRendererLeft.enabled = spriteRenderer == spriteRendererLeft;
            spriteRendererRight.enabled = spriteRenderer == spriteRendererRight;

            activeSpriteRenderer = spriteRenderer;
            
            // Activates the idle version of the character if the direction vector is (0, 0)
            activeSpriteRenderer.idle = direction == Vector2.zero;
    }
}