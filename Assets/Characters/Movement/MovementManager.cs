using UnityEngine;

using ColbyDoan.Attributes;

namespace ColbyDoan
{
    using CharacterBase;

    public class MovementManager : MonoBehaviour, IAutoDependancy<Character>, IMovingAgent
    {
        public Character Dependancy { set { speedStat = value.stats.speed; } }
        CharacterStat speedStat;

        [SerializeField] protected float moveStopThreshold = 0f;
        [SerializeField] protected float moveMaxThreshold = 1;

        /// <summary> Output target velocity </summary>
        public Vector2 MovingVelocity { get => _targetDirection * speedMultiplier * _speedAdjustments * speedStat.FinalValue; }// set { MovingDirection = value; speedMultiplier = (stats.speed.FinalValue == 0) ? 0 : Mathf.Clamp01(value.magnitude / stats.speed.FinalValue); } }
        /// <summary> Input speed multiplier </summary>
        public float speedMultiplier = 1;
        [ReadOnly][SerializeField] Vector2 _targetDirection;
        [ReadOnly][SerializeField] float _speedAdjustments = 1;

        public virtual void UpdateMovement(Vector2 direction, float score)
        {
            _targetDirection = direction.normalized;
            _speedAdjustments = Mathf.InverseLerp(moveStopThreshold, moveMaxThreshold, score);
        }
    }
}
