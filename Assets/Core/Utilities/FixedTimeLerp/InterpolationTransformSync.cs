// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;

namespace ColbyDoan.FixedTimeLoop
{
    /// <summary>
    /// Syncs transfrom and colliders after true positions are restored in InterpolatedTransform
    /// </summary>
    public class InterpolationTransformSync : MonoBehaviour
    {
        void FixedUpdate()
        {
            Physics2D.SyncTransforms();
        }
    }
}
