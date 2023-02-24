// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;

namespace ColbyDoan.Physics
{
    public class RaycastController2D : MonoBehaviour
    {
        public new BoxCollider2D collider;

        public const float skinWidth = .02f;
        const float distBetweenRays = .125f;
        protected int horizontalRayCount;
        protected int verticalRayCount;
        protected LayerMask collisionMask;

        protected float horizontalRaySpacing;
        protected float verticalRaySpacing;

        public RaycastOrigins GetRaycastOrigins { get => raycastOrigins; }
        protected RaycastOrigins raycastOrigins;

        public const float height = 1;

        protected virtual void Awake()
        {
            collisionMask = PhysicsSettings.solids;
            CalculateRaySpacing();
        }

        protected void UpdateRaycastOrigins()
        {
            Bounds bound = collider.bounds;
            bound.Expand(-2 * skinWidth);

            raycastOrigins.bottomLeft = bound.min;
            raycastOrigins.bottomRight = new Vector2(bound.max.x, bound.min.y);
            raycastOrigins.topLeft = new Vector2(bound.min.x, bound.max.y);
            raycastOrigins.topRight = bound.max;

            raycastOrigins.lowest = Mathf.Min(transform.position.z + skinWidth, 2 - skinWidth);
            raycastOrigins.highest = transform.position.z + height - skinWidth;

            raycastOrigins.bound = bound;
        }

        protected void CalculateRaySpacing()
        {
            Bounds bound = collider.bounds;
            bound.Expand(-2 * skinWidth);

            horizontalRayCount = Mathf.Max(2, Mathf.RoundToInt(bound.size.y / distBetweenRays));
            verticalRayCount = Mathf.Max(2, Mathf.RoundToInt(bound.size.x / distBetweenRays));

            horizontalRaySpacing = bound.size.y / (horizontalRayCount - 1);
            verticalRaySpacing = bound.size.x / (verticalRayCount - 1);
        }

        public struct RaycastOrigins
        {
            public Vector2 topLeft, topRight, bottomLeft, bottomRight;
            public Bounds bound;
            public float highest, lowest;
        }
    }
}