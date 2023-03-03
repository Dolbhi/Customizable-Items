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
            GameStats.OnDataPointsChanged += UpdateText;
        }
        void UpdateText()
        {
            text.text = "Data: " + GameStats.GetXP() + "TB";
        }

        void OnDisable()
        {
            GameStats.OnDataPointsChanged -= UpdateText;
        }
    }
}
