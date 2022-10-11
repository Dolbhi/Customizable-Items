using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ColbyDoan
{
    public class Drone : EnemyBehaviour
    {
        [Header("Dependancies")]
        [SerializeField] FrictionManager frictionManager = null;
        [SerializeField] LineRenderer laserPointer = null;
        [SerializeField] AudioSource fireNoise = null;

        [SerializeField] IdleState idleState = new IdleState();
        [SerializeField] DronePursuitState pursuitState = new DronePursuitState();
        [SerializeField] DroneAttackState attackState = new DroneAttackState();

        BasicEvaluator pursuitMovement = new BasicEvaluator();

        [SerializeField] CombinedEvaluator aimingMovement = new CombinedEvaluator();
        [SerializeField] TargetMinDistEvaluator avoidTargetMovement = new TargetMinDistEvaluator();
        [SerializeField] TargetMaxDistEvaluator stayInRangeMovement = new TargetMaxDistEvaluator();
        [SerializeField] SpreadOutEvaluator spreadOutMovement = new SpreadOutEvaluator();

        ProjectileSkill projectileSkill;

        [SerializeField] float aimingTime = .5f;
        float _aimStartTime;
        void ResetAimTimer()
        {
            _aimStartTime = Time.time;
        }

        protected override void Awake()
        {
            base.Awake();

            projectileSkill = Skills[0] as ProjectileSkill;

            pursuitMovement.displacementProvider = tracker;
            avoidTargetMovement.displacementProvider = tracker;
            stayInRangeMovement.displacementProvider = tracker;

            spreadOutMovement.selfTransform = transform.root;
            spreadOutMovement.spreadWith = toSpreadWith;

            avoidTargetMovement.minDistance = projectileSkill.fireRange / 3;
            stayInRangeMovement.maxDistance = projectileSkill.fireRange * .8f;

            aimingMovement.evaluators = new IDirectionEvaluator[] { avoidTargetMovement, stayInRangeMovement, spreadOutMovement };
        }
        protected override IState InitializeStateMachine(StateMachine stateMachine)
        {
            idleState.Init(this, stateMachine);
            pursuitState.Init(this, stateMachine);
            attackState.Init(this, stateMachine);

            return idleState;
        }

        bool firing;
        protected override void Update()
        {
            base.Update();
            if (firing)
            {
                laserPointer.SetPositions(new Vector3[] { projectileSkill.fireOrigin.position.GetDepthApparentPosition(), tracker.currentTarget.KnownPos.GetDepthApparentPosition() });
            }
        }

        public override void UpdateState()
        {
            OldTargetInfo target = tracker.currentTarget;
            float targetDistSqr = target.KnownDisplacement.sqrMagnitude;
            if (!target.exists)
            {
                // idle
                indicator.SetIndicator("?");
                SafeStateChange(idleState);
            }
            else
            {
                // aggro
                SetFacing = FaceTarget;
                if (!tracker.currentTarget.HasLineOfSight)
                {
                    SafeStateChange(pursuitState);
                }
                else
                {
                    SafeStateChange(attackState);
                }
            }
        }

        void TryFire()
        {
            if (_aimStartTime + aimingTime < Time.time && projectileSkill.Ready && tracker.currentTarget.KnownDisplacement.sqrMagnitude < projectileSkill.FireRangeSqr)
            {
                projectileSkill.TargetPos = tracker.currentTarget.KnownPos;
                projectileSkill.Activate();
            }
        }

        void MatchTargetHeight()
        {
            float distanceFromTargetHeight = tracker.currentTarget.KnownDisplacement.z + .8f;
            float direction = Mathf.Sign(distanceFromTargetHeight);
            float damping = Mathf.Clamp01(Mathf.Abs(distanceFromTargetHeight / .4f));// .4 is the max distance when damping begins
            Vector3 finalV = character.kinematicObject.Velocity;
            finalV.z = 2 * damping * direction;
            character.kinematicObject.ForceTo(finalV);
        }

        public void ShootingAnimation(UnityAction animationCompleteCallback)
        {
            StartCoroutine("ShootingAnimationCoroutine", animationCompleteCallback);
        }
        IEnumerator ShootingAnimationCoroutine(UnityAction animationCompleteCallback)
        {
            //indicator.text = ".";
            //yield return new WaitForSeconds(.2f);
            //indicator.text = "..";
            //yield return new WaitForSeconds(.2f);
            //indicator.text = "...";
            //yield return new WaitForSeconds(.2f);
            //indicator.text = "...!";

            firing = true;
            laserPointer.enabled = true;
            yield return new WaitForSeconds(1);
            laserPointer.enabled = false;
            yield return new WaitForSeconds(.05f);
            laserPointer.enabled = true;
            yield return new WaitForSeconds(.05f);
            laserPointer.enabled = false;
            yield return new WaitForSeconds(.05f);
            laserPointer.enabled = true;
            yield return new WaitForSeconds(.05f);
            laserPointer.enabled = false;
            yield return new WaitForSeconds(.05f);
            laserPointer.enabled = true;
            yield return new WaitForSeconds(.05f);
            laserPointer.enabled = false;
            yield return new WaitForSeconds(.05f);
            laserPointer.enabled = true;
            yield return new WaitForSeconds(.05f);
            laserPointer.enabled = false;

            fireNoise.Play();

            firing = false;
            projectileSkill.TargetPos = tracker.currentTarget.KnownPos;
            animationCompleteCallback.Invoke();
            //indicator.text = "";
        }

        static List<Transform> toSpreadWith = new List<Transform>();
        protected void OnEnable()
        {
            toSpreadWith.Add(transform.root);
        }
        protected override void OnDisable()
        {
            base.OnDisable();
            character.kinematicObject.upLift = 0;
            frictionManager.airFriction = 5;
            toSpreadWith.Remove(transform.root);
        }

        [System.Serializable]
        class DronePursuitState : EnemyState<Drone>
        {
            public float forgetTime = 10;

            public override void Enter()
            {
                base.Enter();
                Decider.DirectionEvaluator = container.pursuitMovement;
                Decider.EvaluateImmediately();
            }

            public override void Update()
            {
                base.Update();

                container.MatchTargetHeight();

                // lose target if it cannot be found at the last seen position or 20s has passed since last seen
                if (!CurrentTarget.HasLineOfSight && (CurrentTarget.timeLastSeen + forgetTime < Time.time || CurrentTarget.KnownDisplacement.sqrMagnitude < .1f))
                {
                    container.tracker.ForgetTarget();
                }

                if (CurrentTarget.HasLineOfSight)
                {
                    sm.ChangeState(container.attackState);
                }
            }
        }
        [System.Serializable]
        class DroneAttackState : EnemyState<Drone>
        {
            public float forgetTime = 10;

            public override void Enter()
            {
                base.Enter();
                container.ResetAimTimer();
                Decider.DirectionEvaluator = container.aimingMovement;
                Decider.EvaluateImmediately();
            }

            public override void Update()
            {
                base.Update();

                container.MatchTargetHeight();

                container.TryFire();

                // lose target if it cannot be found at the last seen position or 20s has passed since last seen
                if (!CurrentTarget.HasLineOfSight && (CurrentTarget.timeLastSeen + forgetTime < Time.time || CurrentTarget.KnownDisplacement.sqrMagnitude < .1f))
                {
                    container.tracker.ForgetTarget();
                }

                if (!CurrentTarget.HasLineOfSight)
                {
                    sm.ChangeState(container.pursuitState);
                }
            }
        }
    }
}