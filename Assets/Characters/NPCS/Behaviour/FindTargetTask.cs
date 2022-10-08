// using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ColbyDoan.BehaviourTree;

namespace ColbyDoan
{
    /// <summary>
    /// Finding and tracking targets task
    /// </summary>
    [System.Serializable]
    public class FindTargetTask : EnemyNode
    {
        TargetInfo _currentTarget = new TargetInfo();
        /// <summary> Targets the target tracker will look for </summary>        
        HashSet<Transform> _targets;

        public float insideFOVRange = 20;
        public float outsideFOVRange = 10;
        [Range(0, 180)]
        public float fov = 75;
        public bool needsLOS = true;

        public const string targetInfoKey = "target_info";
        public const string targetDisplaceKey = "target_displacement";
        public const string targetLOSKey = "target_los";
        public const string targetLOSEnterKey = "target_los_enter";
        public const string targetNewKey = "target_new";

        Character _character;
        Transform _transform;

        public override void Initalize(ColbyDoan.BehaviourTree.Tree toSet)
        {
            base.Initalize(toSet);
            _character = enemyTree.character;
            _transform = _character.transform;
            _targets = RootTracker.GetSet("ally");
            parent.SetData(targetInfoKey, _currentTarget);
        }

        /// <summary>
        /// Updates info for current target, finding a new target if more favourable
        /// If no targets, look for one
        /// </summary>
        /// <returns>success if a target exsists, failure if it doesnt</returns>
        public override NodeState Evaluate()
        {
            // reset data
            parent.SetData(targetLOSEnterKey, false);
            parent.SetData(targetNewKey, false);

            if (_currentTarget.Exists)
            {
                // check for more favourable enemies
                var pos = _transform.position;
                foreach (Transform enemy in _targets)
                {
                    if (enemy == _currentTarget.transform) continue;
                    if (PhysicsSettings.SolidsLinecast(pos, enemy.transform.position)) continue;

                    // New enemy must be many times closer before a switch is considered
                    float currentTargetBias = _currentTarget.HasLineOfSight ? 5 : 2;
                    if (Vector2.Distance(enemy.transform.position, pos) * currentTargetBias > _currentTarget.KnownDisplacement.magnitude) continue;

                    parent.SetData(targetNewKey, TrySetTarget(enemy));
                }

                // update target info
                bool oldLOS = _currentTarget.HasLineOfSight;
                bool newLOS = _CanSeePoint(_currentTarget.transform.position);
                _currentTarget.HasLineOfSight = newLOS;
                _currentTarget.UpdatePos(pos, needsLOS);

                parent.SetData(targetDisplaceKey, _currentTarget.KnownDisplacement);
                parent.SetData(targetLOSKey, newLOS);
                parent.SetData(targetLOSEnterKey, !oldLOS && newLOS);

                state = NodeState.success;
            }
            else
            {
                // search for any valid enemies
                foreach (Transform enemy in _targets)
                {
                    // print(enemy.name + " " + enemy.transform.position.z);
                    if (TrySetTarget(enemy))
                    {
                        parent.SetData(targetDisplaceKey, _currentTarget.KnownDisplacement);
                        parent.SetData(targetLOSKey, _currentTarget.HasLineOfSight);
                        parent.SetData(targetLOSEnterKey, true);
                        parent.SetData(targetNewKey, true);

                        state = NodeState.success;
                        return state;
                    }
                }
                state = NodeState.failure;
            }
            return state;
        }

        public bool TrySetTarget(Transform target)
        {
            Vector2 displacement = target.position - _transform.position;
            if (_CanSeePoint(target.position))
            {
                // Set current target
                _currentTarget.transform = target;
                _currentTarget.HasLineOfSight = true;
                _currentTarget.UpdatePos(_transform.position);
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

        public void ForgetTarget()
        {
            _currentTarget.Reset();
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
            if (_currentTarget.Exists)
            {
                if (_currentTarget.HasLineOfSight)
                    Gizmos.color = Color.blue;
                else
                    Gizmos.color = Color.red;
                Gizmos.DrawCube(_currentTarget.KnownPos, Vector3.one);
                Gizmos.DrawLine(pos, _currentTarget.KnownPos);
            }
        }
    }

    public class LastSeenTimeCondition : Node
    {
        float _waitDuration;
        TargetInfo _target;

        public LastSeenTimeCondition(float waitTime, Node childNode) : base(childNode)
        {
            _waitDuration = waitTime;
        }

        public override void Initalize(ColbyDoan.BehaviourTree.Tree toSet)
        {
            base.Initalize(toSet);
            _target = (TargetInfo)GetData(FindTargetTask.targetInfoKey);
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

    [System.Serializable]
    public class TargetInfo// : IDisplacementProvider
    {
        public Transform transform;
        public Vector3 KnownPos { get; private set; }
        public bool HasLineOfSight { get; set; }
        public float timeLastSeen = 0;
        public Vector3 KnownDisplacement { get; private set; }
        // public Vector3 Displacement => KnownDisplacement;
        public bool Exists => transform;

        public void UpdatePos(Vector3 selfPosition, bool requireLOS = true)
        {
            if (HasLineOfSight || !requireLOS)
            {
                KnownPos = transform.position;
                timeLastSeen = Time.time;
            }
            KnownDisplacement = KnownPos - selfPosition;
        }

        public void Reset()
        {
            transform = null;
        }
    }

    public interface IDisplacementProvider
    {
        Vector3 Displacement { get; }
    }
}
