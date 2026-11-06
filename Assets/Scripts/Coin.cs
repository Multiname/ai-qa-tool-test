using UnityEngine;

namespace Platformer
{
    /// <summary>
    /// A collectible coin. The player picks it up by touching its trigger collider.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider2D))]
    public sealed class Coin : MonoBehaviour
    {
        [SerializeField] private CoinCounter counter;

        private bool _collected;

        private void Awake()
        {
            if (counter == null)
            {
                counter = FindAnyObjectByType<CoinCounter>();
            }
        }

        private void Reset()
        {
            var trigger = GetComponent<Collider2D>();
            trigger.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_collected || other.GetComponentInParent<PlayerController>() == null)
            {
                return;
            }

            _collected = true;

            if (counter == null)
            {
                counter = FindAnyObjectByType<CoinCounter>();
            }

            if (counter != null)
            {
                counter.Collect(gameObject.name);
            }
            else
            {
                GameLogger.LogCoinCollected(gameObject.name, 0, 0);
            }

            gameObject.SetActive(false);
        }
    }
}
