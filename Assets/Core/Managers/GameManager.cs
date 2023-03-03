// using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System;
using DG.Tweening;

namespace ColbyDoan
{
    using CharacterBase;

    // GameManager script is set to execute first
    public class GameManager : Singleton<GameManager>
    {
        [Header("Importants")]
        public Camera mainCamera;
        public Character player;

        public ArtifactPools itemPool;

        public event Action OnLevelLoaded = delegate { };

        public int startingData = 0;

        // for level damaging updating pathfinders
        // public Action<Bounds> OnLevelChange = delegate { };

        [Header("HUD objects")]
        public GameObject gameoverHUD; // should manage itself
        public TMP_Text gameOverText;

        protected override void Awake()
        {
            base.Awake();
            // GameStats.Reset();
            DOTween.Init();
            Cursor.SetCursor(null, Vector2.zero, CursorMode.ForceSoftware);
            itemPool = Instantiate<ArtifactPools>(itemPool);
            GameStats.ChangeDataPoints(startingData);
            // DependancyInjector.InjectDependancies(this);
        }

        // have gameoverHUD subscribe to gamemanager instead
        public void TriggerGameOver()
        {
            gameoverHUD.SetActive(true);
        }
        public void ReloadLevel()
        {
            GameStats.Reset();
            DOTween.KillAll();
            gameoverHUD.SetActive(false);
            SceneManager.LoadSceneAsync("Basic Scene");
            OnLevelLoaded.Invoke();
        }
    }

    // public static class Maffs
    // {
    //     /// <summary> Returns a normalised value that decreases at a decreasing rate, with a limit of 0 </summary>
    //     /// <param name="time">Time since max value</param>
    //     /// <param name="rate">Rate at which the factor falls with time</param>
    //     /// <returns></returns>
    //     public static float GetDecayFactor(float time, float rate)
    //     {
    //         return 1 / (time * rate + 1);
    //     }
    // }
}