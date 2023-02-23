// using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

using ColbyDoan.BehaviourTree;
using ColbyDoan.Attributes;

namespace ColbyDoan
{
    using CharacterBase;
    using Physics;

    /// <summary>
    /// Selector node that looks for targets and banches depending on if one is found
    /// TOCONSIDER: Trigger OnTargetFound with target switching, also changing is own state based of target status
    /// </summary>
    [System.Serializable]
    public class FindTargetTask : EnemyNode
    {
        [SerializeField]
        [ReadOnly]
        Transform _currentTarget;
        [SerializeField]
        [ReadOnly]
        SightingInfo _sightingInfo = new SightingInfo();
        /// <summary> Targets the target tracker will look for </summary>
        HashSet<Transform> _targets;

        public float insideFOVRange = 20;
        public float outsideFOVRange = 10;
        [Range(0, 180)]
        public float fov = 75;
        public bool needsLOS = true;

        public event Action OnTargetFound = delegate { };
        public event Action OnLOSEnter = delegate { };

        public const string targetInfoKey = "target_info";
        public const string targetDisplaceKey = "target_displacement";
        public const string targetLOSKey = "target_los";
        public const string targetLastLOSKey = "target_last_los";

        Character _character;
        Transform _transform;

        public override void Initalize(ColbyDoan.BehaviourTree.Tree toSet)
        {
            _targets = RootTracker.GetSet(TrackerTag.Ally);
            SetData(targetInfoKey, _sightingInfo);

            base.Initalize(toSet);

            _character = enemyTree.character;
            _transform = _character.transform;

            enemyTree.character.healthManager.OnHurt += _InvestigateDamage;
        }

        /// <summary>
        /// If there is no current target, set hurt direction as a sighting point
        /// </summary>
        /// <param name="context"> hurt infomation</param>
        void _InvestigateDamage(HurtInfo context)
        {
            if (!_currentTarget)
            {
                // Debug.Log(context.direction.normalized);
                _sightingInfo.UpdatePos(context.direction.normalized + _transform.position, _transform.position, false, false);
            }
        }

        /// <summary>
        /// Set child nodes to evaluate based on outcomes
        /// </summary>
        /// <param name="noTargetNode"> Node to evaluate when no target is found </param>
        /// <param name="yesTargetNode"> Node to evaluate when target is found </param>
        /// <returns> FindTargetTask node itself </returns>
        public Node AttachNodes(Node noTargetNode, Node yesTargetNode)
        {
            Attach(noTargetNode);
            Attach(yesTargetNode);
            return this;
        }

        /// <summary>
        /// Updates info for current target, finding a new target if more favourable
        /// If no targets, look for one
        /// </summary>
        /// <returns>evaluation of the first node if no points to investigate, second if there is</returns>
        public override NodeState Evaluate()
        {
            SetData(targetLastLOSKey, _sightingInfo.HasLineOfSight);

            // find targets/new targets and update LOS
            bool hasLOS = false;
            if (_currentTarget)
            {
                // check for more favourable enemies
                var pos = _transform.position;
                float displacementToBeat = _sightingInfo.KnownDisplacement.magnitude * (_sightingInfo.HasLineOfSight ? .2f : .5f);
                foreach (Transform enemy in _targets)
                {
                    // update los of current target
                    if (enemy == _currentTarget)
                        hasLOS = _CanSeePoint(_currentTarget.position);
                    // New enemy must be many times closer before a switch is considered
                    else if (Vector2.Distance(enemy.transform.position, pos) < displacementToBeat)
                        // set new target
                        if (TrySetTarget(enemy))
                            hasLOS = true;
                }
            }
            else
            {
                // search for any valid enemies
                foreach (Transform enemy in _targets)
                {
                    // print(enemy.name + " " + enemy.transform.position.z);
                    if (TrySetTarget(enemy))
                    {
                        OnTargetFound.Invoke();
                        hasLOS = true;
                        break; // remove if there are comparisions within TrySetTarget
                    }
                }
            }

            // update info
            if (_currentTarget)
                _sightingInfo.UpdatePos(_currentTarget.position, _transform.position, hasLOS, needsLOS);
            else
                _sightingInfo.UpdatePos(Vector3.zero, _transform.position, hasLOS, needsLOS);

            SetData(targetDisplaceKey, _sightingInfo.KnownDisplacement);
            SetData(targetLOSKey, _sightingInfo.HasLineOfSight);

            // decide child node to evaluate
            if (_sightingInfo.investigated)
            {
                return children[0].Evaluate();
            }
            else
            {
                return children[1].Evaluate();
            }
        }

        // void _UpdateSightingInfo(bool hasLOS)
        // {
        //     _sightingInfo.UpdatePos(_currentTarget?.position ?? Vector3.zero, _transform.position, hasLOS, needsLOS);
        //     SetData(targetDisplaceKey, _sightingInfo.KnownDisplacement);
        //     SetData(targetLOSKey, _sightingInfo.HasLineOfSight);
        // }

        /// <summary>
        /// If target can be seen, set it as current target
        /// </summary>
        /// <param name="target"> target to try set </param>
        /// <returns> if target can be seen </returns>
        public bool TrySetTarget(Transform target)
        {
            Vector2 displacement = target.position - _transform.position;
            if (_CanSeePoint(target.position))
            {
                // Set current target
                _currentTarget = target;
                // _sightingInfo.UpdatePos(_currentTarget.position, _transform.position, true);
                return true;
            }
            return false;
        }
        bool _CanSeePoint(Vector3 targetPos)
        {
            if (!needsLOS) return true;

            var ownPos = _transform.position;
            Vector2 displacement = targetPos - ownPos;

            // Detection range depends if target is in FOV
            if (Vector2.Angle(displacement, _character.FacingDirection) < fov)
            {
                if (displacement.magnitude > insideFOVRange) return false;
            }
            else if (displacement.magnitude > outsideFOVRange) return false;

            return !PhysicsSettings.SolidsLinecast(ownPos, targetPos, Mathf.Max(ownPos.z, targetPos.z));
        }

        /// <summary>
        /// Unset currentTarget and set lastSighting as investigated
        /// </summary>
        public void ForgetTarget()
        {
            _currentTarget = null;
            _sightingInfo.Reset();
        }

        public void DrawGizmos()
        {
            if (!Application.isPlaying) return;

            Gizmos.color = Color.blue;

            var pos = _transform.position;

            // outside fov
            Gizmos.DrawWireSphere(pos, outsideFOVRange);

            // inside fov cone
            if (!_character)
            {
                Gizmos.DrawWireSphere(pos, insideFOVRange);
                return;
            }
            Vector2 facing = _character.FacingDirection.normalized;
            facing = facing.sqrMagnitude == 0 ? Vector2.right : facing;
            Gizmos.DrawLine(pos, pos + Quaternion.AngleAxis(fov, Vector3.forward) * facing * insideFOVRange);
            Gizmos.DrawLine(pos, pos + Quaternion.AngleAxis(-fov, Vector3.forward) * facing * insideFOVRange);

            // targeting line
            if (_currentTarget)
            {
                if (_sightingInfo.HasLineOfSight)
                    Gizmos.color = Color.blue;
                else
                    Gizmos.color = Color.red;
                Gizmos.DrawCube(_sightingInfo.KnownPos, Vector3.one);
                Gizmos.DrawLine(pos, _sightingInfo.KnownPos);
            }
        }
    }

    public class LastSeenTimeCondition : Node
    {
        float _waitDuration;
        SightingInfo _target;

        public LastSeenTimeCondition(float waitTime, Node childNode) : base(childNode)
        {
            _waitDuration = waitTime;
        }

        public override void Initalize(ColbyDoan.BehaviourTree.Tree toSet)
        {
            base.Initalize(toSet);
            _target = (SightingInfo)GetData(FindTargetTask.targetInfoKey);
        }

        public override NodeState Evaluate()
        {
            if (Time.time - _target.timeLastSeen > _waitDuration)
            {
                return NodeState.failure;
            }
            return children[0].Evaluate();
        }
    }

    /// <summary>
    /// Data output class for tracked target
    /// </summary>
    [System.Serializable]
    public class SightingInfo// : IDisplacementProvider
    {
        public Vector3 KnownPos { get; private set; }
        public bool HasLineOfSight { get; set; }
        public float timeLastSeen = 0;
        public Vector3 KnownDisplacement { get; private set; }
        public bool investigated = true;
        // public Vector3 Displacement => KnownDisplacement;
        // public bool exists = false;

        // public void UpdatePos(Vector3 selfPosition, bool requireLOS = true)
        // {
        //     if (HasLineOfSight || !requireLOS)
        //     {
        //         KnownPos = transform.position;
        //         timeLastSeen = Time.time;
        //     }
        //     KnownDisplacement = KnownPos - selfPosition;
        // }

        /// <summary>
        /// Update sighting info
        /// </summary>
        /// <param name="targetPos"> Current true position of target </param>
        /// <param name="selfPosition"> Position of observer eyes </param>
        /// <param name="los"> Is there line of sight to target position? </param>
        /// <param name="requireLOS"> Is line of sight needed to for position to be updated? </param>
        public void UpdatePos(Vector3 targetPos, Vector3 selfPosition, bool los, bool requireLOS = true)
        {
            HasLineOfSight = los;
            if (HasLineOfSight || !requireLOS)
            {
                KnownPos = targetPos;
                timeLastSeen = Time.time;
                investigated = false;
            }
            KnownDisplacement = KnownPos - selfPosition;
        }

        public void Reset()
        {
            investigated = true;
            HasLineOfSight = false;
        }
    }

    public interface IDisplacementProvider
    {
        Vector3 Displacement { get; }
    }
}
