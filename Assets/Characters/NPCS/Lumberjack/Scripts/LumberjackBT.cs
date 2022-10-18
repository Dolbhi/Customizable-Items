// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;

using ColbyDoan.BehaviourTree;

namespace ColbyDoan
{
    /// <summary>
    /// Throws and swing axe enemy
    /// </summary>
    public class LumberjackBT : BaseEnemyBT
    {
        public float forgetTargetDuration = 10;
        public float axeStunDuration = 2;
        public float recallDelay = 3;

        [SerializeField] EnemyIndicator indicator;

        [SerializeField] ThrowSkill throwSkill;
        [SerializeField] RecallSkill recallSkill;

        [SerializeField] FindTargetTask trackingTask;

        // pursuit tasks
        [SerializeField] StraightMoveTask chaseTask;
        [SerializeField] MeleeTask meleeTask;

        // investigation tasks
        [SerializeField] StraightMoveTask investigatePointTask;

        [SerializeField] IdleTask idleTask;

        protected override Node SetupTree()
        {
            // add axe stun effect
            character.artifacts.metaTriggers.Add(AxeTracker.axeHitID, _Stun);

            trackingTask.OnTargetFound += delegate { indicator.SetIndicator("!"); };

            WaitTask recallWaitTask = new WaitTask();
            recallWaitTask.waitTime = recallDelay;

            return trackingTask.AttachNodes(
                idleTask,
                new MultiTask(
                    new Selector
                    (
                        new Condition(FindTargetTask.targetLOSKey,
                            // attacking task, chase and try various attacks with priority
                            new MultiTask(
                                chaseTask,
                                new Selector(
                                    meleeTask,
                                    // if throw is successful start a wait
                                    new Sequence(new UseSkillTask(throwSkill), new SimpleTask(recallWaitTask.StartWait)),
                                    // only recall after wait
                                    new Sequence(recallWaitTask, new UseSkillTask(recallSkill))
                                )
                            )
                        ),
                        new LastSeenTimeCondition(forgetTargetDuration, new Inverter(investigatePointTask)),
                        new SimpleTask(_EnterIdle)
                    ),
                    new DontDropBelowTargetTask()
                )
            );
        }
        void _Stun(TriggerContext context)
        {
            Debug.Log("axe hit");
            context.GetCharacter?.statusEffects.GetStatus<StunSE>("stun").ApplyStatus(axeStunDuration);
        }
        void _EnterIdle()
        {
            indicator.SetIndicator("?");
            trackingTask.ForgetTarget();
            idleTask.ResetSequence();
        }
    }

}
