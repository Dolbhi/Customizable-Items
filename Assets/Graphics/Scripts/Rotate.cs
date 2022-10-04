using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ColbyDoan
{
    public class Rotate : MonoBehaviour
    {
        public Vector3 rotateRate;

        // Update is called once per frame
        void Update()
        {
            transform.Rotate(rotateRate * Time.deltaTime);
        }
    }
}
