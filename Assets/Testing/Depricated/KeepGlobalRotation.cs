using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ColbyDoan
{
    public class KeepGlobalRotation : MonoBehaviour
    {
        public Quaternion trueGlobalRotation;

        void Update()
        {
            transform.rotation = trueGlobalRotation;
        }
    }
}