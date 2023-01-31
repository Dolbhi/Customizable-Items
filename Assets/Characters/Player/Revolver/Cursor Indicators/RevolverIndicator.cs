// using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ColbyDoan
{
    public class RevolverIndicator : MonoBehaviour
    {
        public float indicatorRadius;

        public Color blueBulletColor;
        public RevolverShootSkill revolverSkill;
        public Image indicatorPrefab;

        List<Image> _bulletIndicators = new List<Image>();
        Transform _transform;

        UnityAction<int, int> _UpdateEvent;

        void Awake()
        {
            _transform = transform;
            _ArrangeIndicators(revolverSkill.cylinderCount);
            _UpdateEvent += UpdateIndicators;

            revolverSkill.OnBulletCountChange.AddListener(_UpdateEvent);
        }

        void UpdateIndicators(int loadedBullets, int zoinked)
        {
            for (int i = 0; i < revolverSkill.cylinderCount; i++)
            {
                Image indi = _bulletIndicators[i];
                if (i < loadedBullets)
                {
                    indi.enabled = true;
                    if (i < zoinked)
                    {
                        indi.color = blueBulletColor;
                    }
                    else
                    {
                        indi.color = Color.white;
                    }
                }
                else
                {
                    indi.enabled = false;
                }
            }
        }

        void _ArrangeIndicators(int cylinderCount)
        {
            float angle = 360 / cylinderCount;
            Quaternion rotation = Quaternion.Euler(0, 0, angle);
            Vector2 pos = Vector2.right * indicatorRadius;

            // place indicators in position
            for (int i = 0; i < cylinderCount; i++)
            {
                var indi = _GetIndicator(i);
                indi.transform.localPosition = pos;
                indi.enabled = true;
                pos = rotation * pos;
            }

            // disable extra indicators
            for (int i = cylinderCount; i < _bulletIndicators.Count; i++)
            {
                _bulletIndicators[i].enabled = false;
            }
        }

        Image _GetIndicator(int index)
        {
            while (index >= _bulletIndicators.Count)
            {
                var indi = Instantiate(indicatorPrefab, _transform);
                // indi.transform.SetParent(_transform, true);
                _bulletIndicators.Add(indi);
            }
            return _bulletIndicators[index];
        }
    }
}
