// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;

using ColbyDoan.BehaviourTree;

namespace ColbyDoan
{
    /// <summary>
    /// Allows for interfacing with character components
    /// </summary>
    public abstract class BaseEnemyBT : ColbyDoan.BehaviourTree.Tree, IAutoDependancy<Character>
    {
        public Character Dependancy { set { character = value; } }
        public Character character;
        public MovementManager moveManager;
        public MoveDecider decider;
    }

    /// <summary>
    /// EnemyNodes must have BaseEnemyBT as their trees
    /// </summary>
    public class EnemyNode : Node
    {
        public EnemyNode() : base() { }
        public EnemyNode(params Node[] nodes) : base(nodes) { }

        protected BaseEnemyBT enemyTree;
        public override void Initalize(ColbyDoan.BehaviourTree.Tree toSet)
        {
            enemyTree = (BaseEnemyBT)toSet;
            base.Initalize(toSet);
        }
    }
}
