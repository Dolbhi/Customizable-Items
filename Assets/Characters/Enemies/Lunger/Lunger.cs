using UnityEngine;

namespace ColbyDoan
{
    public class Lunger : EnemyBehaviour
    {
        [SerializeField] IdleState idleState = new IdleState();
        [SerializeField] PursuitTargetState pursuitState = new PursuitTargetState();

        [SerializeField] BobAndWeaveEvaluator bobAndWeaveMovement = new BobAndWeaveEvaluator();
        [SerializeField] CirclingEvaluator maintainDistanceMovement = new CirclingEvaluator();

        LungeSkill lungeSkill;// => Skills as LungeModule;

        protected override IState InitializeStateMachine(StateMachine stateMachine)
        {
            idleState.Init(this, stateMachine);
            pursuitState.Init(this, stateMachine);

            return idleState;
        }
        protected override void Awake()
        {
            base.Awake();

            lungeSkill = Skills[0] as LungeSkill;

            bobAndWeaveMovement.displacementProvider = tracker;
            maintainDistanceMovement.displacementProvider = tracker;

            maintainDistanceMovement.distanceToMaintain = lungeSkill.lungeDist;
        }
        float lastTimeWithNoLOS;
        public override void UpdateState()
        {
            TargetInfo target = tracker.currentTarget;
            if (!target.Exists)
            {
                // idle
                indicator.SetIndicator("?");
                SafeStateChange(idleState);
            }
            else
            {
                // aggro
                enableStateUpdateLoop = true;
                moveDecider.DirectionEvaluator = bobAndWeaveMovement;
                SafeStateChange(pursuitState);

                if (!lungeSkill.Ready)
                {
                    // stay away
                    moveDecider.DirectionEvaluator = maintainDistanceMovement;
                }
                else if (!target.HasLineOfSight)
                {
                    lastTimeWithNoLOS = Time.time;
                }
                else if (Time.time - lastTimeWithNoLOS > 1 && target.KnownDisplacement.sqrMagnitude <= lungeSkill.LungeDistSqr)
                {
                    // attack
                    lungeSkill.TargetPos = target.KnownPos;
                    lungeSkill.Activate();
                    character.statusEffects.GetStatus<SlowSE>("slow").ApplyStatus(3);
                    UpdateState();
                }
            }
        }
    }
    public abstract partial class EnemyBehaviour
    {
        [System.Serializable]
        protected class BobAndWeaveEvaluator : TargetEvaluator
        {
            public float skewAngle = 30;
            public int bobChance = 20;
            int weaveDirection = 1;

            public override void PreEvaluation()
            {
                base.PreEvaluation();
                // randomly change bob direction
                if (Random.Range(0, 100) < bobChance)
                    weaveDirection = -weaveDirection;
            }

            public override float EvaluateDirection(Vector2 inputDirection)
            {
                Vector2 weavingDir = Quaternion.Euler(0, 0, weaveDirection * skewAngle) * targetDisplacement;

                float closenessScore = MoveDecider.TargetDirectionScorer(inputDirection, targetDisplacement);
                float bobingScore = MoveDecider.TargetDirectionScorer(inputDirection, weavingDir);
                return bobingScore;
            }
        }
    }
}