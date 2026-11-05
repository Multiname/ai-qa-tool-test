using UnityEngine;

namespace Platformer
{
    /// <summary>
    /// Keeps the camera focused on the player while preserving the configured offset.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 1f, -10f);
        [SerializeField, Min(0f)] private float smoothTime = 0.08f;

        private Vector3 _velocity;

        public Transform Target
        {
            get => target;
            set => target = value;
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            var desiredPosition = target.position + offset;

            transform.position = smoothTime > 0f
                ? Vector3.SmoothDamp(transform.position, desiredPosition, ref _velocity, smoothTime)
                : desiredPosition;
        }

        private void OnValidate()
        {
            smoothTime = Mathf.Max(0f, smoothTime);
        }
    }
}
