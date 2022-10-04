using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ColbyDoan
{
    public static class GizmosUtility
    {
        public static void DrawCross(Vector3 pos, float size = 1)
        {
            float radius = size / 2;
            Gizmos.DrawLine(pos + Vector3.up * radius, pos + Vector3.down * radius);
            Gizmos.DrawLine(pos + Vector3.right * radius, pos + Vector3.left * radius);
        }
    }
}
