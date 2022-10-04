using System.Collections;
using System;
using UnityEngine;
using TMPro;

namespace ColbyDoan
{
    public class EnemyIndicator : MonoBehaviour
    {
        public TMP_Text indicator;

        public void SetIndicator(string indication, float duration = .5f)
        {
            StopAllCoroutines();
            StartCoroutine(TemporaryIndication(indication, duration));
        }
        IEnumerator TemporaryIndication(string indication, float duration)
        {
            indicator.text = indication;
            yield return new WaitForSeconds(duration);
            indicator.text = "";
        }

        public void PlayIndicatorAnimation(Action sequenceEndCallback = null, params (string, float)[] seqence)
        {
            StopAllCoroutines();
            StartCoroutine(IndicatorAnimation(sequenceEndCallback, seqence));
        }
        IEnumerator IndicatorAnimation(Action sequenceEndCallback, params (string, float)[] seqence)
        {
            for (int i = 0; i < seqence.Length; i++)
            {
                indicator.text = seqence[i].Item1;
                yield return new WaitForSeconds(seqence[i].Item2);
            }
            indicator.text = "";
            sequenceEndCallback?.Invoke();
        }
    }

    public class IndicatorAnimation : ScriptableObject
    {
        public (string, float)[] TextSequence;
    }
}