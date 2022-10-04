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

        // pursuit tasks
        [SerializeField] StraightMoveTask chaseTask = new StraightMoveTask();
        [SerializeField] MeleeTask meleeTask = new MeleeTask();
        [SerializeField] CirclingTask circleEnemyMovement = new CirclingTask();

        // investigation tasks
        [SerializeField] StraightMoveTask investigatePointTask = new StraightMoveTask();

        [SerializeField] IdleTask idleTask = new IdleTask();

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

            return new Selector
            (
                new Sequence
                (
                    trackingTask,
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
                        new Condition(FindTargetTask.targetNewKey, new SimpleTask(_OnIdleExit))
                    )
                ),
                idleTask
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
}
