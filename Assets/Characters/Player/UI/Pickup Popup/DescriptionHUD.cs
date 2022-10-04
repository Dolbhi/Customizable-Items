// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ColbyDoan
{
    public class DescriptionHUD : MonoBehaviour
    {
        [SerializeField] Image icon;
        [SerializeField] TMP_Text label;
        [SerializeField] TMP_Text description;
        [SerializeField] Item item;
        [SerializeField] EffectModifier modifier;
        [SerializeField] MaterialLibrary outlines;
        [SerializeField] ColorLibrary colors;

        void OnValidate()
        {
            UpdateItem();
        }

        public void SetDisplay(Item _item, EffectModifier _modifier = EffectModifier.None)
        {
            item = _item;
            modifier = _modifier;
            UpdateItem();
        }

        void UpdateItem()
        {
            icon.sprite = item.image;
            icon.material = outlines[(int)item.rank];

            if (modifier == EffectModifier.Broken)
            {
                label.text = "Broken " + item.name;
                description.text = "Chance to " + item.description;
            }
            else if (modifier == EffectModifier.Bundle)
            {
                label.text = item.name + " x5";
                description.text = item.description;
            }
            else
            {
                label.text = item.name;
                description.text = item.description;
            }
            label.color = colors[(int)item.rank];
        }
    }
}
