using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/*
*/

namespace ColbyDoan
{
    [RequireComponent(typeof(TargetTracker))]
    public abstract partial class EnemyBehaviour : MonoBehaviour, IAutoDependancy<Character>, IMovingAgent
    {
        [Header("Dependancies")]
        protected Character character;
        public Character Dependancy { set { character = value; } }
        [SerializeField] public MoveDecider moveDecider;
        [SerializeField] public TargetTracker tracker;
        [SerializeField] public EnemyIndicator indicator;
        // [SerializeField] public ItemDropper itemDropper;

        protected Skill[] Skills => character.skills.skills;

        // [Range(0, 1)] public float itemDropChance = .1f;

        // Movement
        public Vector2 MovingVelocity { get => _targetDirection * _speedMultiplier * _speedAdjustments * character.stats.speed.FinalValue; }// set { MovingDirection = value; speedMultiplier = (stats.speed.FinalValue == 0) ? 0 : Mathf.Clamp01(value.magnitude / stats.speed.FinalValue); } }
        protected Vector2 MovingDirection { get => _targetDirection; set => _targetDirection = value.normalized; }
        Vector2 _targetDirection;
        float _speedMultiplier = 1;
        [SerializeField] protected float moveStopThreshold = 0f;
        [SerializeField] protected float moveMaxThreshold = 1;

        float _speedAdjustments = 1;
        public virtual void UpdateMovement((Vector2, float) option)
        {
            MovingDirection = option.Item1;
            _speedAdjustments = Mathf.InverseLerp(moveStopThreshold, moveMaxThreshold, option.Item2);
        }

        protected virtual void Awake()
        {
            tracker.OnTargetStateChange += UpdateState;

            moveDecider.OnEvaluationFinish += UpdateMovement;

            character.healthManager.OnHurt += (_) => tracker.EnhanceSight();
            character.healthManager.OnDeath += OnDeath;

            SetFacing = FaceMovement;

            StartCoroutine("StateUpdateCoroutine");
        }

        void Start()
        {
            stateMachine.Initialize(InitializeStateMachine(stateMachine));
        }

        protected virtual void Update()
        {
            stateMachine.CurrentState.Update();
            SetFacing();
        }

        #region States

        /// <summary> Call all init methods on states </summary>
        protected abstract IState InitializeStateMachine(StateMachine stateMachine);

        [SerializeField] protected StateMachine stateMachine = new StateMachine();
        protected bool SafeStateChange(IState state)
        {
            if (stateMachine.CurrentState != state)
            {
                stateMachine.ChangeState(state);
                return true;
            }
            return false;
        }

        protected bool enableStateUpdateLoop;
        IEnumerator StateUpdateCoroutine()
        {
            while (true)
            {
                if (enableStateUpdateLoop) UpdateState();
                yield return new WaitForSeconds(.2f);
            }
        }
        public abstract void UpdateState();
        #endregion

        #region Facing
        /// <summary>
        /// Method used to determine the desired facing direction
        /// 3 build in options:
        /// -FaceMovement
        /// -FaceTarget
        /// -FaceCustiom (has a vec2 argument)
        /// </summary>
        protected Action SetFacing = delegate { };
        protected void FaceMovement()
        {
            if (MovingDirection.sqrMagnitude != 0)
            {
                character.FacingDirection = MovingDirection;
            }
        }
        protected void FaceTarget()
        {
            character.FacingDirection = tracker.currentTarget.KnownDisplacement;
        }
        protected void FaceCustom(Vector2 target)
        {
            character.FacingDirection = target - (Vector2)transform.position;
        }
        #endregion

        protected virtual void OnDeath()
        {
            // if (UnityEngine.Random.Range(0f, 1) < itemDropChance)
            //     itemDropper.DropArtifact();

            Destroy(transform.root.gameObject);
        }
        protected virtual void OnDisable()
        {
            stateMachine.CurrentState.Exit();
            StopAllCoroutines();
        }
    }
}