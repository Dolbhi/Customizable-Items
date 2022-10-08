using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

/*
Collection of classes that manage enemy behaviour,
all extends from the enemybehaviour class
*/

namespace ColbyDoan
{
    public abstract partial class EnemyBehaviour
    {
        /// <summary>
        /// Enemy state with no movement method.
        /// Comes with GetCharacter, CurrentTarget and Decider
        /// </summary>
        [System.Serializable]
        protected abstract class EnemyState<T> : State<T> where T : EnemyBehaviour
        {
            protected Character GetCharacter => container.character;
            protected TargetInfo CurrentTarget => container.tracker.currentTarget;
            protected MoveDecider Decider => container.moveDecider;

            public override void Enter()
            {
                base.Enter();
                container.MovingDirection = Vector2.zero;
            }
        }

        [System.Serializable]
        protected class IdleState : EnemyState<EnemyBehaviour>
        {
            public float idleSpeedCoefficient = .5f;

            public float idleMinWait = 2;
            public float idleMaxWait = 4;
            public float idleMaxDist = 5;

            public bool requiresGround = true;

            bool following;

            Coroutine waitToMove;

            Vector2 wanderPos;

            public override void Enter()
            {
                base.Enter();
                container.enableStateUpdateLoop = false;
                container._speedMultiplier = 0;
                container.SetFacing = container.FaceMovement;
                Decider.DirectionEvaluator = null;
                waitToMove = container.StartCoroutine(WaitToMove());
            }

            public override void Update()
            {
                base.Update();
                if (following)
                {
                    if ((wanderPos - container.transform.Get2DPos()).magnitude < .1f || container.character.kinematicObject.controller.collisions.collidedThisFrame)
                        waitToMove = container.StartCoroutine(WaitToMove());
                }
            }
            IEnumerator WaitToMove()
            {
                // stop move
                following = false;
                container._speedMultiplier = 0;

                // wait
                yield return new WaitForSeconds(UnityEngine.Random.Range(idleMinWait, idleMaxWait));

                // keep searching for a wanderPos until one without obstacles is found
                Vector3 pos = container.transform.position;
                Vector2 direction;
                int attempts = 0;
                do
                {
                    wanderPos = UnityEngine.Random.insideUnitCircle.normalized * idleMaxDist + container.transform.Get2DPos();
                    direction = wanderPos - (Vector2)pos;
                    attempts++;
                    if (attempts > 20)
                    {
                        wanderPos = pos;
                        break;
                    }
                }
                while (PhysicsSettings.SolidsLinecast(pos, wanderPos) || TileManager.Instance.PitLinecast(pos, wanderPos) < direction.magnitude);

                // start move setting facing, move direction and speed
                following = true;
                container.MovingDirection = direction;
                container._speedMultiplier = idleSpeedCoefficient;
            }

            public override void Exit()
            {
                base.Exit();
                container.StopCoroutine(waitToMove);
                if (container.indicator.isActiveAndEnabled)
                    container.indicator.SetIndicator("!");
                container._speedMultiplier = 1;
            }
        }
        [System.Serializable]
        protected class PursuitTargetState<T> : EnemyState<T> where T : EnemyBehaviour
        {
            public bool forbidDrops = false;
            public float forgetTime = 10;

            public override void Enter()
            {
                base.Enter();
                container.SetFacing = container.FaceTarget;
            }

            public override void Update()
            {
                base.Update();
                // allow dropping or not
                container.moveDecider.allowDrops = !forbidDrops && CurrentTarget.KnownDisplacement.z < -.3f;
                // lose target if it cannot be found at the last seen position or 20s has passed since last seen
                if (!CurrentTarget.HasLineOfSight && (CurrentTarget.timeLastSeen + forgetTime < Time.time || CurrentTarget.KnownDisplacement.sqrMagnitude < .1f))
                {
                    container.tracker.ForgetTarget();
                }
            }

            public override void Exit()
            {
                base.Exit();
                container.moveDecider.allowDrops = false;
            }
        }
        [System.Serializable] protected class PursuitTargetState : PursuitTargetState<EnemyBehaviour> { }

        /// <summary>
        /// Broken DO NOT USE
        /// </summary>
        [System.Serializable]
        protected class PureMoveDeciderState<T> : EnemyState<T> where T : EnemyBehaviour
        {
            /// <summary> Delay between calling MovementUpdate() </summary>
            [SerializeField] protected float moveUpdateDelay = .2f;

            /// <summary> Score at which character stops approaching target </summary>
            [SerializeField] protected float moveStopThreshold = 0;

            /// <summary> Minimum score at which character moves at max initial speed </summary>
            [SerializeField] protected float moveMaxThreshold = .3f;

            /// <summary> yes </summary>
            [SerializeField] protected bool forbidDropping = false;

            /// <summary> Returns the desirablity to move a given direction </summary>
            public virtual float EvaluateDirection(Vector2 inputDirection)
            {
                return MoveDecider.TargetDirectionScorer(inputDirection, CurrentTarget.KnownDisplacement);
            }

            Coroutine movementUpdateLoop;
            public override void Enter()
            {
                base.Enter();
                // container.directionEvaluator = EvaluateDirection;
                movementUpdateLoop = container.StartCoroutine(MovementUpdateLoop());
            }
            IEnumerator MovementUpdateLoop()
            {
                while (true)
                {
                    MovementUpdate();
                    yield return new WaitForSeconds(moveUpdateDelay);
                }
            }
            /// <summary> Feeds GoalScorer to movedecider and tells character to move in that direction </summary>
            protected virtual void MovementUpdate()
            {
                //     // allow drops if enemy is on lower level
                //     container.moveDecider.allowDrops = !forbidDropping && CurrentTarget.KnownDisplacement.z < .3f;
                //     // find best move considering goal and obstacles
                //     MoveOption bestOption = Decider.GetBestMoveOption();
                //     // sets that direction to move to
                //     container.MovingDirection = bestOption.direction;
                //     // slow down based on direction score
                //     container.speedMultiplier = initialSpeedMultiplier * Mathf.InverseLerp(moveStopThreshold, moveMaxThreshold, bestOption.FinalScore);
            }

            public override void Exit()
            {
                base.Exit();
                if (movementUpdateLoop != null) container.StopCoroutine(movementUpdateLoop);
                container.moveDecider.allowDrops = false;
            }
        }

        // Stuff to do on Enter
        // {
        //     base.Enter();
        //     container.MovingDirection = Vector2.zero;
        //     container.SetFacing = InitialFacing;
        //     container._speedMultiplier = InitialSpeedMultiplier;
        //     Decider.DirectionEvaluator = InitialDirectionEvaluator;
        // }
    }

    #region Direction Evaluators
    /// <summary>
    /// Bare bones (for now) implementation of IDirectionEvaluator
    /// </summary>
    public abstract class EnemyDirectionEvaluator : IDirectionEvaluator
    {
        public virtual void PreEvaluation() { }
        public abstract float EvaluateDirection(Vector2 inputDirection);
    }
    /// <summary>
    /// Weighted sum of many evaluators
    /// </summary>
    [System.Serializable]
    public class CombinedEvaluator : EnemyDirectionEvaluator
    {
        public IDirectionEvaluator[] evaluators;
        public float[] coefficents;

        public override void PreEvaluation()
        {
            base.PreEvaluation();

            if (evaluators.Length > coefficents.Length)
            {
                Debug.LogWarning("Number of evaluators does not match number of coefficents");
                return;
            }

            foreach (IDirectionEvaluator evaluator in evaluators) evaluator.PreEvaluation();
        }
        public override float EvaluateDirection(Vector2 inputDirection)
        {
            int length = evaluators.Length;
            float output = 0;

            for (int i = 0; i < length; i++)
            {
                output += coefficents[i] * evaluators[i].EvaluateDirection(inputDirection);
            }
            return output;
        }
    }
    /// <summary>
    /// Evaluator with a targetDirection that can be set. ALL TargetEvaluator needs displacementProvider to be set
    /// </summary>
    public abstract class TargetEvaluator : EnemyDirectionEvaluator
    {
        // public event Action PresetTargetDirection = delegate { };
        public IDisplacementProvider displacementProvider;
        public Vector3 targetDisplacement;
        public override void PreEvaluation()
        {
            base.PreEvaluation();
            if (displacementProvider == null) return;
            targetDisplacement = displacementProvider.Displacement;
            // PresetTargetDirection.Invoke();
        }
    }
    // protected void BindEvaluatorToTracker(TargetEvaluator evaluator)
    // {
    //     evaluator.displacementProvider = tracker;
    // }
    // protected void BindEvaluatorToTracker(params TargetEvaluator[] evaluators)
    // {
    //     foreach (TargetEvaluator evaluator in evaluators) BindEvaluatorToTracker(evaluator);
    // }

    public class BasicEvaluator : TargetEvaluator
    {
        public override float EvaluateDirection(Vector2 inputDirection) => MoveDecider.TargetDirectionScorer(inputDirection, targetDisplacement);
    }
    [System.Serializable]
    public class SpreadOutEvaluator : EnemyDirectionEvaluator
    {
        public List<Transform> spreadWith;
        public Transform selfTransform;

        public float scoreLerpMin = .4f;
        public float scoreLerpMax = 1;

        public float ignoreRange = 7;

        Vector2 _imaginaryRepulsion;
        float _scoreMultiplier;
        public override void PreEvaluation()
        {
            base.PreEvaluation();
            // maximise distance from other active shooters
            _imaginaryRepulsion = Vector2.zero;
            foreach (Transform transform in spreadWith)
            {
                // skip if self
                if (transform == selfTransform) continue;

                Vector2 displacement = selfTransform.position - transform.position;
                // if (displacement.sqrMagnitude > ignoreRange * ignoreRange) continue;

                _imaginaryRepulsion += displacement / displacement.sqrMagnitude;
            }
            _scoreMultiplier = Mathf.Clamp01(Mathf.InverseLerp(scoreLerpMin, scoreLerpMax, _imaginaryRepulsion.magnitude));
            if (selfTransform.name == "logger") Debug.Log($"Repulsion: {_imaginaryRepulsion.magnitude}");
        }

        public override float EvaluateDirection(Vector2 inputDirection)
        {
            return _scoreMultiplier * MoveDecider.TargetDirectionScorer(inputDirection, _imaginaryRepulsion);
        }
    }
    [System.Serializable]
    public class TargetMinDistEvaluator : TargetEvaluator
    {
        public float minDistance;

        // bool _withinMinDist;
        float _closenessMultiplier;
        Vector2 _targetNormalized;

        public override void PreEvaluation()
        {
            base.PreEvaluation();
            // _withinMinDist = TargetDisplacement.sqrMagnitude < minDistance * minDistance;
            _closenessMultiplier = Mathf.Clamp01(minDistance - targetDisplacement.magnitude + 1);
            _targetNormalized = targetDisplacement.normalized;
        }

        // attempt to stay at lungeDist from target
        public override float EvaluateDirection(Vector2 inputDirection)
        {
            return _closenessMultiplier * -Vector2.Dot(inputDirection, _targetNormalized);
            // return _withinMinDist ? Vector2.Dot(inputDirection, -_targetNormalized) : 0;
        }
    }
    [System.Serializable]
    public class TargetMaxDistEvaluator : TargetEvaluator
    {
        public float maxDistance;

        // bool _outsideMaxDist;
        float _farnessMultiplier;
        Vector2 _targetNormalized;

        public override void PreEvaluation()
        {
            base.PreEvaluation();
            // _outsideMaxDist = TargetDisplacement.sqrMagnitude > maxDistance * maxDistance;
            _farnessMultiplier = Mathf.Clamp01(targetDisplacement.magnitude - maxDistance + 1);
            _targetNormalized = targetDisplacement.normalized;
        }

        // attempt to stay at lungeDist from target
        public override float EvaluateDirection(Vector2 inputDirection)
        {
            return _farnessMultiplier * Vector2.Dot(inputDirection, _targetNormalized);
            // return _outsideMaxDist ? Vector2.Dot(inputDirection, _targetNormalized) : 0;
        }
    }
    [System.Serializable]
    public class MaintainDistanceEvaluator : TargetEvaluator
    {
        public float distanceToMaintain;

        protected float _fractionFromRadius;
        public override void PreEvaluation()
        {
            base.PreEvaluation();
            /// normalisedDist = 1 => lunger is double the lunge dist away, Dist = -1 => lunger is at target
            _fractionFromRadius = Mathf.Min((targetDisplacement.magnitude / distanceToMaintain) - 1, 1);
        }

        // attempt to stay at lungeDist from target
        public override float EvaluateDirection(Vector2 inputDirection)
        {
            return Mathf.Abs(_fractionFromRadius) * MoveDecider.TargetDirectionScorer(inputDirection, _fractionFromRadius * targetDisplacement);
        }
    }
    [System.Serializable]
    public class CirclingEvaluator : MaintainDistanceEvaluator
    {
        public KinematicObject kinematicObject;

        Vector2 _displacementNormalised;
        Vector2 _velocityDir;
        public override void PreEvaluation()
        {
            base.PreEvaluation();
            _displacementNormalised = targetDisplacement.normalized;
            _velocityDir = kinematicObject.velocity.normalized;
        }

        public override float EvaluateDirection(Vector2 inputDirection)
        {
            // circle target
            float circleScore = (1 - Mathf.Abs(_fractionFromRadius)) * Mathf.Abs(Vector2.Dot(Vector2.Perpendicular(inputDirection), _displacementNormalised));
            float velocityBias = 0.2f * Vector2.Dot(inputDirection, _velocityDir);
            float followScore = base.EvaluateDirection(inputDirection);

            return circleScore + velocityBias + followScore;
        }
    }
    #endregion

}