using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ColbyDoan
{
    public class AnimationEventManager : MonoBehaviour
    {
        public UnityEvent[] Events;

        public void InvokeEvent(int eventIndex)
        {
            Events[eventIndex].Invoke();
        }
    }
}