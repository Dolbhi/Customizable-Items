using UnityEngine;
using System;

namespace ColbyDoan
{
    /// <summary>
    /// Stores references to multiple components in a Map
    /// </summary>
    public class MonoReferencer : MonoBehaviour
    {
        public Map<String, MonoBehaviour> components;

        private void Awake()
        {
            components.Refresh();
        }
        private void OnValidate()
        {
            components.Refresh();
        }
    }
}