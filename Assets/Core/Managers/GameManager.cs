using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System;

namespace ColbyDoan
{
    // GameManager script is set to execute first
    public class GameManager : Singleton<GameManager>
    {
        [Header("Importants")]
        public Camera mainCamera;
        public Character player;

        public ArtifactPools itemPool;

        public event Action OnLevelLoaded = delegate { };

        // for level damaging updating pathfinders
        // public Action<Bounds> OnLevelChange = delegate { };

        [Header("HUD objects")]
        public GameObject gameoverHUD; // should manage itself
        public TMP_Text gameOverText;

        protected override void Awake()
        {
            base.Awake();
            DG.Tweening.DOTween.Init();
            Cursor.SetCursor(null, Vector2.zero, CursorMode.ForceSoftware);
            itemPool = Instantiate<ArtifactPools>(itemPool);
            // DependancyInjector.InjectDependancies(this);
        }

        // have gameoverHUD subscribe to gamemanager instead
        public void TriggerGameOver()
        {
            gameoverHUD.SetActive(true);
        }
        public void ReloadLevel()
        {
            gameoverHUD.SetActive(false);
            SceneManager.LoadSceneAsync("Basic Scene");
            OnLevelLoaded.Invoke();
        }
    }

    /// <summary> Misc. extensions </summary>
    public static class Extensions
    {
        // vectors
        static public Vector3 GetUndisplacedPosition(this Vector3 position)
        {
            return position + position.z * Vector3.down * PhysicsSettings.depthToHeightMultiplier;
        }
        /// <summary> Adds the z component (multiplied by depth to height ratio) to the y component </summary>
        static public Vector3 GetDepthApparentPosition(this Vector3 position)
        {
            return position + position.z * Vector3.up * PhysicsSettings.depthToHeightMultiplier;
        }
        static public Vector3Int GetXYOnly(this Vector3Int input)
        {
            return new Vector3Int(input.x, input.y, 0);
        }
        static public Vector3 ConvertToV3(this Vector2 vector, float z = 0)
        {
            return new Vector3(vector.x, vector.y, z);
        }

        static public string GetPrefix(this EffectModifier modifier)
        {
            return modifier switch
            {
                EffectModifier.Broken => "broken_",
                EffectModifier.Bundle => "bundle_",
                _ => ""
            };
        }

        // components
        static public void SetZPosition(this Transform transform, float z)
        {
            Vector3 old = transform.position;
            old.z = z;
            transform.position = old;
        }
        static public Vector2 Get2DPos(this Transform transform)
        {
            return transform.position;
        }
        static public void SetSpriteAlpha(this SpriteRenderer spriteRenderer, float alpha)
        {
            Color old = spriteRenderer.color;
            old.a = alpha;
            spriteRenderer.color = old;
        }
        static public void SetSpriteAlpha(this UnityEngine.UI.Image image, float alpha)
        {
            Color old = image.color;
            old.a = alpha;
            image.color = old;
        }

        static public int Exclude(this LayerMask mask, int layer)
        {
            //Debug.Log(Convert.ToString(~(1 << layer), 2));
            return mask & ~(1 << layer);
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