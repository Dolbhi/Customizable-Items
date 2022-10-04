using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace ColbyDoan
{
    public class Bob : MonoBehaviour
    {
        [SerializeField] float period;
        [SerializeField] float amplitude;
        float defaultY;
        bool defaultYSet;

        // void Start()
        // {
        //     defaultY = transform.localPosition.y;
        // }

        void SetDefaultY()
        {
            defaultY = transform.localPosition.y;
            defaultYSet = true;
        }

        Sequence bob;
        void StartBob()
        {
            if (!defaultYSet) SetDefaultY();

            transform.Translate(amplitude * Vector3.down, Space.Self);
            bob = DOTween.Sequence();
            bob.Append(transform.DOLocalMoveY(defaultY + amplitude, period * .5f).SetEase(Ease.InOutSine));
            bob.Append(transform.DOLocalMoveY(defaultY - amplitude, period * .5f).SetEase(Ease.InOutSine));
            bob.SetLoops(-1);
            bob.Play();
        }

        void OnDisable()
        {
            // bob.Complete();
            transform.DOKill();
        }

        void OnEnable()
        {
            StartBob();
        }
    }
}
