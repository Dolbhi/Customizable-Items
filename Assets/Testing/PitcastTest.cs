using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ColbyDoan
{
    public class PitcastTest : MonoBehaviour
    {
        Vector2[] testDirections;

        void Awake()
        {
            testDirections = new Vector2[10];
            float angleIncrement = 2 * Mathf.PI / 10;
            for (int i = 0; i < 10; i++)
            {
                float currentAngle = i * angleIncrement;
                testDirections[i] = new Vector2(Mathf.Cos(currentAngle), Mathf.Sin(currentAngle));
            }
        }

        void OnDrawGizmos()
        {
            if (Application.isPlaying)
            {
                foreach (Vector2 dir in testDirections)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawRay(transform.position, dir * TileManager.Instance.PitRaycast(transform.position, dir, 3));
                }
            }
        }
    }
}
