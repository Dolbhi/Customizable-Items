// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;

namespace ColbyDoan
{
    using Physics;

    public class LumberjackAxe : MonoBehaviour
    {
        public Transform sprite;
        public KinematicObject kinematicObject;

        public float spinRate = .2f;

        // void Awake()
        // {
        //     kinematicObject.controller.OnSolidCollisionEnter += delegate
        //     {
        //         kinematicObject.enabled = false;
        //         kinematicObject.velocity = Vector2.zero;
        //     };
        // }

        void Update()
        {
            sprite.Rotate(Mathf.PI * 2 * spinRate * kinematicObject.velocity.magnitude * Time.deltaTime * Vector3.forward);
        }

        public void Launch()
        {
            transform.SetParent(null);
            gameObject.SetActive(true);
            kinematicObject.enabled = true;
        }

        public void SetGravity(bool set)
        {
            kinematicObject.GravityMultiplier = set ? 1 : 0;
        }

        // public void StuckSelfInWall(ProjectileHitInfo hitInfo)
        // {
        //     if (!hitInfo.solidHit) return;

        //     transform.Translate(-hitInfo.move.normalized * 0.3f);
        //     kinematicObject.enabled = false;
        //     kinematicObject.velocity = Vector2.zero;
        // }
    }
}
