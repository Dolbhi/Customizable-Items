using System.Collections;
// using System.Collections.Generic;
using UnityEngine;

namespace ColbyDoan.BehaviourTree
{
    public abstract class Tree : MonoBehaviour
    {
        // public float brainTickPeriod = .1f;
        Node _root;
        Node _tickedNode;

#if UNITY_EDITOR
        // static System.Text.StringBuilder nodeLog = new System.Text.StringBuilder();
        // [TextArea]
        public string currentNode;
        public Node Root => _root;

        // static public void LogNode(Node node)
        // {
        //     nodeLog.Append(node.ToString());
        //     nodeLog.Append(" ");
        // }
#endif
        public NodeState nodeState;

        protected virtual void Start()
        {
            _root = SetupTree();
            _root.Initalize(this);
            _tickedNode = _root;
            // brainTickWait = new WaitForSeconds(brainTickPeriod);
            // StartCoroutine(BrainTickCoroutine());
        }
        // WaitForSeconds brainTickWait;
        // IEnumerator BrainTickCoroutine()
        // {
        //     while (true)
        //     {
        //         nodeState = _tickedNode.Evaluate();
        //         // #if UNITY_EDITOR
        //         //             currentNode = nodeLog.ToString();
        //         //             nodeLog.Clear();
        //         // #endif
        //         yield return brainTickWait;
        //     }
        // }
        void Update()
        {
            nodeState = _tickedNode.Evaluate();
        }

        public void SetDataToRoot(string key, object value)
        {
            _root.SetData(key, value);
        }
        public void SetTickedNode(Node node)
        {
            _tickedNode = node;
        }
        public void ResetTickedNode()
        {
            _tickedNode = _root;
        }

        /// <summary>
        /// Build tree of nodes
        /// </summary>
        /// <returns> root node of the tree </returns>
        protected abstract Node SetupTree();
    }
}
