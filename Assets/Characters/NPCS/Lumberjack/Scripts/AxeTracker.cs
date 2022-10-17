using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// using ColbyDoan.Attributes;

namespace ColbyDoan
{
    public class AxeTracker : MonoBehaviour
    {
        /// <summary> global pos of where the axe originates and where it returns to </summary>
        public Vector3 Pivot => _transform.position + Vector3.forward;

        public bool HasAxe { get; private set; }

        public LumberjackAxe axe;
        public MeleeSkill meleeSkill;
        Transform _transform;

        void Awake()
        {
            _transform = transform;
            SetAxe(true);
        }

        /// <summary> set axe presence </summary>
        public void SetAxe(bool toSet)
        {
            HasAxe = toSet;
            if (toSet)
            {
                // spriteLibrary.spriteLibraryAsset = axeAsset;
                meleeSkill.damageMultiplier = 2.5f;
                meleeSkill.metaTriggerID = "axeHitID";
            }
            else
            {
                // spriteLibrary.spriteLibraryAsset = noAxeAsset;
                meleeSkill.damageMultiplier = 1f;
                meleeSkill.metaTriggerID = "";
            }
        }
    }
}
