using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ColbyDoan
{
    public class DisplaceWithDepth : MonoBehaviour
    {
        public const float displacementMultiplier = 1;

        public Vector2 trueLocalPosition;
        public bool disableDisplacement;
        public bool onlyUseParentZCoord;

        //public Vector3 TrueGlobalPos { get; private set; }

        private void Awake()
        {
            LateUpdate();
        }

        public void LateUpdate()
        {
            UpdateTransform(onlyUseParentZCoord ? transform.parent.localPosition.z : transform.position.z);
        }

        void UpdateTransform(float zPos)
        {
            transform.localPosition = trueLocalPosition;
            transform.SetZPosition(zPos);
            if (disableDisplacement) return;
            //TrueGlobalPos = transform.position;
            transform.position += zPos * displacementMultiplier * Vector3.up;
        }

        void OnValidate()
        {
            LateUpdate();
        }

        // static public Vector2 UndisplacePosition(Vector3 position)
        // {
        //     return position + position.z * Vector3.up * displacementMultiplier;
        // }
        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            GizmosUtility.DrawCross(transform.parent.position, .5f);
            Gizmos.color = Color.blue;
            GizmosUtility.DrawCross(transform.parent.position + (Vector3)trueLocalPosition, .5f);
        }
    }
}