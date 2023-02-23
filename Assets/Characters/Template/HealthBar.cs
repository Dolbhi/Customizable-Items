// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ColbyDoan.CharacterBase
{
    public class HealthBar : MonoBehaviour
    {
        public Image fill;
        public Health healthManager;

        const float lingerDuration = 5;
        float vanishTime;

        void Awake()
        {
            if (healthManager)
            {
                RectTransform rectTransform = transform as RectTransform;
                rectTransform.sizeDelta = new Vector2(1 + Mathf.Log(healthManager.MaxHealth, 100), rectTransform.sizeDelta.y);
                healthManager.OnHurt += (_) => UpdateFill();
                healthManager.OnHeal += (_) => UpdateFill();
            }

            gameObject.SetActive(false);
        }

        void UpdateFill()
        {
            if (healthManager.CurrentHealth <= 0)
            {
                gameObject.SetActive(false);
                return;
            }
            fill.fillAmount = healthManager.FractionFull;
            vanishTime = Time.time + lingerDuration;
            gameObject.SetActive(true);
        }

        void Update()
        {
            if (vanishTime < Time.time)
                gameObject.SetActive(false);
        }
    }
}