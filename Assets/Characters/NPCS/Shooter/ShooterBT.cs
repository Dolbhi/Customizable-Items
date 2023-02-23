// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using ColbyDoan.BehaviourTree;

namespace ColbyDoan
{
    using CharacterBase;

    public class ShooterBT : BaseEnemyBT
    {
        [SerializeField] EnemyIndicator indicator;

        public float forgetTargetDuration = 10;

        [SerializeField] FindTargetTask trackingTask;

        // pursuit tasks
        [SerializeField] ShootTask shootTask;
        [SerializeField] WaitTask aimTask;

        [SerializeField] float[] pursuitMovementWeights;
        [SerializeField] TargetMinDistTask keepDistEvaluator;
        [SerializeField] TargetMaxDistTask stayInRangeEvaluator;
        [SerializeField] SpreadOutTask spreadOutEvaluator;

        // investigation tasks
        [SerializeField] StraightMoveTask investigatePointTask;

        [SerializeField] IdleTask idleTask;

        protected override Node SetupTree()
        {
            void _EnterIdle()
            {
                indicator.SetIndicator("?");
                trackingTask.ForgetTarget();
                idleTask.ResetSequence();
            }

            trackingTask.OnTargetFound += delegate { indicator.SetIndicator("!"); };

            var root = trackingTask.AttachNodes(
                idleTask,
                new Selector
                (
                    new Condition(FindTargetTask.targetLOSKey,
                        // move and shoot
                        new MultiTask
                        (
                            // new Node(NodeState.running),
                            new CombinedMovementTask(keepDistEvaluator, stayInRangeEvaluator, spreadOutEvaluator),
                            new Selector
                            (
                                // start wait if target just entered LOS
                                new Condition(FindTargetTask.targetLastLOSKey, false,
                                    new SimpleTask(aimTask.StartWait)
                                ),
                                new Sequence(aimTask, shootTask)
                            )
                        )
                    ),
                    // investigate last seen
                    new LastSeenTimeCondition(forgetTargetDuration, new Inverter(investigatePointTask)),
                    new SimpleTask(_EnterIdle)
                )
            );

            return root;
        }

        public void ShootingAnimation(UnityAction animationCompleteCallback)
        {
            void FinalAnimation()
            {
                indicator.PlayIndicatorAnimation(seqence: ("...!", .2f));
                animationCompleteCallback.Invoke();
            }

            indicator.PlayIndicatorAnimation(FinalAnimation, (".", .2f), ("..", .2f), ("...", .2f));
        }

        void OnDrawGizmos()
        {
            idleTask.DrawGizmos();
        }
        void OnDrawGizmosSelected()
        {
            trackingTask.DrawGizmos();
        }
    }

    [System.Serializable]
    public class ShootTask : EnemyNode
    {
        [SerializeField] Skill _skill;
        // [SerializeField] float attackRangeSqr = 10;
        public string targetKey = FindTargetTask.targetInfoKey;
        SightingInfo _currentTarget;

        public override void Initalize(ColbyDoan.BehaviourTree.Tree toSet)
        {
            base.Initalize(toSet);
            _currentTarget = (SightingInfo)GetData(targetKey);
        }

        public override NodeState Evaluate()
        {
#if UNITY_EDITOR
            tree.currentNode = "shootTask";
#endif

            enemyTree.character.FacingDirection = _currentTarget.KnownDisplacement;

            // Debug.Log($"rdy:{skill.Ready} LOS:{_currentTarget.HasLineOfSight} range:{_currentTarget.KnownDisplacement.sqrMagnitude < skill.FireRangeSqr}");

            if (_skill.Ready)
            {
                if (_skill.TargetInRange(_currentTarget))
                {
                    _skill.TargetPos = _currentTarget.KnownPos;
                    _skill.Activate();
                    // return NodeState.failure;
                }
                return NodeState.success;
            }
            return NodeState.failure;
        }
    }
}
