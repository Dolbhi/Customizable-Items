using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ColbyDoan
{
    public class Interactable : MonoBehaviour
    {
        static public List<Interactable> interactables = new List<Interactable>();

        public string hoverText;
        //protected abstract float TextDisplacement { get; }
        public UnityEvent<PlayerBehaviour> OnInteract;

        public void Interact(PlayerBehaviour interacter)
        {
            OnInteract.Invoke(interacter);
        }

        void OnEnable()
        {
            interactables.Add(this);
        }
        void OnDisable()
        {
            interactables.Remove(this);
        }
    }
}