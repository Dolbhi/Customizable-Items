using UnityEngine;
using System;
using System.Collections.Generic;

using ColbyDoan.Attributes;

namespace ColbyDoan.Physics
{
    /// <summary>
    /// Manages movement with collisions
    /// </summary>
    public class Controller2D : RaycastController2D
    {
        [HideInInspector]
        bool collisionsEnabled = true;
        public bool CollisionsEnabled => collisionsEnabled;

        [ReadOnly]
        public CollisionInfo collisions;

        public event Action OnSolidCollisionEnter;

        public void SetCollisionsEnabled(bool set)
        {
            collisionsEnabled = set;
            collider.enabled = set;
        }

        // unintentional movement
        public CollisionInfo Move(Vector3 move, bool pushed = false)// intentional movement
        {
            if (!collisionsEnabled)
            {
                transform.Translate(move, Space.World);
                collisions.Reset();
                return collisions;
            }

            UpdateRaycastOrigins();
            CollisionInfo oldCollisions = collisions;
            collisions.Reset();

            // Collision detection
            if (pushed)
            {
                collisions.left = move.x > 0;
                collisions.right = move.x < 0;
                collisions.up = move.y < 0;
                collisions.down = move.y > 0;
            }

            if (move.x != 0)
            {
                switch (HorizontalCollisions(ref move))
                {
                    case 1:
                        collisions.right = true;
                        break;
                    case -1:
                        collisions.left = true;
                        break;
                }
            }
            if (move.y != 0)
            {
                switch (VerticalCollisions(ref move))
                {
                    case 1:
                        collisions.up = true;
                        break;
                    case -1:
                        collisions.down = true;
                        break;
                }
            }
            if (move.z != 0)
            {
                if (move.z > 0)
                {
                    collisions.above = CeilingCollisions(ref move);
                }
                else
                {
                    collisions.grounded = GroundCollisions(ref move, out collisions.floor);
                }
            }
            //else if (transform.position.z == 0)
            //    collisions.grounded = true;

            transform.Translate(move, Space.World);

            // On collision enter
            if ((collisions.up && !oldCollisions.up) || (collisions.down && !oldCollisions.down) || (collisions.right && !oldCollisions.right) || (collisions.left && !oldCollisions.left))
            {
                //print(name + " rigid collided");
                collisions.collidedThisFrame = true;
                OnSolidCollisionEnter?.Invoke();
            }
            return collisions;
        }

        // cast collider in direction and returns if there is any collisions, move is also truncated appropriately
        public bool Cast(ref Vector3 move)
        {
            bool result = false;
            if (move.x != 0)
                result |= HorizontalCollisions(ref move) != 0;
            if (move.y != 0)
                result |= VerticalCollisions(ref move) != 0;
            if (move.z != 0)
                result |= (move.z < 0) ? GroundCollisions(ref move, out _) : CeilingCollisions(ref move);
            return result;
        }
        public int OverlapCollider(LayerMask layerMask, List<Collider2D> results)
        {
            return collider.OverlapCollider(PhysicsSettings.GetFilter(layerMask, transform.position, above: height), results);
        }

        int HorizontalCollisions(ref Vector3 move)
        {
            int directionX = (int)Mathf.Sign(move.x); ;
            float rayLength = Mathf.Abs(move.x) + skinWidth;
            //if (Mathf.Abs(move.x) < skinWidth)
            //{
            //    rayLength = 2 * skinWidth;
            //}

            int result = 0;

            for (int i = 0; i < horizontalRayCount; i++)// for each ray
            {
                // cast ray
                Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
                rayOrigin += Vector2.up * (horizontalRaySpacing * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask, raycastOrigins.lowest, raycastOrigins.highest + 1);

                Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.red);

                // collision
                if (hit)
                {
                    // if (transform.root.name == "Axe") print("hColl!: " + hit.collider.name);

                    move.x = (hit.distance - skinWidth) * directionX;
                    rayLength = hit.distance;

                    result = directionX;

                    collisions.right = directionX == 1;
                    collisions.left = directionX == -1;
                }
            }
            return result;
        }
        int VerticalCollisions(ref Vector3 move)
        {
            int directionY = (int)Mathf.Sign(move.y);
            float rayLength = Mathf.Abs(move.y) + skinWidth;

            int result = 0;

            for (int i = 0; i < verticalRayCount; i++)// for each ray
            {
                // cast ray
                Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
                rayOrigin += Vector2.right * (verticalRaySpacing * i + move.x);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask, raycastOrigins.lowest, raycastOrigins.highest + 1);

                Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.red);

                // collision
                if (hit)
                {
                    move.y = (hit.distance - skinWidth) * directionY;
                    rayLength = hit.distance;

                    result = directionY;

                    collisions.up = directionY == 1;
                    collisions.down = directionY == -1;
                }
            }
            return result;
        }
        List<Collider2D> depthCollisionResults = new List<Collider2D>();
        bool GroundCollisions(ref Vector3 move, out Collider2D floor)
        {
            // get floors
            ContactFilter2D filter = PhysicsSettings.GetFilter(collisionMask, raycastOrigins.lowest - 2 * skinWidth + move.z, raycastOrigins.lowest + .2f);
            Physics2D.OverlapBox(raycastOrigins.bound.center, raycastOrigins.bound.size, 0, filter, depthCollisionResults);
            // find highest floor
            floor = null;
            foreach (Collider2D collider in depthCollisionResults)
            {
                if (!floor)
                {
                    floor = collider;
                    continue;
                }
                if (collider.transform.position.z > floor.transform.position.z)
                {
                    floor = collider;
                }
            }
            // check for collision
            if (floor && raycastOrigins.lowest - skinWidth + move.z < floor.transform.position.z + skinWidth)
            {
                //print(floor.name + " at " + collider.bounds.center);
                // cull move
                move.z = floor.transform.position.z + skinWidth - transform.position.z;
                return true;
            }
            return false;
        }
        bool CeilingCollisions(ref Vector3 move)
        {
            // get lowest ceiling (feature of overlapBox)
            Collider2D ceiling = Physics2D.OverlapBox(raycastOrigins.bound.center, raycastOrigins.bound.size - skinWidth * Vector3.one, 0, collisionMask, raycastOrigins.highest, raycastOrigins.highest + skinWidth + move.z + 1);
            if (ceiling && raycastOrigins.highest + skinWidth + move.z > ceiling.transform.position.z - 1)
            {
                move.z = ceiling.transform.position.z - 1 - raycastOrigins.highest - skinWidth;
                return true;
            }
            return false;
        }

        [System.Serializable]
        public struct CollisionInfo
        {
            public bool up, down, left, right, grounded, above, collidedThisFrame;
            [ReadOnly] public Collider2D floor;

            public void Reset()
            {
                up = down = left = right = grounded = above = collidedThisFrame = false;
                floor = null;
            }
        }
    }
}