using System;
using UnityEngine;
using TMPro;

namespace ColbyDoan
{
    using ColbyDoan.Physics;

    public class PopupSpawner : MonoBehaviour
    {
        public static Action<Vector3, string> OnSpawnPopup;
        public static Color popupColor = Color.white;

        public TMP_Text popupProp;

        private void Awake()
        {
            OnSpawnPopup = SpawnPopup;
        }

        void SpawnPopup(Vector3 position, string text)
        {
            TMP_Text popup = Instantiate(popupProp, position.GetDepthApparentPosition(), Quaternion.identity, transform);
            popup.text = text;
            popup.color = popupColor;
        }

        // private void OnDisable()
        // {
        //     OnSpawnPopup -= SpawnPopup;
        // }
    }
}