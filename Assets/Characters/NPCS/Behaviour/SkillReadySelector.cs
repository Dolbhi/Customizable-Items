// using UnityEngine;

using ColbyDoan.BehaviourTree;

namespace ColbyDoan
{
    /// <summary>
    /// Chooses action to take based on skill status
    /// </summary>
    public class SkillReadySelector : EnemyNode
    {
        Skill _skill;

        public SkillReadySelector(Skill skill, Node skillReadyNode, Node notRdyNode) : base(skillReadyNode, notRdyNode)
        {
            _skill = skill;
        }

        public override NodeState Evaluate()
        {
            if (_skill.Ready)
                state = children[0].Evaluate();
            else
                state = children[1].Evaluate();
            // UnityEngine.Debug.Log("Skill ready: " + _skill.Ready + " child state: " + state + " target los: " + (bool)GetData(FindTargetTask.targetLOSKey));
            return state;
        }
    }

    /// <summary>
    /// Use a skill if specified condition is satisfied, returns success if activated, running if active and failure if not ready
    /// </summary>
    [System.Serializable]
    public class UseSkillTask : EnemyNode
    {
        public Skill skill;
        public string targetKey = FindTargetTask.targetInfoKey;
        protected SightingInfo currentTarget;

        public UseSkillTask() : base() { }
        /// <summary>
        /// Use a skill if specified condition is satisfied, returns success if activated, running if active and failure if not ready
        /// </summary>
        public UseSkillTask(Skill skill) : base()
        {
            this.skill = skill;
        }

        public override void Initalize(ColbyDoan.BehaviourTree.Tree toSet)
        {
            currentTarget = (SightingInfo)GetData(targetKey);
            base.Initalize(toSet);
        }

        /// <summary>
        /// Check if skill should be activated, by default uses skill.TargetInRange()
        /// </summary>
        /// <returns> if skill should be activated </returns>
        protected virtual bool ShouldActivate()
        {
            return skill.TargetInRange(currentTarget);
        }

        public override NodeState Evaluate()
        {
#if UNITY_EDITOR
            tree.currentNode = "skillTask";
#endif

            enemyTree.character.FacingDirection = currentTarget.KnownDisplacement;

            if (skill.Active)
                return NodeState.running;

            if (skill.Ready)
            {
                if (ShouldActivate())
                {
                    skill.TargetPos = currentTarget.KnownPos;
                    skill.Activate();
                    return NodeState.success;
                }
            }
            return NodeState.failure;
        }
    }
}