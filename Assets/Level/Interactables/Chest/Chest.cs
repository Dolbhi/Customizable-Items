using UnityEngine;
using DG.Tweening;

namespace ColbyDoan
{
    public class Chest : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] Interactable interactable;
        [SerializeField] ItemDropper pickupDropper;
        [SerializeField] SpriteRenderer spriteRenderer;

        [Space]
        [SerializeField] Sprite openedSprite;

        public void OnInteract()
        {
            interactable.enabled = false;
            var shake = transform.DOShakePosition(duration: .7f, strength: .4f * Vector3.right, vibrato: 20, randomness: 0);
            shake.onComplete += Open;
            shake.Play();
        }

        public void Open()
        {
            pickupDropper.DropArtifact();
            spriteRenderer.sprite = openedSprite;
        }
    }
}
