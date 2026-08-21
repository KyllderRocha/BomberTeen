using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Custom and extremely lightweight script to swap images frame by frame using InvokeRepeating,
/// replacing Unity's native "Animator" component.
/// Excellent for the project's retro aesthetics (simple Sprite Sheets).
/// </summary>
public class AnimatedSpriteRenderer : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    [Header("Sprite Settings")]
    [Tooltip("Static image shown when the object is stopped.")]
    public Sprite idleSprite;
    
    [Tooltip("List (Array) with the images that form the movement animation in order.")]
    public Sprite[] animationSprites;

    [Tooltip("Time in seconds between each image swap.")]
    public float animationTime = 0.25f;
    private int animationFrame;

    public bool loop = true;
    public bool idle = true;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // If the script is enabled in the hierarchy (enabled=true), ensures the internal SpriteRenderer turns on.
    private void OnEnable()
    {
        spriteRenderer.enabled = true;
    }

    // If the script is disabled by PlayerMovement, hides the sprite.
    private void OnDisable()
    {
        spriteRenderer.enabled = false;
    }

    private void Start()
    {
        // Starts calling the NextFrame function repeatedly at the rate configured in 'animationTime'.
        InvokeRepeating(nameof(NextFrame), animationTime, animationTime);
    }

    /// <summary> Updates the animation frame by mathematically iterating through the Sprite array. </summary>
    private void NextFrame()
    {
        animationFrame++;

        // Resets the count if it reached the end of the array and the 'loop' option is on.
        if (loop && animationFrame >= animationSprites.Length)
            animationFrame = 0;

        // If the entity stopped moving (idle=true), locks it to the idleSprite.
        if (idle)
            spriteRenderer.sprite = idleSprite;
        else if (animationFrame >= 0 && animationFrame < animationSprites.Length)
            spriteRenderer.sprite = animationSprites[animationFrame];
    }
}
