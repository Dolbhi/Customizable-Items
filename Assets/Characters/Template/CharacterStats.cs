using System.Collections.Generic;
using UnityEngine;
using System;

namespace ColbyDoan
{
    public class CharacterStats : MonoBehaviour
    {
        public CharacterStat maxHealth = new CharacterStat(50);
        public CharacterStat armor = new CharacterStat(0);
        public CharacterStat speed = new CharacterStat(6);
        public CharacterStat attack = new CharacterStat(10);
        public CharacterStat attackSpeed = new CharacterStat(1);
        public CharacterStat critChance = new CharacterStat(.05f);
    }

    [Serializable]
    public class CharacterStat
    {
        public CharacterStat(float value)
        {
            baseValue = value;
        }

        public float FinalValue => baseValue * finalMultiplier;
        public float BaseValue
        {
            get
            {
                return baseValue;
            }
            set
            {
                baseValue = value;
                OnStatChanged?.Invoke(FinalValue);
            }
        }
        [SerializeField] float baseValue;

        float finalMultiplier = 1;
        List<float> multipliers = new List<float>();

        public event Action<float> OnStatChanged;
        // void ForceUpdate()
        // {
        //     OnStatChanged.Invoke(FinalValue);
        // }

        public void AddMultiplier(float multiplier)
        {
            multipliers.Add(multiplier);
            finalMultiplier *= multiplier;
            OnStatChanged?.Invoke(FinalValue);
        }
        public void RemoveMultiplier(float multiplier)
        {
            multipliers.Remove(multiplier);
            UpdateFinalMultiplier();
            OnStatChanged?.Invoke(FinalValue);
        }
        public void UpdateFinalMultiplier()
        {
            finalMultiplier = 1;
            foreach (float mul in multipliers)
            {
                finalMultiplier *= mul;
            }
        }
    }
}