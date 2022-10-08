// using System.Collections;
using System.Collections.Generic;
using System;

namespace ColbyDoan.BehaviourTree
{
    public enum NodeState { running, success, failure }

    public class Node
    {
        protected NodeState state = NodeState.failure;

        protected Node parent;
        protected Tree tree;
        // instead of parent store reference to contextData of the tree OR just the tree itself
        protected List<Node> children = new List<Node>();

        Dictionary<string, object> _contextData = new Dictionary<string, object>();

        public Node() { }
        public Node(NodeState startState)
        {
            state = startState;
        }

        protected int childCount;
        public Node(params Node[] childrenNodes)
        {
            foreach (Node child in childrenNodes)
                Attach(child);
        }

        /// <summary>
        /// Append node to list of children nodes
        /// </summary>
        /// <param name="child">Node to append</param>
        protected void Attach(Node child)
        {
            // UnityEngine.Debug.Log(child.ToString());
            child.parent = this;
            children.Add(child);
            childCount++;
        }

        /// <summary>
        /// Called right after SetupTree in the Tree class Start method, sets tree of all child nodes
        /// </summary>
        public virtual void Initalize(Tree toSet)
        {
            tree = toSet;
            foreach (Node child in children)
            {
                child.Initalize(tree);
            }
        }

        public virtual NodeState Evaluate()
        {
            return state;
        }

        public void SetData(string key, object value)
        {
            _contextData[key] = value;
        }
        public object GetData(string key)
        {
            // check self for data
            object value;
            if (_contextData.TryGetValue(key, out value))
                return value;

            // check parents (and their parents) for data
            if (parent != null)
                return parent.GetData(key);

            return null;
        }
        public bool ClearData(string key)
        {
            // check self for data
            if (_contextData.ContainsKey(key))
            {
                _contextData.Remove(key);
                return true;
            }

            // check parents (and their parents) for data
            if (parent != null)
                return parent.ClearData(key);

            return false;
        }
    }

    /// <summary>
    /// Evaluates child if conditonKey equals given value, if no value is given conditionKey is evaluated as a bool
    /// </summary>
    public class Condition : Node
    {
        string _conditionKey;
        object _value;

        public Condition(string conditionKey, object value, Node child) : base(child) { _conditionKey = conditionKey; _value = value; }
        public Condition(string conditionKey, Node child) : this(conditionKey, true, child) { }

        public override NodeState Evaluate()
        {
            var condition = GetData(_conditionKey);
            if (condition == null || !condition.Equals(_value))
            {
                return NodeState.failure;
            }
            return children[0].Evaluate();
        }
    }

    /// <summary>
    /// Evaluates the given node and returns the inverse NodeState
    /// </summary>
    /// <remarks>
    /// failure => success, success => failure, running => running
    /// </remarks>
    public class Inverter : Node
    {
        public Inverter(Node child) : base(child) { }

        public override NodeState Evaluate()
        {
            switch (children[0].Evaluate())
            {
                case NodeState.failure:
                    return NodeState.success;
                case NodeState.success:
                    return NodeState.failure;
                case NodeState.running:
                    return NodeState.running;
                default:
                    return NodeState.success;
            }
        }
    }

    /// <summary>
    /// Runs the given action when executed, always succeeds
    /// </summary>
    public class SimpleTask : Node
    {
        Action _task;

        public SimpleTask(Action task) : base()
        {
            _task = task;
        }

        public override NodeState Evaluate()
        {
            _task.Invoke();
            return NodeState.success;
        }
    }

    /// <summary>
    /// Triggers multiple nodes in order regardless of their result, returns state of the first node
    /// </summary>
    public class MultiTask : Node
    {
        public MultiTask(params Node[] nodes) : base(nodes) { }

        public override NodeState Evaluate()
        {
            var result = children[0].Evaluate();
            for (int i = 1; i < childCount; i++)
            {
                children[i].Evaluate();
            }
            return result;
        }
    }

    /// <summary>
    /// Tries to evaluate all children up to one that has failed or is running
    /// </summary>
    /// <remarks>
    /// Success -> all nodes succeed,
    /// Running -> final node is running,
    /// Failure -> final nodes failed
    /// </remarks>
    public class Sequence : Node
    {
        public Sequence() : base() { }
        public Sequence(params Node[] children) : base(children) { }

        public override NodeState Evaluate()
        {
            for (int i = 0; i < childCount; i++)
            {
                var node = children[i];
                switch (node.Evaluate())
                {
                    case NodeState.failure:
                        state = NodeState.failure;
                        return state;
                    case NodeState.success:
                        continue;
                    case NodeState.running:
                        state = NodeState.running;
                        return state;
                    default:
                        state = NodeState.success;
                        return state;
                }
            }
            state = NodeState.success;
            return state;
        }
    }

    /// <summary>
    /// Tries to evaluate all children up to one that does not fail
    /// </summary>
    /// <remarks>
    /// Success -> selected node succeeds,
    /// Running -> selected node is running,
    /// Failure -> all nodes failed
    /// </remarks>
    public class Selector : Node
    {
        public Selector() : base() { }
        public Selector(params Node[] children) : base(children) { }

        public override NodeState Evaluate()
        {
            for (int i = 0; i < childCount; i++)
            {
                var node = children[i];
                switch (node.Evaluate())
                {
                    case NodeState.failure:
                        continue;
                    case NodeState.success:
                        state = NodeState.success;
                        return state;
                    case NodeState.running:
                        state = NodeState.running;
                        return state;
                    default:
                        continue;
                }
            }
            state = NodeState.failure;
            return state;
        }
    }
}
