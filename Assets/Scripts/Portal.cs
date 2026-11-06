using UnityEngine;

namespace Platformer
{
    /// <summary>
    /// The level exit. It is hidden until every coin is collected and shows the
    /// victory screen when the player touches it.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider2D))]
    public sealed class Portal : MonoBehaviour
    {
        [SerializeField] private GameObject victoryScreen;

        private bool _victoryShown;

        public bool IsOpen => gameObject.activeSelf;

        public void Open()
        {
            gameObject.SetActive(true);
        }

        private void Reset()
        {
            var trigger = GetComponent<Collider2D>();
            trigger.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_victoryShown || other.GetComponentInParent<PlayerController>() == null)
            {
                return;
            }

            _victoryShown = true;
            GameLogger.LogPortalEntered();

            if (victoryScreen != null)
            {
                victoryScreen.SetActive(true);
            }
        }
    }
}
