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

    [UnityEngine.Serialization.FormerlySerializedAs("idle")]
    [SerializeField] private bool _idle = false;
    public bool idle
    {
        get => _idle;
        set
        {
            _idle = value;
            if (_idle)
            {
                animationFrame = 0;
                if (spriteRenderer != null && idleSprite != null)
                {
                    spriteRenderer.sprite = idleSprite;
                }
            }
            else
            {
                if (spriteRenderer != null && animationSprites != null && animationSprites.Length > 0)
                {
                    if (animationFrame < 0 || animationFrame >= animationSprites.Length)
                    {
                        animationFrame = 0;
                    }
                    spriteRenderer.sprite = animationSprites[animationFrame];
                }
            }
        }
    }

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }

    // If the script is enabled in the hierarchy (enabled=true), ensures the internal SpriteRenderer turns on and starts animation.
    private void OnEnable()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }

        animationFrame = 0;

        if (idle)
        {
            if (spriteRenderer != null && idleSprite != null)
            {
                spriteRenderer.sprite = idleSprite;
            }
        }
        else
        {
            if (spriteRenderer != null && animationSprites != null && animationSprites.Length > 0)
            {
                spriteRenderer.sprite = animationSprites[0];
            }
        }

        CancelInvoke(nameof(NextFrame));
        InvokeRepeating(nameof(NextFrame), animationTime, animationTime);
    }

    // If the script is disabled by PlayerMovement, stops invoke and hides the sprite.
    private void OnDisable()
    {
        CancelInvoke(nameof(NextFrame));
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }
    }

    private void Start()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }

    /// <summary> Updates the animation frame by mathematically iterating through the Sprite array. </summary>
    private void NextFrame()
    {
        if (idle)
        {
            if (spriteRenderer != null && idleSprite != null)
            {
                spriteRenderer.sprite = idleSprite;
            }
            return;
        }

        if (animationSprites == null || animationSprites.Length == 0) return;

        animationFrame++;

        // Resets the count if it reached the end of the array and the 'loop' option is on.
        if (loop && animationFrame >= animationSprites.Length)
        {
            animationFrame = 0;
        }

        if (animationFrame >= 0 && animationFrame < animationSprites.Length && spriteRenderer != null)
        {
            spriteRenderer.sprite = animationSprites[animationFrame];
        }
    }
}
