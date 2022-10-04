using UnityEngine;

namespace ColbyDoan
{
    public class Flyer : MonoBehaviour
    {
        [SerializeField] float propulsion = 20;
        IMovingAgent mover;
        [SerializeField] KinematicObject kinematicObject;

        void Awake()
        {
            mover = GetComponentInChildren<IMovingAgent>();
            if (mover == null) Debug.LogWarning("No moving agent found on " + transform.root.name);
            kinematicObject = GetComponentInParent<KinematicObject>();
            if (kinematicObject == null) Debug.LogWarning("No kinematic object found on " + transform.root.name);
        }

        // Update is called once per frame
        void Update()
        {
            // Debug.Log($"Moving direction: {character.MovingVelocity}, Multiplier: {character.speedMultiplier}, Final: {(Vector3)character.MovingVelocity + kinematicObject.velocity.z * Vector3.forward}");
            kinematicObject.ForceTo((Vector3)mover.MovingVelocity + kinematicObject.velocity.z * Vector3.forward, propulsion * Time.deltaTime);
        }
    }
}