// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using ColbyDoan.BehaviourTree;

namespace ColbyDoan
{
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
            void _OnIdleExit()
            {
                indicator.SetIndicator("!");
            }

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
                            new Condition(FindTargetTask.targetNewKey, new SimpleTask(_OnIdleExit)),
                            new Selector
                            (
                                new Condition(FindTargetTask.targetLOSEnterKey, new SimpleTask(aimTask.StartWait)),
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
        [SerializeField] ProjectileSkill skill;
        // [SerializeField] float attackRangeSqr = 10;
        public string targetKey = FindTargetTask.targetInfoKey;
        TargetInfo _currentTarget;

        public override void Initalize(ColbyDoan.BehaviourTree.Tree toSet)
        {
            base.Initalize(toSet);
            _currentTarget = (TargetInfo)GetData(targetKey);
        }

        public override NodeState Evaluate()
        {
#if UNITY_EDITOR
            tree.currentNode = "shootTask";
#endif

            enemyTree.character.FacingDirection = _currentTarget.KnownDisplacement;

            // Debug.Log($"rdy:{skill.Ready} LOS:{_currentTarget.HasLineOfSight} range:{_currentTarget.KnownDisplacement.sqrMagnitude < skill.FireRangeSqr}");

            if (skill.Ready)
            {
                if (_currentTarget.HasLineOfSight && _currentTarget.KnownDisplacement.sqrMagnitude < skill.FireRangeSqr)
                {
                    skill.TargetPos = _currentTarget.KnownPos;
                    skill.Activate();
                    // return NodeState.failure;
                }
                return NodeState.success;
            }
            return NodeState.failure;
        }
    }
}
