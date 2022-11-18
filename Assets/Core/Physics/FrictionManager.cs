using UnityEngine;

namespace ColbyDoan.Physics
{
    [RequireComponent(typeof(IPhysicsObject))]
    public class FrictionManager : MonoBehaviour
    {
        IPhysicsObject target;
        public float groundFriction = 10;
        public float slidingFriction = 5;
        public float airFriction = 2;

        public Vector2 groundSpeedOffset;

        public bool sliding;
        public float EffectiveGroundFriction { get { return target.Grounded ? (sliding ? slidingFriction * target.Velocity.magnitude : groundFriction) : 0; } }

        private void Awake()
        {
            target = GetComponent<IPhysicsObject>();
        }
        protected virtual void FixedUpdate()
        {
            ApplyAirFriction();
            ApplyGroundFriction();
        }
        protected virtual void ApplyAirFriction()
        {
            Vector3 windSpeed = WindManager.GetWindAtPoint(transform.position);
            target.ForceTo(windSpeed, airFriction * (windSpeed - target.Velocity).sqrMagnitude * Time.fixedDeltaTime);
        }
        protected virtual void ApplyGroundFriction()
        {
            //print(groundSpeedOffset);
            Vector3 groundSpeed = Vector2.zero + groundSpeedOffset;
            target.AccelerateTo(groundSpeed + Vector3.forward * target.Velocity.z, EffectiveGroundFriction * Time.fixedDeltaTime);
        }
    }
}