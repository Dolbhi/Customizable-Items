// using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ColbyDoan
{
    using CharacterBase;

    public class StatusUI : MonoBehaviour
    {
        public StatusIndicator statusIndicatorPrefab;

        [SerializeField] StatusEffectsManager statusEffects;

        List<StatusIndicator> icons = new List<StatusIndicator>();

        void Update()
        {
            // add more icons as needed
            while (statusEffects.activeEffects.Count > icons.Count)
            {
                AddIcon();
            }
        }

        public void AddIcon()
        {
            StatusIndicator icon = Instantiate(statusIndicatorPrefab, transform);
            icon.activeEffects = statusEffects.activeEffects;
            icon.index = icons.Count;
            icons.Add(icon);
        }
    }
}
