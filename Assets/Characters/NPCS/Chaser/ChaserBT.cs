// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;

using ColbyDoan.BehaviourTree;

namespace ColbyDoan
{
    public class ChaserBT : BaseEnemyBT
    {
        [SerializeField] EnemyIndicator indicator;

        public float forgetTargetDuration = 10;

        [SerializeField] FindTargetTask trackingTask;

        // pursuit tasks
        [SerializeField] StraightMoveTask chaseTask;
        [SerializeField] MeleeTask meleeTask;
        [SerializeField] CirclingTask circleEnemyMovement;

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

            return trackingTask.AttachNodes(
                idleTask,
                new MultiTask(
                    new Selector
                    (
                        new Condition(FindTargetTask.targetLOSKey,
                            new Selector
                            (
                                new Sequence(meleeTask, chaseTask),
                                circleEnemyMovement
                            )
                        ),
                        new LastSeenTimeCondition(forgetTargetDuration, new Inverter(investigatePointTask)),
                        new SimpleTask(_EnterIdle)
                    ),
                    new DontDropBelowTargetTask(),
                    new Condition(FindTargetTask.targetNewKey, new SimpleTask(_OnIdleExit))
                )
            );
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

    public class DontDropBelowTargetTask : EnemyNode
    {
        public string targetKey = FindTargetTask.targetInfoKey;
        TargetInfo _currentTarget;
        MoveDecider _decider;

        public override void Initalize(ColbyDoan.BehaviourTree.Tree toSet)
        {
            base.Initalize(toSet);
            _currentTarget = (TargetInfo)GetData(targetKey);
            _decider = enemyTree.decider;
        }

        public override NodeState Evaluate()
        {
            _decider.allowDrops = _currentTarget.KnownDisplacement.z < -.5f;
            return NodeState.success;
        }
    }

    [System.Serializable]
    public class MeleeTask : EnemyNode
    {
        [SerializeField] MeleeSkill skill;
        [SerializeField] float attackRangeSqr = 1;
        public string targetKey = FindTargetTask.targetInfoKey;
        TargetInfo _currentTarget;

        public override void Initalize(ColbyDoan.BehaviourTree.Tree toSet)
        {
            _currentTarget = (TargetInfo)GetData(targetKey);
            base.Initalize(toSet);
        }

        public override NodeState Evaluate()
        {
#if UNITY_EDITOR
            tree.currentNode = "meleeTask";
#endif

            enemyTree.character.FacingDirection = _currentTarget.KnownDisplacement;

            if (skill.Active)
                return NodeState.success;

            if (skill.Ready)
            {
                if (_currentTarget.HasLineOfSight && _currentTarget.KnownDisplacement.sqrMagnitude < attackRangeSqr)
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
