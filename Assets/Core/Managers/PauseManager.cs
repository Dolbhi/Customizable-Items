// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ColbyDoan
{
    public class PauseManager : MonoBehaviour
    {
        static public bool GameIsPaused => _paused;
        static bool _paused = false;

        [SerializeField] GameObject pauseMenu;

        public void Pause(InputAction.CallbackContext ctx)
        {
            if (!_paused)
            {
                Time.timeScale = 0;
            }
            else
            {
                Time.timeScale = 1;
            }
            _paused = !_paused;
            pauseMenu.SetActive(_paused);
        }
    }
}
