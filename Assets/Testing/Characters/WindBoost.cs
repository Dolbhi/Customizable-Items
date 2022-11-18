// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;

namespace ColbyDoan
{
    using Physics;

    public class WindBoost : Skill, IWindSource
    {
        public float rechargeRate = .3f;
        public float useRate = 2;
        public float boostForce = 3000;

        public float windSpeed = 500;
        public float coneAngle = 15;

        public float downwardsRadius = 1;

        float _air = 1;
        Vector3 _windDirection = Vector3.one;
        Transform _transform;

        public override void SetUp(Character _character)
        {
            base.SetUp(_character);
            _transform = _character.transform;
        }

        public override void Activate()
        {
            Active = true;
            WindManager.windSources.Add(this);
        }
        public override void Cancel()
        {
            base.Cancel();
            Active = false;
            WindManager.windSources.Remove(this);
        }

        public Vector3 GetWindAtPoint(Vector3 point)
        {
            Vector3 displacement = point - _transform.position;
            float sqrDist = displacement.sqrMagnitude;
            if (Vector2.Angle(_windDirection, displacement) < coneAngle && sqrDist >= 1 && sqrDist < 100)
            {
                // print($"Point: {point}, Speed:{speed}");
                return windSpeed * _windDirection / sqrDist;
            }
            return Vector3.zero;
        }

        void FixedUpdate()
        {
            if (Active)
            {
                if (_air <= 0)
                {
                    // cancel if out of air
                    Cancel();
                }
                else
                {
                    _windDirection = (TargetPos - _transform.position.GetDepthApparentPosition() + Vector3.back * downwardsRadius).normalized;
                    // boost upwards if targetPos is close to character
                    _air -= useRate * Time.fixedDeltaTime;

                    // add falloff from low air
                    _windDirection *= Mathf.InverseLerp(0, .5f, _air);

                    character.kinematicObject.ApplyImpulse(-_windDirection * boostForce * Time.fixedDeltaTime);
                }
            }
            _air += rechargeRate * Time.fixedDeltaTime;
            _air = Mathf.Clamp01(_air);
        }

        void OnGUI()
        {
            // GUI.Label()
            var style = new GUIStyle();
            style.border.top = 1;
            style.border.left = 1;
            // style.alignment = TextAnchor.MiddleLeft;
            GUILayout.Label("Air: " + _air * 100);
        }
    }
}
