using System;
using System.Collections;
// using System.Collections.Generic;
using UnityEngine;
// using UnityEngine.Events;

namespace ColbyDoan
{
    public class SwipeAnimation : MonoBehaviour
    {
        [SerializeField] SpriteRenderer attackSprite;
        public float attackWindupDuration = .5f;
        float attackSpeed = 1;

        public void SetAttackSpeed(float speed)
        {
            attackSpeed = speed;
        }

        public void StartAnimation(Vector2 direction, Action swingCallback)
        {
            StartCoroutine(AttackAnimation(direction, swingCallback));
        }
        public void CancelAnimation()
        {
            StopCoroutine("AttackAnimation");
        }

        IEnumerator AttackAnimation(Vector2 direction, Action swingCallback)
        {
            // show windup
            transform.rotation = Quaternion.FromToRotation(Vector2.right, direction);
            attackSprite.color = new Color(1, 0, 0, .5f);
            yield return new WaitForSeconds(attackWindupDuration / attackSpeed);

            // Trigger swing event
            swingCallback.Invoke();

            // Show slash and fade
            float fadeTime = Time.time + .5f;
            attackSprite.color = Color.white;
            while (Time.time < fadeTime)
            {
                float alpha = (fadeTime - Time.time) / .5f;
                attackSprite.color = new Color(1, 1, 1, alpha);
                yield return null;
            }
            attackSprite.color = new Color(1, 1, 1, 0);
        }
    }
}
