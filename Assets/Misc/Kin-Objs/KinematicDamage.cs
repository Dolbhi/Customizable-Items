using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ColbyDoan.Physics;
using ColbyDoan.Attributes;

namespace ColbyDoan
{
    public class KinematicDamage : MonoBehaviour, IAutoDependancy<KinematicObject>
    {
        [SerializeField] KinematicCollider kCollider;
        KinematicObject _kinematicObj;

        [SerializeField][ReadOnly] DamageInfo damageInfo;

        public float damageMultiplier;

        public KinematicObject Dependancy
        {
            set
            {
                _kinematicObj = value;
            }
        }

        void Awake()
        {
            kCollider.onCollide += _DoDamage;

            Character character;
            Character.instanceFromTransform.TryGetValue(transform, out character);
            damageInfo.source = character;
        }

        /// <summary>
        /// Does damage based on collision impulse
        /// </summary>
        /// <param name="impulse">collision impulse</param>
        void _DoDamage(Vector2 impulse, Transform target)
        {
            float excessMomentum = impulse.magnitude;// - _kinematicObj.mass * speedTreshold;
            if (excessMomentum > 0)
            {
                // print(damageInfo);
                damageInfo.damage = excessMomentum * damageMultiplier;
                damageInfo.knockback.v = impulse;
                damageInfo.ApplyTo(target);
            }
        }

        void OnDestroy()
        {
            kCollider.onCollide -= _DoDamage;
        }
    }
}
