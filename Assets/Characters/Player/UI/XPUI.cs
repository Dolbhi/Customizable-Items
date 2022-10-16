using UnityEngine;
using TMPro;

namespace ColbyDoan
{
    public class XPUI : MonoBehaviour
    {
        [SerializeField] TMP_Text text;

        // Start is called before the first frame update
        void Start()
        {
            UpdateText();
            GameStats.OnXPChanged += UpdateText;
        }
        void UpdateText()
        {
            text.text = "XP: " + GameStats.GetXP();
        }

        void OnDisable()
        {
            GameStats.OnXPChanged -= UpdateText;
        }
    }
}
