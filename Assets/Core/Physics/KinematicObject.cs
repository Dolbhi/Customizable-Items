using System.Collections.Generic;
using UnityEngine;
using System;

using ColbyDoan.FixedTimeLerp;

namespace ColbyDoan.Physics
{
    /// <summary>
    /// Physics manager of inanimate but movable objs i.e homemade rigidbody
    /// </summary>
    [RequireComponent(typeof(Controller2D))]
    [RequireComponent(typeof(InterpolatedTransform))]
    [SelectionBase]
    public class KinematicObject : FindableByRoot<KinematicObject>, IPhysicsObject
    {
        [Header("Physical Properties")]
        public float mass = 1;
        public Vector3 velocity;
        [Range(0, 1)]
        public float elasticity = 1;
        public float upLift = 0;

        [HideInInspector] public Carrier objCarrier;
        [HideInInspector] public Controller2D controller;
        [HideInInspector] public InterpolatedTransform updateLerper;

        float EffectiveGravity => PhysicsSettings.gravity * (1 - upLift);

        [HideInInspector] public Carrier carriedBy;

        // physicalObject interface
        public Vector3 Velocity { get => velocity; set { velocity = value; } }
        public float Mass => mass;
        public bool Grounded => controller.collisions.grounded && !carriedBy;
        public float GravityMultiplier { get => (1 - upLift); set { upLift = 1 - value; } }

        void Awake()
        {
            objCarrier = GetComponent<Carrier>();
            objCarrier.self = this;

            controller = GetComponent<Controller2D>();
            updateLerper = GetComponent<InterpolatedTransform>();

            // handles all that depends on this class
            DependancyInjector.InjectDependancies(this);
        }

        void FixedUpdate()
        {
            // gravity
            velocity.z += EffectiveGravity * Time.fixedDeltaTime;

            // move with velcity
            ManageCollisions(controller.Move(velocity * Time.fixedDeltaTime));

            ManagePitting();
        }

        public void Teleport(Vector3 postion)
        {
            transform.position = postion;
            updateLerper.ForgetPreviousTransforms();
            objCarrier.TeleportCarriedIntoPos();
        }

        // changes velocity based on collisions
        void ManageCollisions(Controller2D.CollisionInfo collisions)
        {
            if (collisions.up)
                velocity.y = Mathf.Min(velocity.y, 0);
            else if (collisions.down)
                velocity.y = Mathf.Max(velocity.y, 0);
            if (collisions.right)
                velocity.x = Mathf.Min(velocity.x, 0);
            else if (collisions.left)
                velocity.x = Mathf.Max(velocity.x, 0);

            if (collisions.grounded)
                velocity.z = Mathf.Max(velocity.z, 0);
            else if (collisions.above)
                velocity.z = Mathf.Min(velocity.z, 0);
        }

        // move pitting to seperate mono
        public bool destroyWhenPitted = true;
        public event Action OnPitted;
        void ManagePitting()
        {
            if (transform.position.z < -1)
            {
                if (destroyWhenPitted)
                    Destroy(gameObject);
                OnPitted?.Invoke();
            }
        }

        protected override void OnDisable()
        {
            carriedBy?.Drop(this);
            objCarrier.DropAll();
            base.OnDisable();
        }
    }
}