// using System.Collections;
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
        [SerializeField] UnityEvent<PlayerBehaviour> OnInteract;
        [SerializeField] UnityEvent OnApproach;
        [SerializeField] UnityEvent OnLeave;

        public void Interact(PlayerBehaviour interacter)
        {
            OnInteract.Invoke(interacter);
        }
        public void OnPlayerApproach()
        {
            OnApproach.Invoke();
            // Debug.Log("approach", this);
        }
        public void OnPlayerLeave()
        {
            OnLeave.Invoke();
            // Debug.Log("Leave", this);
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