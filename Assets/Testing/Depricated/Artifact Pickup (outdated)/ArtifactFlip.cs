using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace ColbyDoan
{
    public class ArtifactFlip : MonoBehaviour
    {
        [SerializeField] ItemRenderer itemRenderer;
        [SerializeField] ArtifactPickup pickup;

        void Start()
        {
            UpdateRenderer();
            transform.rotation = Quaternion.LookRotation(Vector3.left, Vector3.up);
        }

        void FlipLoop()
        {
            itemRenderer.SetItem(pickup.trigger);
            transform.DORotate(Vector3.up * 90, 1, RotateMode.FastBeyond360).SetEase(Ease.Linear).onComplete += () =>
            {
                itemRenderer.SetItem(pickup.effect);
                transform.DORotate(Vector3.up * 270, 1, RotateMode.FastBeyond360).SetEase(Ease.Linear).onComplete += FlipLoop;
            };
        }

        [ContextMenu("Update Renderer")]
        void UpdateRenderer()
        {
            itemRenderer.SetItem(pickup.trigger);
        }

        void OnDisable()
        {
            transform.DOKill();
        }

        void OnEnable()
        {
            FlipLoop();
        }
    }
}
