// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using ColbyDoan.Attributes;

namespace ColbyDoan
{
    public class FollowCursor : MonoBehaviour
    {
        // [SerializeField] PlayerBehaviour player;

        public Image depthIndicatorSprite;

        public Sprite aboveIndicator;
        public Sprite belowIndicator;

        [ReadOnly] public Vector3 worldPos;

        Transform _root;
        Controls.PlayerActions playerActions;

        void Awake()
        {
            playerActions = new Controls().Player;
            _root = transform.root;
        }
        void OnEnable()
        {
            playerActions.Enable();
        }
        void OnDisable()
        {
            playerActions.Disable();
        }

        void Update()
        {
            // move cursor
            Vector2 screenPos = playerActions.Point.ReadValue<Vector2>();
            transform.position = screenPos;
            // update cursor world pos
            worldPos = GameManager.Instance.mainCamera.ScreenToWorldPoint(screenPos);

            // look for tiles starting from top most
            worldPos.y -= 2;
            for (int h = 2; h >= 0; h--)
            {
                var tile = TileManager.Instance.GetTile(worldPos, h);
                if (tile != null)
                {
                    worldPos.z = h;
                    break;
                }
                worldPos.y += 1;
            }
            // int terrainHeight = TileManager.instance.GetTerrainHeight(worldPos);// + Mathf.Floor(transform.position.z) * Vector3.down);
            // worldPos.z = Mathf.Clamp(terrainHeight - Mathf.FloorToInt(transform.position.z), -1, 1);
            // depthIndicator update
            int depthValue = (int)Mathf.Clamp(worldPos.z - Mathf.FloorToInt(_root.position.z), -1, 1);

            if (depthValue > 0)
            {
                depthIndicatorSprite.enabled = true;
                depthIndicatorSprite.sprite = aboveIndicator;
            }
            else if (playerActions.Sneak.IsPressed())
            {
                depthIndicatorSprite.enabled = true;
                depthIndicatorSprite.sprite = belowIndicator;
            }
            else
            {
                depthIndicatorSprite.enabled = false;
            }
            // return;

            // if (depthValue == 0)
            // {
            //     depthIndicatorSprite.enabled = false;
            //     return;
            // }
            // depthIndicatorSprite.enabled = true;
            // depthIndicatorSprite.sprite = depthValue > 0 ? aboveIndicator : belowIndicator;
        }
    }
}
