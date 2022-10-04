using UnityEngine;

namespace ColbyDoan
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
        protected virtual void Update()
        {
            ApplyAirFriction();
            ApplyGroundFriction();
        }
        protected virtual void ApplyAirFriction()
        {
            Vector3 windSpeed = Vector3.zero;//GameManager.instance.windManager.GetWindAtPoint(transform.position);
            target.ForceTo(windSpeed, airFriction * (windSpeed - target.Velocity).magnitude * Time.deltaTime);
        }
        protected virtual void ApplyGroundFriction()
        {
            //print(groundSpeedOffset);
            Vector3 groundSpeed = Vector2.zero + groundSpeedOffset;
            target.AccelerateTo(groundSpeed + Vector3.forward * target.Velocity.z, EffectiveGroundFriction * Time.deltaTime);
        }
    }
}