using System.Collections;
// using System.Collections.Generic;
using UnityEngine;

namespace ColbyDoan
{
    using BehaviourTree;
    using Physics;

    [System.Serializable]
    public class IdleTask : EnemyNode
    {
        [SerializeField] RandomWaitTask waitTask = new RandomWaitTask();
        [SerializeField] PickSpotTask pickSpotTask = new PickSpotTask();
        [SerializeField] StraightMoveTask goToSpotTask = new StraightMoveTask();

        Sequence _idleSequence;

        public IdleTask() : base()
        {
            _idleSequence = new Sequence(waitTask, pickSpotTask, goToSpotTask, new SimpleTask(ResetSequence));
            // have movement take displacement from pickSpot
            goToSpotTask.displacementKey = PickSpotTask.displacementKey;
            Attach(_idleSequence);
        }

        public override NodeState Evaluate()
        {
            enemyTree.decider.allowDrops = false;
            return _idleSequence.Evaluate();
        }

        public void ResetSequence()
        {
            enemyTree.moveManager.speedMultiplier = 0;
            pickSpotTask.Reset();
            waitTask.StartWait();
        }

        public void DrawGizmos()
        {
            Gizmos.color = Color.grey;
            var pos = pickSpotTask.GetData("random_position");
            if (pos != null)
            {
                // Debug.Log("help");
                Gizmos.DrawCube((Vector3)pos, Vector3.one * .2f);
                Gizmos.DrawLine(enemyTree.character.transform.position, (Vector3)pos);
            }
        }
    }

    /// <summary> When StartWait is called the node enters the running state until the time runs out </summary>
    [System.Serializable]
    public class WaitTask : Node
    {
        public float waitTime = 2;

        Coroutine _coroutine;

        public override void Initalize(ColbyDoan.BehaviourTree.Tree tree)
        {
            base.Initalize(tree);
            state = NodeState.success;
        }

        public override NodeState Evaluate()
        {
#if UNITY_EDITOR
            tree.currentNode = "WaitTask";
#endif
            return state;
        }

        /// <summary>
        /// Set state to running and wait a duration
        /// Afterwards set state to success
        /// </summary>
        public void StartWait()
        {
            if (_coroutine != null)
                tree.StopCoroutine(_coroutine);
            _coroutine = tree.StartCoroutine(WaitCoroutine(waitTime));
        }
        IEnumerator WaitCoroutine(float duration)
        {
            state = NodeState.running;
            yield return new WaitForSeconds(duration);
            state = NodeState.success;
        }
    }

    /// <summary> When StartWait is called the node enters the running state until the time runs out </summary>
    [System.Serializable]
    public class RandomWaitTask : Node
    {
        public float minWait = 2;
        public float maxWait = 4;

        Coroutine _coroutine;

        public override void Initalize(ColbyDoan.BehaviourTree.Tree tree)
        {
            base.Initalize(tree);
            state = NodeState.success;
        }

        public override NodeState Evaluate()
        {
#if UNITY_EDITOR
            tree.currentNode = "RandomWaitTask";
#endif
            return state;
        }

        /// <summary>
        /// Set state to running and wait a random duration
        /// Afterwards set state to success
        /// </summary>
        public void StartWait()
        {
            if (_coroutine != null)
                tree.StopCoroutine(_coroutine);
            var waitTime = UnityEngine.Random.Range(minWait, maxWait);
            _coroutine = tree.StartCoroutine(WaitCoroutine(waitTime));
        }
        IEnumerator WaitCoroutine(float duration)
        {
            state = NodeState.running;
            yield return new WaitForSeconds(duration);
            state = NodeState.success;
        }
    }
    /// <summary> When evaluated finds an in sight pos, stores ref to self in parent under "spot_picker" </summary>
    [System.Serializable]
    public class PickSpotTask : EnemyNode
    {
        public bool needGround = true;
        public float idleMaxDist = 5;
        public float clearance = .7f;
        Transform _transform;
        public const string displacementKey = "spot_displacement";
        Vector3 _chosenSpot;

        public override void Initalize(ColbyDoan.BehaviourTree.Tree tree)
        {
            base.Initalize(tree);
            _transform = enemyTree.character.transform;
        }

        public override NodeState Evaluate()
        {
#if UNITY_EDITOR
            tree.currentNode = "PickSpotTask";
#endif
            // check if _chosenSpot is already found, update displacement
            if (state == NodeState.success)
            {
                parent.SetData(displacementKey, _chosenSpot - _transform.position);
                return NodeState.success;
            }

            // keep searching for a wanderPos until one without obstacles is found
            Vector3 pos = _transform.position;
            Vector2 direction;
            int attempts = 0;
            do
            {
                _chosenSpot = (Vector3)UnityEngine.Random.insideUnitCircle.normalized * idleMaxDist + _transform.position;
                direction = _chosenSpot - pos;
                attempts++;
                if (attempts > 20)
                {
                    state = NodeState.failure;
                    return NodeState.failure;
                }
            }
            while (PhysicsSettings.CheckForSolids(_chosenSpot, clearance) || PhysicsSettings.SolidsLinecast(pos, _chosenSpot) || (needGround && TileManager.Instance.PitLinecast(pos, _chosenSpot) < direction.magnitude));

            parent.SetData(displacementKey, _chosenSpot - _transform.position);
            state = NodeState.success;
            return NodeState.success;
        }

        public void Reset()
        {
            state = NodeState.failure;
        }
    }


}
