using UnityEngine;
using System.Collections.Generic;

namespace ColbyDoan
{
    public static class PhysicsSettings
    {
        static readonly public float gravity = -10;

        static readonly public LayerMask kinematics = LayerMask.GetMask("Kinematic", "Animate");
        static readonly public LayerMask animates = LayerMask.GetMask("Animate");
        static readonly public LayerMask solids = LayerMask.GetMask("Solid");
        static readonly public LayerMask hitboxes = LayerMask.GetMask("Enemy", "Friendly", "Block Projectiles");

        static readonly public float depthToHeightMultiplier = 1;

        /// <summary> Linecasts for solids at the starting point's depth </summary>
        /// <remarks> Accounts for the origin of solids being .5 units higher then their center </remarks>
        static public RaycastHit2D SolidsLinecast(Vector3 start, Vector3 end)
        {
            return SolidsLinecast(start, end, start.z);
        }
        /// <summary> Linecasts for solids at a chosen depth </summary>
        /// <remarks> Accounts for the origin of solids being .5 units higher then their center </remarks>
        static public RaycastHit2D SolidsLinecast(Vector3 start, Vector3 end, float depth)
        {
            return Physics2D.Linecast(start, end, solids, depth, depth + 1);
        }
        /// <summary>
        /// Raycast for solids within a certain depth along a line,
        /// accounts for the origin of solids being .5 units higher then their center
        /// </summary>
        /// <param name="below">units to check below the z position (feet)</param>
        /// <param name="above">units to check above the z position + 1 (head)</param>
        static public RaycastHit2D SolidsRaycast(Vector3 start, Vector3 direction, float distance, float below = 0, float above = 0)
        {
            return Physics2D.Raycast(start, direction, distance, solids, start.z - below, start.z + above + 1);
        }

        static public bool CheckForSolids(Vector3 point, float radius)
        {
            return Physics2D.OverlapCircle(point, radius, solids, point.z, point.z + 1);
        }
        /// <summary>
        /// This is just Physics2D.BoxCast
        /// </summary>
        /// <see cref="Physics2D.BoxCast"/>
        /// <param name="collider"></param>
        /// <param name="direction"></param>
        /// <param name="filter"></param>
        /// <param name="results"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        static public int CustomCast(this BoxCollider2D collider, Vector2 direction, ContactFilter2D filter, List<RaycastHit2D> results, float distance)
        {
            // DONT USE collider.cast IT NEEDS A RIGIDBODY
            return Physics2D.BoxCast(collider.bounds.center, collider.size, 0, direction, filter, results, distance);
        }


        /// <summary> Returns a filter of a specified mask and depth </summary>
        static public ContactFilter2D GetFilter(LayerMask mask, Vector3 selfPos, float below = 0, float above = 0)
        {
            return GetFilter(mask, selfPos.z - below, selfPos.z + above);
        }
        static public ContactFilter2D GetFilter(LayerMask mask, float min = 0, float max = 2)
        {
            ContactFilter2D result = new ContactFilter2D
            {
                layerMask = mask,
                useLayerMask = true,
                useDepth = true,

                minDepth = min,
                maxDepth = max
            };

            return result;
        }

        #region Force Methods
        // ALL FORCE METHODS ARE INSTANTANEOUS AND DO NOT ACCOUNT FOR DELTATIME
        static public void ApplyImpulseOn(this IPhysicsObject target, IPhysicsObject other, Vector3 impulse)
        {
            other.ApplyImpulse(impulse);
            target.ApplyImpulse(-impulse);
        }
        static public void ApplyImpulse(this IPhysicsObject target, Vector3 impulse)
        {
            Vector3 deltaV = impulse / target.Mass;
            target.Velocity += deltaV;
            //print(name + " was pushed by " + deltaV + " to " + velocity);
        }

        static public void ForceTo(this IPhysicsObject target, Vector3 targetV, float maxImpulse = float.PositiveInfinity, ForceIsolation isolation = ForceIsolation.None)
        {
            target.AccelerateTo(targetV, maxImpulse / target.Mass, isolation);
        }
        static public void AccelerateTo(this IPhysicsObject target, Vector3 targetV, float maxBoost = float.PositiveInfinity, ForceIsolation isolation = ForceIsolation.None)
        {
            Vector3 velocity = target.Velocity;
            switch (isolation)
            {
                case ForceIsolation.None:
                    target.Velocity = Vector3.MoveTowards(velocity, targetV, maxBoost);
                    break;
                case ForceIsolation.Vertical:
                    velocity.z = Mathf.MoveTowards(velocity.z, targetV.z, maxBoost);
                    target.Velocity = velocity;
                    break;
                case ForceIsolation.Horizontal:
                    target.Velocity = Vector3.MoveTowards((Vector2)velocity, (Vector2)targetV, maxBoost) + velocity.z * Vector3.forward;
                    break;
            }
        }

        public enum ForceIsolation { None, Vertical, Horizontal };

        static public void EZForce(this IPhysicsObject target, ForceInfo info)
        {
            // Debug.Log(info);
            if (info.impulseMode)
                target.ApplyImpulse(info.v.normalized * info.i);
            else
                target.ForceTo(info.v, info.i);
        }
        #endregion
    }

    /// <summary> interface for stuff that forces can be applied on </summary>
    public interface IPhysicsObject
    {
        Vector3 Velocity { get; set; }
        float Mass { get; }
        float GravityMultiplier { get; set; }
        bool Grounded { get; }
    }

    [System.Serializable]
    public struct ForceInfo
    {
        /// <summary> Impulse vector in impulse mode, target velocity otherwise </summary>
        public Vector3 v;
        /// <summary> Impulse magnitude </summary>
        public float i;
        [Tooltip("When true the affected object receives a fixed impulse with direction V and magnitude I")]
        public bool impulseMode;
        /// <summary> For a force with a target velocity </summary>
        /// <param name="targetV">Velocity the affected object will try to match</param>
        /// <param name="_impulse">Max rate at which its momentum can change when attempting to match targetV</param>
        public ForceInfo(Vector3 targetV, float _impulse)
        {
            v = targetV;
            i = _impulse;
            impulseMode = false;
        }
        /// <summary> Raw impulse force </summary>
        /// <param name="_impulse">It is impulse</param>
        public ForceInfo(Vector3 _impulse)
        {
            v = _impulse;
            i = _impulse.magnitude;
            impulseMode = true;
        }

        public override string ToString()
        {
            if (impulseMode)
            {
                return $"Impulse: {v.normalized * i}";
            }
            else
            {
                return $"TargetV: {v}, Max Impulse: {i}";
            }
        }
    }
}