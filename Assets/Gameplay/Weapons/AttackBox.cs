using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ColbyDoan
{
    using Physics;

    public class AttackBox : MonoBehaviour
    {
        [SerializeField] Collider2D coll;

        public LayerMask attackMask;
        public float above;
        public float below;

        public bool needLOS = true;
        public Transform sightOrigin;

        List<Collider2D> hitResults = new List<Collider2D>();

        public void Attack(DamageInfo damage)
        {
            Vector3 pos = transform.position;
            coll.OverlapCollider(PhysicsSettings.GetFilter(attackMask, pos, below, above), hitResults);
            foreach (Collider2D hit in hitResults)
            {
                //print("attacking");
                if (!PhysicsSettings.SolidsLinecast(sightOrigin.position, hit.transform.position))
                    damage.ApplyTo(hit.transform);
            }
        }
    }
}
