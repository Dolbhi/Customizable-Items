// using System.Collections;
// using System.Collections.Generic;
using ColbyDoan.BehaviourTree;
using UnityEngine;

namespace ColbyDoan
{
    public class LungerBT : BaseEnemyBT
    {
        [SerializeField] EnemyIndicator indicator;

        public float forgetTargetDuration = 10;

        [SerializeField] FindTargetTask trackingTask = new FindTargetTask();
        [SerializeField] IdleTask idleTask = new IdleTask();

        // pursuit tasks
        [SerializeField] BobAndWeaveTask chaseTask = new BobAndWeaveTask();
        [SerializeField] WaitTask lungeAimTask = new WaitTask();
        [SerializeField] LungeTask lungeTask = new LungeTask();

        // cooldown movement
        [SerializeField] CirclingTask maintainDistanceTask = new CirclingTask();

        // investigation tasks
        [SerializeField] StraightMoveTask investigatePointTask = new StraightMoveTask();

        protected override Node SetupTree()
        {
            void _EnterIdle()
            {
                indicator.SetIndicator("?");
                trackingTask.ForgetTarget();
                idleTask.ResetSequence();
            }

            trackingTask.OnTargetFound += delegate { indicator.SetIndicator("!"); };

            return trackingTask.AttachNodes
            (
                idleTask,
                new Selector
                (
                    new SkillReadySelector(character.skills.skills[0],
                        new MultiTask(
                            // ensure conditions are met before attacking
                            new Condition(FindTargetTask.targetLOSKey,
                                new Selector
                                (
                                    new Condition(FindTargetTask.targetLastLOSKey, false,
                                        new SimpleTask(lungeAimTask.StartWait)
                                    ),
                                    new Sequence(lungeAimTask, lungeTask)
                                )
                            ),
                            // chase
                            chaseTask
                        ),
                        // back off if skill on cooldown
                        maintainDistanceTask
                    ),
                    new LastSeenTimeCondition(forgetTargetDuration, new Inverter(investigatePointTask)),
                    new SimpleTask(_EnterIdle)
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

    /// <summary>
    /// Lunges if ready and target is in range
    /// Copied from Shooter ShootTask
    /// </summary>
    [System.Serializable]
    public class LungeTask : EnemyNode
    {
        [SerializeField] LungeSkill skill;
        public string targetKey = FindTargetTask.targetInfoKey;
        SightingInfo _currentTarget;

        public override void Initalize(ColbyDoan.BehaviourTree.Tree toSet)
        {
            base.Initalize(toSet);
            _currentTarget = (SightingInfo)GetData(targetKey);
        }

        /// <summary>
        /// Attempt to lunge
        /// </summary>
        /// <returns> success -> skill is ready, failure -> skill not ready </returns>
        public override NodeState Evaluate()
        {
#if UNITY_EDITOR
            tree.currentNode = "shootTask";
#endif

            enemyTree.character.FacingDirection = _currentTarget.KnownDisplacement;

            // Debug.Log($"rdy:{skill.Ready} LOS:{_currentTarget.HasLineOfSight} range:{_currentTarget.KnownDisplacement.sqrMagnitude < skill.FireRangeSqr}");

            if (skill.Ready)
            {
                if (_currentTarget.HasLineOfSight && _currentTarget.KnownDisplacement.sqrMagnitude < skill.LungeDistSqr)
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
