// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ColbyDoan
{
    public class PlayerHealthBar : MonoBehaviour
    {
        public Image fill;
        public TMP_Text text;
        public Health playerHealth;

        void Update()
        {
            fill.fillAmount = playerHealth.FractionFull;
            text.text = Mathf.FloorToInt(playerHealth.CurrentHealth) + "/" + Mathf.FloorToInt(playerHealth.MaxHealth);
        }
    }
}