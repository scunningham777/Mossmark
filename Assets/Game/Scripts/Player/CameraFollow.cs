using UnityEngine;

namespace Mossmark.Player
{
    public class CameraFollow : MonoBehaviour
    {
        [Header("Follow Settings")]
        [SerializeField] private Transform target;
        [SerializeField] private float smoothTime = 0.3f;
        [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f); // Default orthographic offset

        private Vector3 velocity = Vector3.zero;

        private void LateUpdate()
        {
            if (target == null)
            {
                Debug.LogWarning("CameraFollow: No target assigned!");
                return;
            }

            // Calculate the target position with offset
            Vector3 targetPosition = target.position + offset;

            // Smoothly move the camera towards the target position
            transform.position = Vector3.SmoothDamp(
                transform.position,
                targetPosition,
                ref velocity,
                smoothTime);
        }

        // Call this method if you need to change the follow target at runtime
        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }
    }
}
