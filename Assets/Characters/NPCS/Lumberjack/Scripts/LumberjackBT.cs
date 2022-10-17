using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ColbyDoan.BehaviourTree;

namespace ColbyDoan
{
    /// <summary>
    /// Throws and swing axe enemy
    /// </summary>
    public class LumberjackBT : BaseEnemyBT
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
                            new Selector
                            (
                                new Sequence(meleeTask, chaseTask),
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
    }
}
