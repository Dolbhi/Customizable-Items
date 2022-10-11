// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;

using ColbyDoan.BehaviourTree;

namespace ColbyDoan
{
    /// <summary>
    /// Movement task with a targetDirection that can be set. ALL TargetMovementTask needs a vector3 value at displacementKey to be set
    /// </summary>
    [System.Serializable]
    public abstract class TargetMovementTask : MovementTask
    {
        /// <summary>
        /// Defaults to using tracked target from FindTargetTask
        /// </summary>
        public string displacementKey = FindTargetTask.targetDisplaceKey;

        protected Vector3 targetDisplacement;
        /// <summary>
        /// Pre computing done before all directions are scored
        /// </summary>
        public override void PreEvaluation()
        {
            targetDisplacement = (Vector3)GetData(displacementKey);
            // PresetTargetDirection.Invoke();
        }
    }

    [System.Serializable]
    public class StraightMoveTask : TargetMovementTask
    {
        public float targetDistanceGoal = .1f;

        public override float EvaluateDirection(Vector2 inputDirection) => MoveDecider.TargetDirectionScorer(inputDirection, targetDisplacement);

        public override NodeState Evaluate()
        {
            base.Evaluate();
            Vector2 horizontalDisplacement = targetDisplacement;
            if (horizontalDisplacement.sqrMagnitude < targetDistanceGoal)
            {
                // Debug.Log(targetDisplacement, tree);
                return NodeState.success;
            }
            return NodeState.running;
        }
    }

    [System.Serializable]
    public class TargetMinDistTask : TargetMovementTask
    {
        public float minDistance;

        // bool _withinMinDist;
        float _closenessMultiplier;
        Vector2 _targetNormalized;
        public override void PreEvaluation()
        {
            base.PreEvaluation();
            // try to be, one unit outside minDist
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
    public class TargetMaxDistTask : TargetMovementTask
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
}
