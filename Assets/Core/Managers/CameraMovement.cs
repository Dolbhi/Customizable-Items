using UnityEngine;
using UnityEngine.InputSystem;

namespace ColbyDoan
{
    public class CameraMovement : MonoBehaviour
    {
        public new Camera camera;

        public Transform follow;
        [Range(0, 1)]
        public float mouseFollow;
        public float dampTime;
        public Rect boundary;
        public bool useBoundary;
        [SerializeField]
        Rect centerBounds;

        Vector3 velocity;

        float height;

        protected void Awake()
        {
            centerBounds = boundary;
            float unitsPerPixel = 2 * Camera.main.orthographicSize / Camera.main.pixelHeight;
            centerBounds.height -= 2 * Camera.main.orthographicSize;
            centerBounds.width -= unitsPerPixel * Camera.main.pixelWidth;
            centerBounds.center = boundary.center;

            height = transform.position.z;
        }

        void Update()
        {
            if (follow)
            {
                Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                Vector3 targetPos = Vector2.Lerp(follow.position, mousePosition, mouseFollow);
                if (useBoundary)
                {
                    targetPos.x = Mathf.Clamp(targetPos.x, centerBounds.xMin, centerBounds.xMax);
                    targetPos.y = Mathf.Clamp(targetPos.y, centerBounds.yMin, centerBounds.yMax);
                }
                targetPos.z = height;
                transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, dampTime);
            }
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireCube(boundary.center, boundary.size);
            if (Application.isPlaying)
                Gizmos.DrawWireCube(centerBounds.center, centerBounds.size);
        }
    }
}