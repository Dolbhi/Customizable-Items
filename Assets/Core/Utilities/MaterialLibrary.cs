using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ColbyDoan
{
    [CreateAssetMenu(fileName = "Material Library", menuName = "Custom Assets/Create Material Library")]
    public class MaterialLibrary : ScriptableObject
    {
        public Material[] materials;

        public Material this[int i]
        {
            get { return materials[i]; }
        }
    }
}
