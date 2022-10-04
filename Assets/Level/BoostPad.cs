using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ColbyDoan
{
    public class BoostPad : MonoBehaviour
    {
        public Collider2D trigger;
        public Vector3 targetV;
        public float limit;

        void OnTriggerEnter2D(Collider2D collider)
        {
            //print("contacted");
            KinematicObject kinematicObject;
            KinematicObject.instanceFromTransform.TryGetValue(collider.transform.root, out kinematicObject);
            kinematicObject?.AccelerateTo(targetV, limit);
        }
    }
}