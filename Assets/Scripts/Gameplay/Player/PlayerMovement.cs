using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    /// <summary>
    /// Local reference to the body's physics engine.
    /// </summary>
    public Rigidbody2D Rigidbody { get; private set; }
    
    // Vector that stores the direction the character is pointing/walking towards.
    private Vector2 direction = Vector2.zero;
    
    [Tooltip("Movement speed of the player.")]
    public float speed = 5f;

    // NetworkVariables to synchronize position and movement direction continuously
    private readonly NetworkVariable<Vector2> networkPosition = new NetworkVariable<Vector2>(
        default,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    private readonly NetworkVariable<Vector2> networkDirection = new NetworkVariable<Vector2>(
        default,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    private PlayerAnimatorSync playerAnimator;

    private void Awake()
    {
        Rigidbody = GetComponent<Rigidbody2D>();
        playerAnimator = GetComponent<PlayerAnimatorSync>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            Rigidbody.bodyType = RigidbodyType2D.Dynamic;
            networkPosition.Value = Rigidbody.position;
            networkDirection.Value = Vector2.zero;
        }
        else
        {
            // Non-owners are kinematic so local physics don't fight network position updates
            Rigidbody.bodyType = RigidbodyType2D.Kinematic;
            Rigidbody.useFullKinematicContacts = true;

            if (networkPosition.Value != Vector2.zero)
            {
                Rigidbody.position = networkPosition.Value;
            }

            // Sync initial direction and idle state immediately
            if (playerAnimator != null)
            {
                playerAnimator.ApplyDirectionVisuals(networkDirection.Value);
            }

            // Listen for direction changes to sync animations on remote clients
            networkDirection.OnValueChanged += HandleNetworkDirectionChanged;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner)
        {
            networkDirection.OnValueChanged -= HandleNetworkDirectionChanged;
        }
    }

    private void HandleNetworkDirectionChanged(Vector2 oldDir, Vector2 newDir)
    {
        if (playerAnimator != null)
        {
            playerAnimator.ApplyDirectionVisuals(newDir);
        }
    }

    private void FixedUpdate()
    {
        if (IsOwner)
        {
            Vector2 position = Rigidbody.position;
            Vector2 translation = speed * Time.fixedDeltaTime * direction;
            Rigidbody.MovePosition(position + translation);
            networkPosition.Value = Rigidbody.position;
        }
        else
        {
            // Smoothly interpolate remote player to network position
            float dist = Vector2.Distance(Rigidbody.position, networkPosition.Value);
            if (dist > 2f)
            {
                Rigidbody.position = networkPosition.Value;
            }
            else
            {
                Rigidbody.position = Vector2.Lerp(Rigidbody.position, networkPosition.Value, Time.fixedDeltaTime * 20f);
            }
        }
    }

    public void SetMoveDirection(Vector2 newDirection)
    {
        direction = newDirection;
        if (IsOwner && IsSpawned)
        {
            if (networkDirection.Value != newDirection)
            {
                networkDirection.Value = newDirection;
            }
        }
    }
}