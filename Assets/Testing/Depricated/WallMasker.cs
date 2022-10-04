using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ColbyDoan
{
    public class WallMasker : MonoBehaviour, IAutoDependancy<KinematicObject>
    {
        public KinematicObject Dependancy { private get; set; }
        public SpriteMask mask;

        //bool visible = false;

        //void OnBecameVisible()
        //{
        //    visible = true;
        //}

        //void OnBecameInvisible()
        //{
        //    visible = false;
        //}

        void OnTriggerEnter2D(Collider2D collision)
        {
            if (transform.position.z > 1)
            {
                mask.enabled = false;
                return;
            }

            //if (!visible) return;
            if (collision.gameObject == TileManager.Instance.DoubleTiles.gameObject)
            {
                mask.enabled = true;
            }
        }

        void OnTriggerExit2D(Collider2D collision)
        {
            if (transform.position.z > 1)
            {
                mask.enabled = false;
                return;
            }

            if (collision.gameObject == TileManager.Instance.DoubleTiles.gameObject)
            {
                mask.enabled = false;
            }
        }
    }
}
