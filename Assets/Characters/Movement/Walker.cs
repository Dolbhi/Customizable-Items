using UnityEngine;

namespace ColbyDoan
{
    public class Walker : MonoBehaviour
    {
        IMovingAgent mover;
        [SerializeField] FrictionManager frictionManager = null;

        void Awake()
        {
            mover = GetComponentInChildren<IMovingAgent>();
            if (mover == null) Debug.LogWarning("No moving agent found on " + transform.root.name);
        }

        void Update()
        {
            //if (transform.root.name == "Lumberjack")
            //{
            //    print(character.speedMultiplier);
            //    print(character.stats.speed.FinalValue);
            //    print("v: " + character.MovingVelocity + " d: " + character.MovingDirection);
            //}
            frictionManager.groundSpeedOffset = mover.MovingVelocity;
        }

        private void OnDisable()
        {
            frictionManager.groundSpeedOffset = Vector2.zero;
        }
    }

    public interface IMovingAgent
    {
        Vector2 MovingVelocity { get; }
    }
}