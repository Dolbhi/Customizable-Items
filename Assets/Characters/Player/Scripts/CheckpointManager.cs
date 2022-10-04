using System.Collections;
using UnityEngine;

namespace ColbyDoan
{
    public class CheckpointManager : MonoBehaviour, IAutoDependancy<Character>
    {
        public Character Dependancy
        {
            set
            {
                kinematicObject = value.kinematicObject;
                kinematicObject.OnPitted += OnPitted;
                health = value.healthManager;
            }
        }

        KinematicObject kinematicObject;
        Health health;

        Vector3 lastSafePoint;
        readonly float updatePeriod = 1;

        void Awake()
        {
            lastSafePoint = .1f * Vector3.forward;
            StartCoroutine("UpdateSafePoint");
        }

        public IEnumerator UpdateSafePoint()
        {
            while (true)
            {
                yield return new WaitForSeconds(updatePeriod);
                if (kinematicObject.controller.collisions.grounded)
                {
                    lastSafePoint = transform.position + 1 * Vector3.forward;
                }
            }
        }

        void OnPitted()
        {
            health.Damage(30);
            kinematicObject.velocity = Vector3.zero;
            ReturnToSafePoint();
        }

        public void ReturnToSafePoint()
        {
            kinematicObject.Teleport(lastSafePoint);
        }
    }
}