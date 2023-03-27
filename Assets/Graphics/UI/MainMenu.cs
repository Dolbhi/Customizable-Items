using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace ColbyDoan
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] UIDocument document;
        // VisualElement _root;
        // VisualElement _hiddenPanel;

        // const string HIDDEN_PANEL_NAME = "hiddenPanel";


        void Awake()
        {
            var root = document.rootVisualElement;
            // _hiddenPanel = root.Q<VisualElement>("HiddenPanel");

            // root.Q<Button>("Arena").clicked += _TogglePanelHiding;
        }

        // void _TogglePanelHiding()
        // {
        //     _hiddenPanel.ToggleInClassList(HIDDEN_PANEL_NAME);
        // }
    }
}
