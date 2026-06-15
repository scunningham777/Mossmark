using Mossmark.Day;
using Mossmark.Development;
using Mossmark.Inventory;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Mossmark.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 4f;
        [SerializeField] private float smoothing = .1f;

        private Rigidbody2D rb;
        private InputAction moveAction;
        private Vector2 movement;
        private Vector2 currentVelocity;

        private Vector2 lastMoveDirection = Vector2.down;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();

            rb.gravityScale = 0f;
            rb.freezeRotation = true;

            if (GetComponent<Collider2D>() == null)
            {
                gameObject.AddComponent<CircleCollider2D>().isTrigger = true;
            }

            gameObject.tag = "Player";

            var gameplay = InputSystem.actions.FindActionMap("Gameplay");
            gameplay.Enable();
            moveAction = gameplay.FindAction("Move");
        }

        private void Update()
        {
            HandleInput();
        }

        private void FixedUpdate()
        {
            HandleMovement();
        }

        private void HandleInput()
        {
            // Bedroll's Rest() locks movement (and attention, via AttentionManager) for
            // the duration of the day-transition fade.
            if (DayCycleManager.Instance != null && DayCycleManager.Instance.IsTransitioning)
            {
                movement = Vector2.zero;
                return;
            }

            // The chest menu (ChestUI) owns input while open.
            if (ChestUI.Instance != null && ChestUI.Instance.IsOpen)
            {
                movement = Vector2.zero;
                return;
            }

            // The Horizon panel (HorizonUI) owns input while open.
            if (HorizonUI.Instance != null && HorizonUI.Instance.IsOpen)
            {
                movement = Vector2.zero;
                return;
            }

            movement = moveAction.ReadValue<Vector2>().normalized;

            if (movement != Vector2.zero)
            {
                lastMoveDirection = movement;
            }
        }

        private void HandleMovement()
        {
            Vector2 targetVelocity = movement * moveSpeed;
            rb.linearVelocity = Vector2.SmoothDamp(rb.linearVelocity, targetVelocity, ref currentVelocity, smoothing);
        }

        private void OnDisable()
        {
            movement = Vector2.zero;
            if (rb != null) rb.linearVelocity = Vector2.zero;
        }

        // Animation helpers
        public Vector2 GetMoveDirection() => movement;
        public Vector2 GetLastMoveDirection() => lastMoveDirection;
        public bool IsMoving() => movement.magnitude > 0.1f;

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, (Vector2)transform.position + lastMoveDirection);
        }
    }
}
