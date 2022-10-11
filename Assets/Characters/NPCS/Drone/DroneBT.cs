using System.Collections;
// using System.Collections.Generic;
using UnityEngine;
// using UnityEngine.Events;

using ColbyDoan.BehaviourTree;

namespace ColbyDoan
{
    public class DroneBT : BaseEnemyBT
    {
        [Header("Dependancies")]
        [SerializeField] FrictionManager frictionManager;
        [SerializeField] LineRenderer laserPointer;
        [SerializeField] EnemyIndicator indicator;

        public float forgetTargetDuration = 10;

        [SerializeField] FindTargetTask trackingTask;

        // pursuit tasks
        [SerializeField] DroneShootTask shootTask;
        [SerializeField] CirclingTask circleEnemyMovement;

        // investigation tasks
        [SerializeField] StraightMoveTask investigatePointTask;

        [SerializeField] IdleTask idleTask;

        protected override Node SetupTree()
        {
            void _EnterIdle()
            {
                indicator.SetIndicator("?");
                trackingTask.ForgetTarget();
                idleTask.ResetSequence();
            }
            var ownTransform = transform;
            void _MaintainHeight()
            {
                float distanceFromTargetHeight = 1.5f - ownTransform.position.z;
                float direction = Mathf.Sign(distanceFromTargetHeight);
                float damping = Mathf.Clamp01(Mathf.Abs(distanceFromTargetHeight / .4f));// .4 is the max distance when damping begins
                Vector3 finalV = character.kinematicObject.Velocity;
                finalV.z = 2 * damping * direction;
                character.kinematicObject.ForceTo(finalV);
            }

            trackingTask.OnTargetFound += delegate { indicator.SetIndicator("!"); };

            var root = new MultiTask
            (
                // maintain height at lvl 2, always succeeds
                new SimpleTask(_MaintainHeight),
                // find and update targets, succeeds if one is/was found
                trackingTask.AttachNodes(
                    idleTask,
                    new Selector
                    (
                        // target in los tasks
                        new Condition(FindTargetTask.targetLOSKey,
                            new MultiTask
                            (
                                circleEnemyMovement,
                                shootTask
                            )
                        ),
                        new MultiTask
                        (
                            // investigate last seen, fails when investigate point is reached
                            new LastSeenTimeCondition(forgetTargetDuration, new Inverter(investigatePointTask)),
                            // new SimpleTask(() => { print((bool)Root.GetData(_lastLOSKey)); }),
                            new SimpleTask(shootTask.InteruptAiming)
                        ),
                        new SimpleTask(_EnterIdle)
                    )
                )
            );

            return root;
        }

        // public void StartAiming(UnityAction animationCompleteCallback)
        // {
        //     animationCompleteCallback.Invoke();
        // }

        void OnDrawGizmos()
        {
            idleTask.DrawGizmos();

        }
        void OnDrawGizmosSelected()
        {
            trackingTask.DrawGizmos();
        }
    }

    [System.Serializable]
    public class DroneShootTask : EnemyNode
    {
        [SerializeField] ProjectileSkill skill;
        [SerializeField] LineRenderer laserPointer;
        [SerializeField] AudioSource fireNoise;
        public string targetKey = FindTargetTask.targetInfoKey;
        SightingInfo _currentTarget;

        public override void Initalize(ColbyDoan.BehaviourTree.Tree toSet)
        {
            base.Initalize(toSet);
            _currentTarget = (SightingInfo)GetData(targetKey);
        }

        public override NodeState Evaluate()
        {
#if UNITY_EDITOR
            tree.currentNode = "droneShootTask";
#endif

            enemyTree.character.FacingDirection = _currentTarget.KnownDisplacement;

            // Debug.Log($"rdy:{skill.Ready} LOS:{_currentTarget.HasLineOfSight} range:{_currentTarget.KnownDisplacement.sqrMagnitude < skill.FireRangeSqr}");

            laserPointer.SetPositions(new Vector3[] { skill.fireOrigin.position.GetDepthApparentPosition(), _currentTarget.KnownPos.GetDepthApparentPosition() });

            if (skill.Ready && state != NodeState.running)
            {
                if (_currentTarget.HasLineOfSight && _currentTarget.KnownDisplacement.sqrMagnitude < skill.FireRangeSqr)
                {
                    skill.TargetPos = _currentTarget.KnownPos;
                    coroutine = tree.StartCoroutine(ShootingAnimationCoroutine());
                    // return NodeState.failure;
                }
                return NodeState.success;
            }
            return NodeState.failure;
        }

        Coroutine coroutine;
        public void InteruptAiming()
        {
            // Debug.Log("Interuppting aim...", tree);
            if (coroutine != null)
            {
                // Debug.Log("Interuppting goooo!", tree);
                tree.StopCoroutine(coroutine);
            }
            laserPointer.enabled = false;
            state = NodeState.success;
        }
        IEnumerator ShootingAnimationCoroutine()
        {
            state = NodeState.running;

            //indicator.text = ".";
            //yield return new WaitForSeconds(.2f);
            //indicator.text = "..";
            //yield return new WaitForSeconds(.2f);
            //indicator.text = "...";
            //yield return new WaitForSeconds(.2f);
            //indicator.text = "...!";

            laserPointer.enabled = true;
            yield return new WaitForSeconds(1);
            laserPointer.enabled = false;
            yield return new WaitForSeconds(.05f);
            laserPointer.enabled = true;
            yield return new WaitForSeconds(.05f);
            laserPointer.enabled = false;
            yield return new WaitForSeconds(.05f);
            laserPointer.enabled = true;
            yield return new WaitForSeconds(.05f);
            laserPointer.enabled = false;
            yield return new WaitForSeconds(.05f);
            laserPointer.enabled = true;
            yield return new WaitForSeconds(.05f);
            laserPointer.enabled = false;
            yield return new WaitForSeconds(.05f);
            laserPointer.enabled = true;
            yield return new WaitForSeconds(.05f);
            laserPointer.enabled = false;

            fireNoise.Play();

            state = NodeState.success;

            skill.TargetPos = _currentTarget.KnownPos;
            skill.Activate();
            //indicator.text = "";
        }
    }
}
