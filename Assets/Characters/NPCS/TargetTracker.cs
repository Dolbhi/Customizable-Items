using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace ColbyDoan
{
    public class TargetTracker : MonoBehaviour, IAutoDependancy<Character>, IDisplacementProvider
    {
        public OldTargetInfo currentTarget;
        public Vector3 Displacement => currentTarget.KnownDisplacement; //PREPARE FOR NULL ERROR
        public float searchWaitTime = .2f;
        /// <summary> Targets the target tracker will look for </summary>        
        HashSet<Transform> _targets;

        public float insideFOVRange = 20;
        public float outsideFOVRange = 10;
        [Range(0, 180)]
        public float fov = 75;
        public bool needsLOS = true;
        // public bool FOVactive = false;

        public Character Dependancy { get; set; }

        public event Action OnTargetStateChange = () => { };

        public void ForgetTarget()
        {
            currentTarget.Reset();
            OnTargetStateChange.Invoke();
        }
        public void EnhanceSight()
        {
            StopCoroutine("DoubleRange");
            StartCoroutine("DoubleRange");
        }
        IEnumerator DoubleRange()
        {
            outsideFOVRange *= 2;
            yield return new WaitForSeconds(1);
            outsideFOVRange /= 2;
        }

        protected virtual void Start()
        {
            _targets = RootTracker.GetSet("ally");
            StartCoroutine("SearchForTarget");
        }

        void Update()
        {
            // Return if no target
            if (!currentTarget.exists) return;
            // Update LOS
            bool canSee = CanSeeTarget(currentTarget.transform);
            if (currentTarget.HasLineOfSight != canSee)
            {
                currentTarget.HasLineOfSight = canSee;
                OnTargetStateChange.Invoke();
            }
            // Update pos
            currentTarget.UpdatePos(transform.position, needsLOS);
        }
        protected virtual IEnumerator SearchForTarget()
        {
            yield return null;
            while (true)
            {
                if (!currentTarget.exists)
                {
                    // search for any valid enemies
                    foreach (Transform enemy in _targets)
                    {
                        // print(enemy.name + " " + enemy.transform.position.z);
                        if (TrySetTarget(enemy)) break;
                    }
                }
                else
                {
                    // check for more favourable enemies
                    foreach (Transform enemy in _targets)
                    {
                        if (enemy == currentTarget.transform) continue;
                        if (PhysicsSettings.SolidsLinecast(transform.position, enemy.transform.position)) continue;

                        float currentTargetBias = currentTarget.HasLineOfSight ? 5 : 2;
                        if (Vector2.Distance(enemy.transform.position, transform.position) * currentTargetBias > currentTarget.KnownDisplacement.magnitude) continue;

                        TrySetTarget(enemy);
                    }
                }
                yield return new WaitForSeconds(searchWaitTime);
            }
        }

        public bool TrySetTarget(Transform target)
        {
            Vector2 displacement = target.transform.position - transform.position;
            if (CanSeeTarget(target))
            {
                // Set current target
                currentTarget.transform = target;
                currentTarget.HasLineOfSight = true;
                currentTarget.UpdatePos(transform.position);
                OnTargetStateChange.Invoke();
                return true;
            }
            return false;
        }
        bool CanSeeTarget(Transform target)
        {
            if (!needsLOS) return true;
            Vector2 displacement = target.transform.position - transform.position;

            // Detection range depends if target is in FOV
            if (Vector2.Angle(displacement, Dependancy.FacingDirection) < fov)
            {
                if (displacement.magnitude > insideFOVRange) return false;
            }
            else if (displacement.magnitude > outsideFOVRange) return false;

            return !PhysicsSettings.SolidsLinecast(transform.position, target.transform.position, Mathf.Max(transform.position.z, target.transform.position.z));
        }

        void OnDisable()
        {
            StopCoroutine("SearchForTarget");
        }
        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;

            // outside fov
            Gizmos.DrawWireSphere(transform.position, outsideFOVRange);

            // inside fov cone
            if (!Dependancy)
            {
                Gizmos.DrawWireSphere(transform.position, insideFOVRange);
                return;
            }
            Vector2 facing = Dependancy.FacingDirection.normalized;
            facing = facing.sqrMagnitude == 0 ? Vector2.right : facing;
            Gizmos.DrawLine(transform.position, transform.position + Quaternion.AngleAxis(fov, Vector3.forward) * facing * insideFOVRange);
            Gizmos.DrawLine(transform.position, transform.position + Quaternion.AngleAxis(-fov, Vector3.forward) * facing * insideFOVRange);

            // targeting line
            if (currentTarget.exists)
            {
                if (currentTarget.HasLineOfSight)
                    Gizmos.color = Color.blue;
                else
                    Gizmos.color = Color.red;
                Gizmos.DrawCube(currentTarget.KnownPos, Vector3.one);
                Gizmos.DrawLine(transform.position, currentTarget.KnownPos);
            }
        }
    }

    [System.Serializable]
    public class OldTargetInfo// : IDisplacementProvider
    {
        public Transform transform;
        public Vector3 KnownPos { get; private set; }
        public bool HasLineOfSight { get; set; }
        public float timeLastSeen = 0;
        public Vector3 KnownDisplacement { get; private set; }
        public Vector3 Displacement => KnownDisplacement;
        public bool exists => transform;

        public void UpdatePos(Vector3 selfPosition, bool requireLOS = true)
        {
            if (HasLineOfSight || !requireLOS)
            {
                KnownPos = transform.position;
                timeLastSeen = Time.time;
            }
            KnownDisplacement = KnownPos - selfPosition;
        }

        // public void UpdatePos(Transform target, Vector3 selfPosition, bool requireLOS = true)
        // {
        //     if (HasLineOfSight || !requireLOS)
        //     {
        //         KnownPos = target.position;
        //         timeLastSeen = Time.time;
        //     }
        //     KnownDisplacement = KnownPos - selfPosition;
        // }

        public void Reset()
        {
            transform = null;
        }
    }
}