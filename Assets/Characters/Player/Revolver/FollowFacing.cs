// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;

namespace ColbyDoan
{
    public class FollowFacing : MonoBehaviour
    {
        [SerializeField] CharacterRotater rotater;
        [SerializeField] Character character;
        [SerializeField] new SpriteRenderer renderer;

        const float ANGLE_INCREMENT = Mathf.PI / 4;
        Vector3 _defaultPos;
        Transform _transform;

        public float maxDisplacement;

        void Awake()
        {
            _transform = transform;
            _defaultPos = transform.localPosition;
        }

        void Update()
        {
            int facingNo = (int)rotater.facing;
            // reduce sorting order when back facing
            renderer.sortingOrder = facingNo < 4 ? -1 : 0;
            // Shift local pos left and right according to lean if moving
            if (character.Velocity.sqrMagnitude > float.Epsilon)
            {
                float cos = Mathf.Cos(facingNo * ANGLE_INCREMENT + ANGLE_INCREMENT / 2);
                cos *= maxDisplacement;
                if (rotater.movingBackwards) cos *= -1;
                _transform.localPosition = _defaultPos + Vector3.right * cos;
            }
            else
            {
                _transform.localPosition = _defaultPos;
            }
        }
    }
}
