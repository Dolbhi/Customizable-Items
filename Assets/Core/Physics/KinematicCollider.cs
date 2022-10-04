// using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace ColbyDoan
{
    /// <summary>
    /// Class controlling the behaviour of KO-KO collisions. 
    /// Colliders are modelled as 1 unit tall cylinders
    /// </summary>
    [System.Serializable]
    public class KinematicCollider : MonoBehaviour, IAutoDependancy<KinematicObject>
    {
        [SerializeField] KinematicObject kinematicObj = null;
        new BoxCollider2D collider;
        public KinematicObject Dependancy
        {
            set
            {
                kinematicObj = value;
                collider = kinematicObj.controller.collider;
            }
        }
        [Header("Stats")]
        /// <summary> Min damage speed, also reduces final damage </summary>
        public float speedTreshold = 10;

        bool wasAProjectile;

        HashSet<Collider2D> pastCollisions = new HashSet<Collider2D>();
        List<RaycastHit2D> projectileCollisions = new List<RaycastHit2D>();

        public event Action<Vector2, Transform> onCollide = delegate { };

        void Update()
        {
            if (kinematicObj.velocity.sqrMagnitude > speedTreshold * speedTreshold)
            {
                HashSet<Collider2D> newCollisions = new HashSet<Collider2D>();
                Vector2 move = kinematicObj.velocity * Time.deltaTime;

                // find colliders in path of motion
                collider.CustomCast(move, PhysicsSettings.GetFilter(PhysicsSettings.kinematics, kinematicObj.transform.position, 1, 1), projectileCollisions, move.magnitude);// DONT USE collider.cast IT NEEDS A RIGIDBODY
                //Debug.Log(self.name + " center: " + collider.bounds.center + " size: " + collider.size + " move: " + move);

                // loop over each collision
                foreach (RaycastHit2D hit in projectileCollisions)
                {
                    // check if new
                    newCollisions.Add(hit.collider);
                    if (pastCollisions.Contains(hit.collider) || !wasAProjectile)
                        continue;

                    //Debug.Log(kinematicObj.name + " hit " + hit.transform.name);
                    _ProcessCollision(hit);
                }
                wasAProjectile = true;
                pastCollisions = newCollisions;
            }
            else
            {
                wasAProjectile = false;
                // clear pastCollisions once below treshold
                pastCollisions.Clear();
            }
        }

        void _ProcessCollision(RaycastHit2D hit)
        {
            // Get direction
            Vector2 direction = hit.transform.position - transform.position;
            direction.Normalize();

            // check if its kinematic
            KinematicObject otherObj;
            if (KinematicObject.instanceFromTransform.TryGetValue(hit.transform.root, out otherObj))
            {
                //if (carriedBy && otherObj.gameObject == carriedBy.gameObject) return; // Check if not carrier

                // apply knockback and recoil
                Vector2 relativeV = kinematicObj.velocity - otherObj.velocity;
                relativeV = Mathf.Max(Vector2.Dot(direction, relativeV), 0) * direction;
                Vector2 impulse = (1 + Mathf.Min(otherObj.elasticity, kinematicObj.elasticity)) * relativeV * otherObj.mass * kinematicObj.mass / (otherObj.mass + kinematicObj.mass);
                kinematicObj.ApplyImpulseOn(otherObj, impulse);

                onCollide.Invoke(impulse, hit.transform);
            }
            else
            {
                Debug.LogWarning("I have somehow hit a collider on the kinematic layer which has no KO component");
            }
        }
    }
}