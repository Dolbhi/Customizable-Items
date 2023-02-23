// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;

namespace ColbyDoan
{
    using Physics;

    public class EntityShadows : MonoBehaviour
    {
        /* Sprite settings:
         * sort from center
         * position just above sprite
         * NOTE: if the kinematic object doesnt move in height it can't detect the ground
        */

        public SpriteRenderer spriteRenderer;
        public Vector2 posFromRoot;
        public bool forceVisible;

        [ContextMenu("Update")]
        public void LateUpdate()
        {
            transform.position = transform.root.position + (Vector3)posFromRoot;

            if (forceVisible)
            {
                spriteRenderer.SetSpriteAlpha(1);
                return;
            }

            RaycastHit2D hit = Physics2D.GetRayIntersection(new Ray(transform.position, Vector3.back), 2);

            if (hit)
            {
                float floorZ = hit.collider.transform.position.z + .02f;

                // fade based on distance from floor
                spriteRenderer.SetSpriteAlpha(Mathf.Clamp01((2 - transform.position.z + floorZ) / 2));

                transform.SetZPosition(floorZ);
                transform.position += floorZ * PhysicsSettings.depthToHeightMultiplier * Vector3.up;
            }
            else
            {
                spriteRenderer.SetSpriteAlpha(0);
            }

            //if (transform.parent.position.z < -0.5f)
            //    spriteRenderer.sortingOrder = 1;
            //else
            //    spriteRenderer.sortingOrder = 0;
        }

        void OnValidate()
        {
            LateUpdate();
        }
    }
}