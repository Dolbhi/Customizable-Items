using UnityEngine;
using System.Collections;

namespace ColbyDoan.FixedTimeLerp
{
    [RequireComponent(typeof(InterpolatedTransformUpdater))]
    public class InterpolatedTransform : MonoBehaviour
    {
        private TransformData[] m_lastTransforms;
        private int m_newTransformIndex;
        private Transform _transform;

        // Collider2D test_coll;

        void Awake()
        {
            _transform = transform;
            // test_coll = GetComponent<BoxCollider2D>();
            // if (name == "Player")
            //     StartCoroutine("LogPos");
        }
        // IEnumerator LogPos()
        // {
        //     while (gameObject)
        //     {
        //         yield return new WaitForFixedUpdate();
        //         print("After physics pos: " + _transform.position.y + " box: " + test_coll.bounds.center.y);
        //     }
        // }

        void OnEnable()
        {
            ForgetPreviousTransforms();
        }

        public void ForgetPreviousTransforms()
        {
            m_lastTransforms = new TransformData[2];
            TransformData t = new TransformData(
                                    _transform.localPosition,
                                    _transform.localRotation,
                                    _transform.localScale);
            m_lastTransforms[0] = t;
            m_lastTransforms[1] = t;
            m_newTransformIndex = 0;
        }

        void FixedUpdate()
        {
            TransformData newestTransform = m_lastTransforms[m_newTransformIndex];
            _transform.SetLocalPositionAndRotation(newestTransform.position, newestTransform.rotation);
            _transform.localScale = newestTransform.scale;
        }

        public void LateFixedUpdate()
        {
            m_newTransformIndex = OldTransformIndex();
            m_lastTransforms[m_newTransformIndex] = new TransformData(
                                                        _transform.localPosition,
                                                        _transform.localRotation,
                                                        _transform.localScale);
        }

        void Update()
        {
            TransformData newestTransform = m_lastTransforms[m_newTransformIndex];
            TransformData olderTransform = m_lastTransforms[OldTransformIndex()];

            _transform.localPosition = Vector3.Lerp(
                                        olderTransform.position,
                                        newestTransform.position,
                                        InterpolationController.InterpolationFactor);
            _transform.localRotation = Quaternion.Slerp(
                                        olderTransform.rotation,
                                        newestTransform.rotation,
                                        InterpolationController.InterpolationFactor);
            _transform.localScale = Vector3.Lerp(
                                        olderTransform.scale,
                                        newestTransform.scale,
                                        InterpolationController.InterpolationFactor);
        }

        private int OldTransformIndex()
        {
            return 1 - m_newTransformIndex;
        }

        [System.Serializable]
        private struct TransformData
        {
            public Vector3 position;
            public Quaternion rotation;
            public Vector3 scale;

            public TransformData(Vector3 position, Quaternion rotation, Vector3 scale)
            {
                this.position = position;
                this.rotation = rotation;
                this.scale = scale;
            }
        }
    }
}