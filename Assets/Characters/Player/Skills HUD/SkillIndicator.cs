using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ColbyDoan
{
    public class SkillIndicator : MonoBehaviour
    {
        IDisplayableSkill skill;

        [SerializeField] Color readyColor = Color.green;
        [SerializeField] Color cooldownColor = Color.red;

        [SerializeField] TMP_Text label = null;
        [SerializeField] TMP_Text timer = null;
        [SerializeField] TMP_Text stacks = null;
        [SerializeField] Image fill = null;
        [SerializeField] Image icon = null;

        public void SetSkill(IDisplayableSkill toSet)
        {
            bool skillPresent = toSet != null;
            gameObject.SetActive(skillPresent);
            if (!skillPresent) return;
            skill = toSet;
            // icon.sprite = toSet.Icon;
            timer.gameObject.SetActive(toSet.ShowTimer);
        }

        public void SetLabel(string text)
        {
            label.text = text;
        }

        void Update()
        {
            timer.text = skill.CooldownProgress == 1 ? "" : Mathf.CeilToInt(skill.CooldownLeft).ToString();
            fill.fillAmount = 1 - skill.CooldownProgress;
            stacks.text = skill.HasStacks ? skill.Stacks.ToString() : "";
            icon.color = skill.Ready ? readyColor : cooldownColor;
        }
    }

    public interface IDisplayableSkill
    {
        Sprite Icon { get; }

        bool Ready { get; }
        bool ShowTimer { get; }
        float CooldownProgress { get; }
        float CooldownLeft { get; }

        bool HasStacks { get; }
        int Stacks { get; }
    }
}