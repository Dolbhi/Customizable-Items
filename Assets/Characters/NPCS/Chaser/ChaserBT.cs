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

            trackingTask.OnTargetFound += delegate { indicator.SetIndicator("!"); };

            return trackingTask.AttachNodes(
                idleTask,
                new MultiTask(
                    new Selector
                    (
                        new Condition(FindTargetTask.targetLOSKey,
                            // if skill ready do chase and attack, else do circling
                            new SkillReadySelector(meleeTask.GetSkill(),
                                new MultiTask(chaseTask, meleeTask),
                                circleEnemyMovement
                            )
                        ),
                        new LastSeenTimeCondition(forgetTargetDuration, new Inverter(investigatePointTask)),
                        new SimpleTask(_EnterIdle)
                    ),
                    new DontDropBelowTargetTask()
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
        SightingInfo _currentTarget;
        MoveDecider _decider;

        public override void Initalize(ColbyDoan.BehaviourTree.Tree toSet)
        {
            base.Initalize(toSet);
            _currentTarget = (SightingInfo)GetData(targetKey);
            _decider = enemyTree.decider;
        }

        public override NodeState Evaluate()
        {
            _decider.allowDrops = _currentTarget.KnownDisplacement.z < -.5f;
            return NodeState.success;
        }
    }

    /// <summary>
    /// Uses the melee skill if target within range, returns success if activated, running if active and failure if not ready
    /// </summary>
    [System.Serializable]
    public class MeleeTask : EnemyNode
    {
        [SerializeField] Skill _skill;
        [SerializeField] float _attackRangeSqr = 1;
        public string targetKey = FindTargetTask.targetInfoKey;
        SightingInfo _currentTarget;

        public Skill GetSkill()
        {
            return _skill;
        }
        public void SetSkill(Skill skill)
        {
            _skill = skill;
        }

        public override void Initalize(ColbyDoan.BehaviourTree.Tree toSet)
        {
            _currentTarget = (SightingInfo)GetData(targetKey);
            base.Initalize(toSet);
        }

        public override NodeState Evaluate()
        {
#if UNITY_EDITOR
            tree.currentNode = "meleeTask";
#endif

            enemyTree.character.FacingDirection = _currentTarget.KnownDisplacement;

            if (_skill.Active)
                return NodeState.running;

            if (_skill.Ready)
            {
                if (_currentTarget.HasLineOfSight && _currentTarget.KnownDisplacement.sqrMagnitude < _attackRangeSqr)
                {
                    _skill.TargetPos = _currentTarget.KnownPos;
                    _skill.Activate();
                    return NodeState.success;
                }
            }
            return NodeState.failure;
        }
    }
}
