using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ColbyDoan
{
    public class Lumberjack : EnemyBehaviour
    {
        [SerializeField] IdleState idleState = new IdleState();
        [SerializeField] LumberjackPursuit pursuitState = new LumberjackPursuit();
        [SerializeField] LumberJackCatching catchingState = new LumberJackCatching();

        // LumberjackSkills lumberjackSkills;// unset

        protected override void Awake()
        {
            base.Awake();
        }

        protected override IState InitializeStateMachine(StateMachine stateMachine)
        {
            idleState.Init(this, stateMachine);
            pursuitState.Init(this, stateMachine);
            catchingState.Init(this, stateMachine);

            return null;
        }

        public override void UpdateState()
        {
            throw new System.NotImplementedException();
        }

        [System.Serializable]
        class LumberjackPursuit : PureMoveDeciderState<Lumberjack>
        {
            const float minDistSqr = 1.4f * 1.4f;
            Cooldown waitToRecallCooldown = new Cooldown(5);
            public override float EvaluateDirection(Vector2 inputDirection)
            {
                if (CurrentTarget.KnownDisplacement.sqrMagnitude < minDistSqr)
                    return 0;

                return base.EvaluateDirection(inputDirection);
            }

            public override void Update()
            {
                base.Update();
                return;

                // set target
                //GetCharacter.FacingDirection = CurrentTarget.KnownDisplacement;
                RecallSkill recallSkill = null;
                ThrowSkill throwSkill = null;
                // MeleeSkill meleeSkill = null;
                float targetDist = CurrentTarget.KnownDisplacement.magnitude;

                // attack
                if (CurrentTarget.HasLineOfSight)
                {
                    container.Skills[0].TargetPos = CurrentTarget.KnownPos;
                    // melee
                    if (targetDist < 1)//melee range
                    {
                        container.Skills[0].Activate();
                    }
                    // throw
                    else
                    {
                        if (throwSkill.Ready && targetDist < throwSkill.range)
                        {
                            container.Skills[0].Activate();
                            waitToRecallCooldown.StartCooldown();
                            // sm.ChangeState(container.noAxeState);
                        }
                    }
                }
                // catch
                if (recallSkill.Ready && waitToRecallCooldown.Ready)
                {
                    print("catching");
                    container.Skills[0].Activate();
                    sm.ChangeState(container.catchingState);
                }
            }
        }

        [System.Serializable]
        class LumberJackCatching : EnemyState<Lumberjack>
        {
            public override void Update()
            {
                base.Update();

                // GetCharacter.FacingDirection = container.lumberjackSkills.axe.transform.position - container.transform.position;

                // // change state
                // if (!container.lumberjackSkills.recallSkill.Active)
                // {
                //     sm.ChangeState(container.pursuitState);
                // }
            }
        }
    }
}
