using UnityEngine;

namespace ColbyDoan
{
    using Physics;

    /// <summary> Finds interactables by distance </summary>
    public class InteractablesFinder : MonoBehaviour
    {
        [SerializeField] float interactionRadius = 2;

        public Interactable ClosestInteractable { get; private set; }

        void Update()
        {
            // searches through list of interactables for closest in range
            Interactable oldInteractable = ClosestInteractable;
            ClosestInteractable = null;
            float closestDist = interactionRadius;
            Vector3 selfPos = transform.position;
            foreach (Interactable interactable in Interactable.interactables)
            {
                // if (!interactable.interactable)
                //     continue;
                Vector3 interactablePos = interactable.transform.position;
                float distance = Vector2.Distance(interactablePos, selfPos);
                if (distance < closestDist)
                {
                    // check for blocking solids, unless solid is the interactable
                    RaycastHit2D hit = PhysicsSettings.SolidsLinecast(selfPos, interactablePos);
                    if (!hit || hit.transform == interactable.transform)
                    {
                        ClosestInteractable = interactable;
                        closestDist = distance;
                    }
                }
            }

            // detect approach and leaving
            if (oldInteractable != ClosestInteractable)
            {
                oldInteractable?.OnPlayerLeave();
                ClosestInteractable?.OnPlayerApproach();
            }

            // set interactable text
            if (ClosestInteractable)
                InteractionHUD.SetInteractionText("E: " + ClosestInteractable.hoverText);
            else
                InteractionHUD.SetInteractionText("");
        }
    }
}