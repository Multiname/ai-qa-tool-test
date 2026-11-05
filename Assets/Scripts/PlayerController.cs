using UnityEngine;
using UnityEngine.InputSystem;

namespace Platformer
{
    /// <summary>
    /// Moves the player horizontally with A/D and jumps with Space.
    /// Requires a Rigidbody2D and a Collider2D on the same GameObject.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public sealed class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField, Min(0f)] private float moveSpeed = 6f;
        [SerializeField, Min(0f)] private float jumpSpeed = 10f;

        [Header("Ground Check")]
        [SerializeField] private Transform groundCheck;
        [SerializeField, Min(0.01f)] private float groundCheckRadius = 0.12f;
        [SerializeField] private LayerMask groundLayers = ~0;

        private readonly Collider2D[] _groundHits = new Collider2D[8];
        private ContactFilter2D _groundFilter;
        private Rigidbody2D _body;
        private Collider2D _ownCollider;
        private float _moveInput;
        private bool _jumpRequested;

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            _ownCollider = GetComponent<Collider2D>();

            if (groundCheck == null)
            {
                var check = new GameObject("GroundCheck");
                check.transform.SetParent(transform, false);
                check.transform.localPosition = new Vector3(0f, -0.55f, 0f);
                groundCheck = check.transform;
            }

            _groundFilter = new ContactFilter2D { useTriggers = false };
            _groundFilter.SetLayerMask(groundLayers);
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                _moveInput = 0f;
                return;
            }

            _moveInput = 0f;
            if (keyboard.aKey.isPressed)
            {
                _moveInput -= 1f;
            }

            if (keyboard.dKey.isPressed)
            {
                _moveInput += 1f;
            }

            if (keyboard.spaceKey.wasPressedThisFrame && IsGrounded())
            {
                _jumpRequested = true;
            }
        }

        private void FixedUpdate()
        {
            var velocity = _body.linearVelocity;
            velocity.x = _moveInput * moveSpeed;

            if (_jumpRequested)
            {
                velocity.y = jumpSpeed;
                _jumpRequested = false;
            }

            _body.linearVelocity = velocity;
        }

        private bool IsGrounded()
        {
            var count = Physics2D.OverlapCircle(
                groundCheck.position,
                groundCheckRadius,
                _groundFilter,
                _groundHits);

            for (var i = 0; i < count; i++)
            {
                var hit = _groundHits[i];
                if (hit != null && hit != _ownCollider && !hit.isTrigger)
                {
                    return true;
                }
            }

            return false;
        }

        private void OnDrawGizmosSelected()
        {
            if (groundCheck == null)
            {
                return;
            }

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
