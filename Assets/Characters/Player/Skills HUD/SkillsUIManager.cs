using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ColbyDoan
{
    using CharacterBase;
    public class SkillsUIManager : MonoBehaviour
    {
        public SkillIndicator indicatorProp;
        // public Transform divider;
        public SkillsManager playerSkills;

        public string[] mainSkillControls = new string[] { "M1", "M2", "Shift", "R" };

        [SerializeField] List<SkillIndicator> skillIndicators;

        void Awake()
        {
            playerSkills.OnSkillsChanged += UpdateSkills;

            // skillIndicators = new List<SkillIndicator>(4);

            // Add class skill indicators
            // int i = 0;
            // while (i < 4)
            // {
            //     AddIndicator(null);
            //     i++;
            // }

            // move divider behind class skills indicators
            // divider.SetAsLastSibling();

            // startup update
        }

        void Start()
        {
            UpdateSkills();
        }

        public void UpdateSkills()
        {
            // look at total skills count and updates accordingly
            var skillCount = playerSkills.skills.Length;
            for (int i = 0; i < skillCount; i++)
            {
                if (skillIndicators.Count <= i)
                    AddIndicator(playerSkills.skills[i] as IDisplayableSkill);
                else
                {
                    SkillIndicator currentIndicator = skillIndicators[i];
                    currentIndicator.SetSkill(playerSkills.skills[i] as IDisplayableSkill);
                }
            }
            // disables excess indicators
            if (skillIndicators.Count > skillCount)
            {
                for (int i = skillCount; i < skillIndicators.Count; i++)
                {
                    skillIndicators[i].SetSkill(null);
                }
            }
        }

        void AddIndicator(IDisplayableSkill skill)
        {
            if (!this) return;
            SkillIndicator skillIndicator = Instantiate(indicatorProp, transform);
            skillIndicator.SetSkill(skill);

            skillIndicators.Add(skillIndicator);

            int count = skillIndicators.Count;
            if (count <= mainSkillControls.Length)
            {
                skillIndicator.SetLabel(mainSkillControls[count - 1]);
            }
            else
            {
                skillIndicator.SetLabel((count - mainSkillControls.Length + 1).ToString());
            }
        }
    }
}