using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ColbyDoan
{
    [CreateAssetMenu(fileName = "Color Library", menuName = "Custom Assets/Create Color Library")]
    public class ColorLibrary : ScriptableObject
    {
        public Color[] colors;

        public Color this[int i]
        {
            get { return colors[i]; }
        }
    }
}
