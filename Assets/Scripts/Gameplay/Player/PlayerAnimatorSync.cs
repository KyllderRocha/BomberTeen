using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BomberTeen;

public class PlayerAnimatorSync : NetworkBehaviour
{
    [Header("Visual Animations")]
    public AnimatedSpriteRenderer spriteRendererUp; 
    public AnimatedSpriteRenderer spriteRendererDown; 
    public AnimatedSpriteRenderer spriteRendererLeft; 
    public AnimatedSpriteRenderer spriteRendererRight; 
    public AnimatedSpriteRenderer spriteRendererDeath; 
    
    // Stores the rendering script that is currently being used (e.g. Walk Down Animation)
    private AnimatedSpriteRenderer activeSpriteRenderer;
    private Vector2 lastFacingDirection = Vector2.down;
    private Vector2 currentDirection = Vector2.zero;
    
    private void Awake()
    {
        activeSpriteRenderer = spriteRendererDown;
        ApplyDirectionVisuals(Vector2.zero);
    }

    /// <summary>
    /// Sends the movement command over the network to change the animation on other players' screens.
    /// </summary>
    public void SetDirection(Vector2 newDirection)
    {
        if (currentDirection == newDirection) return;
        currentDirection = newDirection;
        
        ApplyDirectionVisuals(newDirection);
    }

    public void ApplyDirectionVisuals(Vector2 newDirection)
    {
        currentDirection = newDirection;
        bool isMoving = (newDirection != Vector2.zero);

        if (newDirection == Vector2.up || newDirection == Vector2.down || 
            newDirection == Vector2.left || newDirection == Vector2.right)
        {
            lastFacingDirection = newDirection;
        }

        AnimatedSpriteRenderer targetRenderer = spriteRendererDown;
        if (lastFacingDirection == Vector2.up)
            targetRenderer = spriteRendererUp;
        else if (lastFacingDirection == Vector2.down)
            targetRenderer = spriteRendererDown;
        else if (lastFacingDirection == Vector2.left)
            targetRenderer = spriteRendererLeft;
        else if (lastFacingDirection == Vector2.right)
            targetRenderer = spriteRendererRight;

        // Turns off the rendering of animations that are not active in the corresponding direction
        if (spriteRendererUp != null) spriteRendererUp.enabled = (targetRenderer == spriteRendererUp);
        if (spriteRendererDown != null) spriteRendererDown.enabled = (targetRenderer == spriteRendererDown);
        if (spriteRendererLeft != null) spriteRendererLeft.enabled = (targetRenderer == spriteRendererLeft);
        if (spriteRendererRight != null) spriteRendererRight.enabled = (targetRenderer == spriteRendererRight);

        activeSpriteRenderer = targetRenderer;
        
        // Activates the idle version of the character if the direction vector is (0, 0)
        if (activeSpriteRenderer != null)
        {
            activeSpriteRenderer.idle = !isMoving;
        }
    }

    /// <summary>
    /// RPC called simultaneously on all PCs in the room. 
    /// Processes the visual change of the character's sprites according to the direction they walked.
    /// </summary>
    [Rpc(SendTo.ClientsAndHost)]
    public void ChangeSpriteRpc(Vector2 newDirection)
    {
        ApplyDirectionVisuals(newDirection);
    }
}