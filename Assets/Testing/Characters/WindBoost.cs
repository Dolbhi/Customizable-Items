// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;

namespace ColbyDoan
{
    using CharacterBase;
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
            float dist = displacement.magnitude;
            if (Vector2.Angle(_windDirectionPower, displacement) < coneAngle && dist >= 1 && dist < 10)
            {
                // print($"Point: {point}, Speed:{speed}");
                return windSpdMultiplier * _windDirectionPower / dist;
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

                    character.kinematicObject.ApplyImpulse(-_windDirectionPower * boostSpdMultiplier * Time.fixedDeltaTime * Mathf.InverseLerp(0, .5f * boostDuration, _boostTimeLeft));
                }
            }
        }

        void OnDisable()
        {
            WindManager.windSources.Remove(this);
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
