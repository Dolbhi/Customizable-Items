using UnityEngine;

namespace ColbyDoan
{
    /// <summary> Finds interactables by distance </summary>
    public class InteractablesFinder : MonoBehaviour
    {
        [SerializeField] float interactionRadius = 2;

        public Interactable ClosestInteractable { get; private set; }

        void Update()
        {
            // searches through list of interactables for closest in range
            ClosestInteractable = null;
            float closestDist = interactionRadius;
            foreach (Interactable interactable in Interactable.interactables)
            {
                // if (!interactable.interactable)
                //     continue;
                float distance = Vector2.Distance(interactable.transform.position, transform.position);
                if (distance > closestDist)
                    continue;

                ClosestInteractable = interactable;
                closestDist = distance;
            }

            // set interactable text
            if (ClosestInteractable)
                InteractionHUD.SetInteractionText(ClosestInteractable.hoverText);
            else
                InteractionHUD.SetInteractionText("");
        }
    }
}