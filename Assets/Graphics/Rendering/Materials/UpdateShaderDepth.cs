// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;

namespace ColbyDoan
{
    public class UpdateShaderDepth : MonoBehaviour
    {
        [SerializeField] Material[] materials;

        Transform _transform;
        void Awake()
        {
            _transform = transform;
        }

        void Update()
        {
            int l = materials.Length;
            for (int i = 0; i < l; i++)
            {
                materials[i].SetFloat("_TargetDepth", _transform.position.z);
            }
        }
    }
}
