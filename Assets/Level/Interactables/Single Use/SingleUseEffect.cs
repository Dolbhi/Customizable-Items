using System.Collections;
// using System.Collections.Generic;
using UnityEngine;

namespace ColbyDoan
{
    using Attributes;

    [RequireComponent(typeof(Interactable))]
    public class SingleUseEffect : MonoBehaviour
    {
        public ArtifactPools effectPool;
        // more varied costs in the future
        public int cost;
        public int level = 1;
        public ItemRank rank;

        [SerializeField] Item effectInfo;

        const float DESTROY_EFFECT_SECONDS = 60;

        [SerializeField][ReadOnly] Interactable _interactable;
        [SerializeField][ReadOnly] ItemRenderer _itemRenderer;

        Item _last;
        void OnValidate()
        {
            _interactable = GetComponent<Interactable>();
            _itemRenderer = GetComponentInChildren<ItemRenderer>();

            // EFFECT MUST BE EFFECT
            if (effectInfo && effectInfo.type != ItemType.Effect) effectInfo = null;

            // copy item if set manually and not null
            if (effectInfo != _last)
            {
                if (effectInfo)
                {
                    effectInfo = effectInfo.Copy();
                }
                _last = effectInfo;
            }
        }

        void Awake()
        {
            // generate effect if needed
            if (effectInfo == null)
            {
                if (effectPool == null)
                {
                    Debug.LogWarning("Missing pool to generate item", this);
                }
                else
                {
                    // non targeting effect of specified rank
                    effectInfo = effectPool.GetRandomItem(false, false, rank);
                }
            }
            // set text
            _interactable.hoverText = $"Trigger {effectInfo.name} ({cost}TB)";
            // set item
            _itemRenderer.SetItem(effectInfo);
        }

        public void OnInteract(PlayerBehaviour playerBehaviour)
        {
            if (effectInfo == null)
            {
                Debug.LogWarning("Missing effect", this);
                return;
            }

            if (GameStats.TryDeductDataPoints(cost))
            {
                // set-up effect
                var af = ArtifactManager.FindFromRoot(playerBehaviour.transform.root);
                Effect effect = ArtifactFactory.effectFactory.GetItem(effectInfo.idName);
                effect.SetUp(af);
                effect.SetLevel(level);
                // trigger
                effect.Trigger(new TriggerContext(af.transform.position));
                // destroy
                StartCoroutine(DestroyLaterCoroutine(effect));

                // clear self
                _interactable.enabled = false;
                _itemRenderer.SetEmpty();
            }
        }

        WaitForSeconds destroyDelay = new WaitForSeconds(DESTROY_EFFECT_SECONDS);
        // IS THERE BUGS IF BEHAVIOUR IS DESTORYED BEFORE COROUTINE FINISHES?
        IEnumerator DestroyLaterCoroutine(Effect effect)
        {
            yield return destroyDelay;
            effect.OnDestroy();
        }
    }
}
