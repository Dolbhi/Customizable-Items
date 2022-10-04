using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ColbyDoan
{
    public class EventTimer : MonoBehaviour
    {
        public float duration;
        public UnityEvent action;
        private void Start()
        {
            StartCoroutine(TriggerEvent());
        }

        IEnumerator TriggerEvent()
        {
            yield return new WaitForSeconds(duration);
            action.Invoke();
        }
    }
}
