// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;

// using ColbyDoan.Attributes;

namespace ColbyDoan
{
    public class AxeTracker : MonoBehaviour
    {
        public float pivotHeight = .5f;

        /// <summary> global pos of where the axe originates and where it returns to </summary>
        public Vector3 Pivot => _transform.position + Vector3.forward * pivotHeight;
        public bool HasAxe { get; private set; }

        public const string axeHitID = "on_axe_hit";

        public FacingSpritesLibrary axeSprites;
        public FacingSpritesLibrary noAxeSprites;

        public LumberjackAxe axe;
        public MeleeSkill meleeSkill;
        public CharacterRotater rotater;
        Transform _transform;

        void Awake()
        {
            _transform = transform;
            SetAxe(true);
        }

        /// <summary> set axe presence, update lumber sprites and melee </summary>
        public void SetAxe(bool toSet)
        {
            HasAxe = toSet;
            if (toSet)
            {
                rotater.facingSprites = axeSprites;
                meleeSkill.damageMultiplier = 2.5f;
                meleeSkill.metaTriggerID = axeHitID;
            }
            else
            {
                rotater.facingSprites = noAxeSprites;
                meleeSkill.damageMultiplier = 1f;
                meleeSkill.metaTriggerID = "";
            }
        }
    }
}
