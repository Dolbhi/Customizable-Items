// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;

namespace ColbyDoan
{
    using Physics;

    public class WindBoost : CooldownSkill, IWindSource
    {
        // public float rechargeRate = .3f;
        public float boostDuration = .5f;
        public float boostSpdMultiplier = 500;

        public float cooldown = 4;

        public float windSpdMultiplier = 100;
        public float coneAngle = 15;

        public float downwardsRadius = 1;

        float _boostTimeLeft = 0;
        /// <summary>
        /// Direction of wind with magnitude equal to speed stat * falloff
        /// </summary>
        Vector3 _windDirectionPower = Vector3.one;
        Transform _transform;

        public override void SetUp(Character _character)
        {
            base.SetUp(_character);
            _transform = _character.transform;
        }

        public override void Activate()
        {
            Active = true;
            _boostTimeLeft = boostDuration;
            cooldownHandler.StartCooldown(cooldown);
            WindManager.windSources.Add(this);
        }

        public Vector3 GetWindAtPoint(Vector3 point)
        {
            Vector3 displacement = point - _transform.position;
            float sqrDist = displacement.sqrMagnitude;
            if (Vector2.Angle(_windDirectionPower, displacement) < coneAngle && sqrDist >= 1 && sqrDist < 100)
            {
                // print($"Point: {point}, Speed:{speed}");
                return windSpdMultiplier * _windDirectionPower / sqrDist;
            }
            return Vector3.zero;
        }

        void FixedUpdate()
        {
            if (Active)
            {
                if (_boostTimeLeft <= 0)
                {
                    // stop if time is up
                    Active = false;
                    WindManager.windSources.Remove(this);
                }
                else
                {
                    // Wind direction with speed stat as multiplier
                    // boost upwards if targetPos is close to character
                    _windDirectionPower = Stats.speed.FinalValue * (TargetPos - _transform.position.GetDepthApparentPosition()).normalized;
                    _boostTimeLeft -= Time.fixedDeltaTime;

                    // add falloff from low air
                    // _windDirectionPower *= Mathf.InverseLerp(0, .5f, _boostTimeLeft);

                    character.kinematicObject.ApplyImpulse(-_windDirectionPower * boostSpdMultiplier * Time.fixedDeltaTime);
                }
            }
        }

        // void OnGUI()
        // {
        //     // GUI.Label()
        //     var style = new GUIStyle();
        //     style.border.top = 1;
        //     style.border.left = 1;
        //     // style.alignment = TextAnchor.MiddleLeft;
        //     GUILayout.Label("Air: " + _boostTimeLeft * 100);
        // }
    }
}
