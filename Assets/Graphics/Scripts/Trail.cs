using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ColbyDoan
{
    public class Trail : MonoBehaviour
    {
        [SerializeField] LineRenderer lineRenderer;
        public float timeInterval;
        public int trailPoints;

        private void Start()
        {
            lineRenderer.positionCount = trailPoints;
            int i = 0;
            while (i < trailPoints)
            {
                lineRenderer.SetPosition(i, transform.position.GetDepthApparentPosition());
                i++;
            }
            StartCoroutine("TrailCoroutine");
        }

        IEnumerator TrailCoroutine()
        {
            while (true)
            {
                for (int i = trailPoints - 1; i > 0; i--)
                {
                    lineRenderer.SetPosition(i, lineRenderer.GetPosition(i - 1));
                }
                lineRenderer.SetPosition(0, transform.position.GetDepthApparentPosition());

                yield return new WaitForSeconds(timeInterval);
            }
        }
    }
}
