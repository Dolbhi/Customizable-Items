// using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ColbyDoan.BehaviourTree;

namespace ColbyDoan
{
    [System.Serializable]
    public abstract class MovementTask : EnemyNode, IDirectionEvaluator
    {
        public float speedMultiplier = 1;
        public float updatePeriod = .1f;
        // [SerializeField] protected float moveStopThreshold = 0f;
        // [SerializeField] protected float moveMaxThreshold = 1;

        float _lastMoveTime = 0;

        public MovementTask() : base() { }
        public MovementTask(params Node[] nodes) : base(nodes) { }

        Transform _transform;
        MovementManager _moveMan;
        public override void Initalize(ColbyDoan.BehaviourTree.Tree tree)
        {
            base.Initalize(tree);
            _transform = enemyTree.character.transform;
            _moveMan = enemyTree.moveManager;
        }

        public virtual void PreEvaluation() { }
        public abstract float EvaluateDirection(Vector2 inputDirection);

        /// <summary>
        /// Do preEvaluation then update movemanage with evaluation results from move decider
        /// </summary>
        /// <returns></returns>
        public override NodeState Evaluate()
        {
            // skip if evaluation was done recently
            if (Time.time - _lastMoveTime < updatePeriod)
                return NodeState.running;
            _lastMoveTime = Time.time;

            // do preEvaluation
            PreEvaluation();

            // evaluate best move
            var moveDecision = enemyTree.decider.GetBestMoveOption(EvaluateDirection);

            // Debug.Log($"{_transform.root.name} is moving at {moveDecision.Item1}");

            // set move and speed
            _moveMan.UpdateMovement(moveDecision.Item1, moveDecision.Item2);
            _moveMan.speedMultiplier = speedMultiplier;// * Mathf.InverseLerp(moveStopThreshold, moveMaxThreshold, moveDecision.Item2); ;
            // _moveMan.MovingDirection = moveDecision.Item1;

            // set facing
            enemyTree.character.FacingDirection = moveDecision.Item1;

            return NodeState.running;
        }
    }

    // [System.Serializable]
    // public class ChangeObsticle

    /// <summary>
    /// Weighted sum of many evaluators
    /// </summary>
    public class CombinedMovementTask : MovementTask
    {
        MovementTask[] _evaluators;

        public CombinedMovementTask(params MovementTask[] moveEvaluators) : base(moveEvaluators)
        {
            _evaluators = moveEvaluators;

            // moveStopThreshold = moveStop;
            // moveMaxThreshold = moveMax;
        }

        public override void PreEvaluation()
        {
            base.PreEvaluation();

            // if (_evaluators.Length > _coefficents.Length)
            // {
            //     Debug.LogWarning("Number of evaluators does not match number of coefficents");
            //     return;
            // }

            foreach (IDirectionEvaluator evaluator in _evaluators) evaluator.PreEvaluation();
        }
        public override float EvaluateDirection(Vector2 inputDirection)
        {
            PreEvaluation();

            int length = _evaluators.Length;
            float output = 0;

            for (int i = 0; i < length; i++)
            {
                var evaluator = _evaluators[i];
                output += evaluator.speedMultiplier * evaluator.EvaluateDirection(inputDirection);
                // Debug.Log($"dir:{inputDirection} eval:{i} out:{_evaluators[i].EvaluateDirection(inputDirection)}");
            }
            // Debug.Log($"dir:{inputDirection} out:{output}");
            return output;
        }
    }

    [System.Serializable]
    public class SpreadOutTask : MovementTask
    {
        public TrackerTag spreadWithTrackerKey; //"spread_with_drone"
        public float scoreLerpMin = .4f;
        public float scoreLerpMax = 1;

        // public float ignoreRange = 7;

        HashSet<Transform> _spreadWith;
        Transform _root;

        public override void Initalize(ColbyDoan.BehaviourTree.Tree tree)
        {
            base.Initalize(tree);
            _root = enemyTree.character.transform.root;
            _spreadWith = RootTracker.GetSet(spreadWithTrackerKey);
        }

        Vector2 _imaginaryRepulsion;
        float _scoreMultiplier;
        public override void PreEvaluation()
        {
            base.PreEvaluation();
            // maximise distance from other active shooters
            _imaginaryRepulsion = Vector2.zero;
            foreach (Transform transform in _spreadWith)
            {
                // skip if self
                if (transform == _root) continue;

                Vector2 displacement = _root.position - transform.position;
                // if (displacement.sqrMagnitude > ignoreRange * ignoreRange) continue;

                _imaginaryRepulsion += displacement / displacement.sqrMagnitude;
            }
            _scoreMultiplier = Mathf.Clamp01(Mathf.InverseLerp(scoreLerpMin, scoreLerpMax, _imaginaryRepulsion.magnitude));
            // if (_root.name == "logger") Debug.Log($"Repulsion: {_imaginaryRepulsion.magnitude}");
        }

        public override float EvaluateDirection(Vector2 inputDirection)
        {
            // Debug.Log($"multi:{_scoreMultiplier} repul:{_imaginaryRepulsion}");
            return _scoreMultiplier * MoveDecider.TargetDirectionScorer(inputDirection, _imaginaryRepulsion);
        }

        // add success condition when far enough away from anyone
        public override NodeState Evaluate()
        {
            base.Evaluate();
            if (_scoreMultiplier == 0)
                return NodeState.success;
            return NodeState.running;
        }
    }
}
