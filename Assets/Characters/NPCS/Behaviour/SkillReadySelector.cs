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
}