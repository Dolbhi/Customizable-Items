using UnityEngine;

namespace ColbyDoan
{
    public class Gun : MonoBehaviour
    {
        public Animator animator;
        public Transform nozzel;
        public AudioSource audioSource;
        public AudioSource heavyAudioSource;

        SpriteRenderer spriteRenderer;
        float defaultNozzelY;

        void Awake()
        {
            defaultNozzelY = nozzel.localPosition.y;
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }


        public void PointAt(Vector2 direction)
        {
            // print("direction: " + direction);
            transform.right = direction;
            spriteRenderer.flipY = transform.right.x < 0;
            // flips local y displacement of nozzel as the rifle sprite flips
            nozzel.localPosition = new Vector3(nozzel.localPosition.x, Mathf.Sign(transform.right.x) * defaultNozzelY);
        }

        public void PlayShootSFX(bool lastBullet)
        {
            if (lastBullet)
            {
                heavyAudioSource.Play();
            }
            else
            {
                audioSource.Play();
            }
        }
    }
}