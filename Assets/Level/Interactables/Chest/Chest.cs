using UnityEngine;
using DG.Tweening;

namespace ColbyDoan
{
    public class Chest : MonoBehaviour
    {
        public int cost = 100;

        [Space]
        [SerializeField] Sprite openedSprite;

        [Header("Dependencies")]
        [SerializeField] Interactable interactable;
        [SerializeField] ItemDropper pickupDropper;
        [SerializeField] SpriteRenderer spriteRenderer;

        void Awake()
        {
            interactable.hoverText = "Open chest (" + cost + "TB)";
        }

        public void OnInteract()
        {
            if (GameStats.TryDeductDataPoints(cost))
            {
                interactable.enabled = false;
                var shake = transform.DOShakePosition(duration: .7f, strength: .4f * Vector3.right, vibrato: 20, randomness: 0);
                shake.onComplete += Open;
                shake.Play();
            }
        }

        public void Open()
        {
            pickupDropper.DropArtifact();
            spriteRenderer.sprite = openedSprite;
        }
    }
}
