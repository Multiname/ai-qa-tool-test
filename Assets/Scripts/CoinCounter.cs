using UnityEngine;
using UnityEngine.UI;

namespace Platformer
{
    /// <summary>
    /// Keeps the HUD counter in sync with the coins collected in the scene.
    /// The total is discovered from the active coins when the scene starts.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CoinCounter : MonoBehaviour
    {
        [SerializeField] private Text counterLabel;
        [SerializeField] private Portal portal;

        private int _total;
        private int _collected;

        public int Total => _total;
        public int Collected => _collected;
        public int Remaining => Mathf.Max(0, _total - _collected);

        private void Awake()
        {
            if (counterLabel == null)
            {
                counterLabel = GetComponentInChildren<Text>(true);
            }

            if (portal == null)
            {
                portal = FindAnyObjectByType<Portal>(FindObjectsInactive.Include);
            }

            _total = FindObjectsByType<Coin>(FindObjectsInactive.Exclude).Length;
            Refresh();
        }

        public void Collect()
        {
            if (Remaining <= 0)
            {
                return;
            }

            _collected++;
            Refresh();

            if (Remaining == 0)
            {
                portal?.Open();
            }
        }

        public void Refresh()
        {
            if (counterLabel == null)
            {
                return;
            }

            counterLabel.text = $"Собрано: {_collected}   Осталось: {Remaining}";
        }
    }
}
