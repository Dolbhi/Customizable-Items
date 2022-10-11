﻿using System;
using System.Collections;
using UnityEngine;

namespace ColbyDoan
{
    public class SkillsManager : MonoBehaviour, IAutoDependancy<Character>
    {
        public Skill[] skills = new Skill[0];
        public Character Dependancy { set => SetUp(value); }
        Character _character;

        public event Action OnSkillsChanged;

        void SetUp(Character value)
        {
            _character = value;
            foreach (Skill skill in skills)
            {
                skill.SetUp(_character);
            }
        }
        void OnEnable()
        {
            // Debug.Log("On enabled called", this);
            foreach (Skill skill in skills)
            {
                skill.enabled = true;
            }
        }
        void OnDisable()
        {
            foreach (Skill skill in skills)
            {
                skill.Cancel();
                skill.enabled = false;
            }
        }
        void Start()
        {
            OnSkillsChanged?.Invoke();
        }
        void OnValidate()
        {
            if (enabled) return;
            foreach (Skill skill in skills)
            {
                skill.enabled = false;
            }
        }
    }
    public abstract class Skill : MonoBehaviour, IDisplayableSkill
    {
        [SerializeField] Sprite icon;

        public virtual bool Ready => enabled;
        public virtual Vector3 TargetPos { get; set; }
        protected Character character;
        protected CharacterStats Stats => character.stats;

        // IDisplayableSkill Implementation
        public virtual Sprite Icon => icon;
        public virtual bool ShowTimer => false;
        public virtual float CooldownProgress => 1;
        public virtual float CooldownLeft => 0;

        // limbo of maybe visual only or maybe concrete system (currently only visual)
        public virtual int Stacks { get; set; }
        public virtual bool HasStacks { get; set; }

        public virtual void SetUp(Character _character)
        {
            character = _character;
        }

        public abstract void Activate();
        public virtual void Cancel() { }

        public virtual void ResetCooldown() { }
        public virtual void ReduceCooldown(float reduction) { }
    }

    public abstract class CooldownSkill : Skill
    {
        public override bool Ready => base.Ready && cooldownHandler.Ready;

        public override bool ShowTimer => true;
        public override float CooldownProgress => cooldownHandler.ProgressFraction;
        public override float CooldownLeft => cooldownHandler.TimeLeft;

        public SkillCooldownHandler cooldownHandler;

        public override void SetUp(Character _character)
        {
            base.SetUp(_character);
            cooldownHandler = new SkillCooldownHandler(this);
        }
        public override void ReduceCooldown(float reduction)
        {
            base.ReduceCooldown(reduction);
            cooldownHandler.DecreaseCurrentCooldown(reduction);
        }
        public override void ResetCooldown()
        {
            base.ResetCooldown();
            cooldownHandler.ForceFinish();
        }
    }

    public class SkillCooldownHandler
    {
        public bool Ready => ReadyTime <= Time.time;
        public float ReadyTime { get; private set; }
        public float TimeLeft => Mathf.Max(ReadyTime - Time.time, 0);
        public float ProgressFraction => Mathf.Clamp01(1 - (TimeLeft / _currentCooldownDuration));

        public event Action onCooldownFinish = delegate { };

        float _currentCooldownDuration = -1;
        MonoBehaviour _coroutineHandler;
        Coroutine _currentCoroutine;

        public SkillCooldownHandler(MonoBehaviour coroutineHandler)
        {
            _coroutineHandler = coroutineHandler;
        }

        public void StartCooldown(float duration)
        {
            // if in the middle of cooldown, cancel it
            if (_currentCoroutine != null)
            {
                _coroutineHandler.StopCoroutine(_currentCoroutine);
                _currentCoroutine = null;
            }

            _currentCooldownDuration = duration;
            ReadyTime = Time.time + _currentCooldownDuration;
            _currentCoroutine = _coroutineHandler.StartCoroutine(CheckCooldownCoroutine());
        }
        public void ForceFinish()
        {
            ReadyTime = 0;
            if (_currentCoroutine == null) return;
            _coroutineHandler.StopCoroutine(_currentCoroutine);
            _currentCoroutine = null;
            onCooldownFinish.Invoke();
        }

        public void IncreaseCurrentCooldown(float increase)
        {
            if (Ready) return;
            ReadyTime += increase;
        }
        public void DecreaseCurrentCooldown(float decrease)
        {
            ReadyTime -= decrease;
            if (_currentCoroutine != null)
                _coroutineHandler.StopCoroutine(_currentCoroutine);
            if (Ready)
            {
                onCooldownFinish.Invoke();
                _currentCoroutine = null;
            }
            else
            {
                _currentCoroutine = _coroutineHandler.StartCoroutine(CheckCooldownCoroutine());
            }
        }

        IEnumerator CheckCooldownCoroutine()
        {
            do
            {
                yield return new WaitForSeconds(TimeLeft);
            }
            while (!Ready);
            _currentCoroutine = null;
            onCooldownFinish.Invoke();
        }
    }


    // public interface IDynamicCooldown
    // {
    //     void ForceFinishCooldown();
    //     void IncreaseCurrentCooldown(float increase);
    //     void DecreaseCurrentCooldown(float reduction);
    // }

    // expandable framework for handling skills and their dependancies
    public abstract class SkillsManagerOld : MonoBehaviour//, ISkillsManager
    {
        public virtual Vector3 TargetedPos { set => TargetPos = value; }
        public abstract int SkillCount { get; }

        public Vector3 TargetedDisplacement => TargetPos - transform.position;
        protected Vector3 TargetPos;

        protected Character character;
        protected CharacterStats Stats => character.stats;

        /// <summary> up to character behavour to set, has to be set on awake </summary>
        public virtual void SetUp(Character setTo)
        {
            character = setTo;
            enabled = true;
        }

        protected virtual void Start()
        {
            if (character == null)
                Debug.LogError("Skill manager not initialized");
        }

        public abstract ISkill GetSkill(int skillIndex = 0);

        public void ActivateSkill(int skillIndex = 0)
        {
            ISkill chosenSkill = GetSkill(skillIndex);
            if (chosenSkill != null && chosenSkill.Ready)
                chosenSkill.Activate();
        }
        public void CancelSkill(int skillIndex = 0)
        {
            ISkill chosenSkill = GetSkill(skillIndex);
            chosenSkill?.Cancel();
        }
        void OnDisable()
        {
            for (int i = 0; i < SkillCount; i++)
            {
                ISkill skill = GetSkill(i);
                skill.Cancel();
                skill.enabled = false;
            }
        }
        void OnEnable()
        {
            // Debug.Log("On enabled called", this);
            for (int i = 0; i < SkillCount; i++)
            {
                ISkill skill = GetSkill(i);
                skill.enabled = true;
            }
        }
    }
    public class OldSkillCrap : Skill, IDisplayableSkill
    {
        // ISkill implementation
        public override bool Ready => base.Ready && (cooldown?.Ready ?? true);
        public bool Active { get; set; }
        public override void Activate() => OnActivate?.Invoke();
        public override void Cancel() => OnCancel?.Invoke();

        // IDisplayableSkill Implementation
        public override bool ShowTimer => cooldown.cooldownDuration >= 2;
        public override float CooldownProgress => cooldown?.ProgressFraction ?? 1;
        public override float CooldownLeft => cooldown?.TimeLeft ?? 0;

        public Cooldown cooldown;
        readonly Action OnActivate;
        readonly Action OnCancel;

        public OldSkillCrap(Action _start, float cooldownTime = 0) : this(_start, () => { }, cooldownTime) { }
        public OldSkillCrap(Action _start, Action _stop, float cooldownTime = 0)
        {
            enabled = true;

            cooldown = (cooldownTime == 0) ? null : new Cooldown(cooldownTime);

            OnActivate = _start;
            OnCancel = _stop;
        }
    }
    public interface ISkill
    {
        /// <summary> Interface for disabling skills </summary>
        bool enabled { get; set; }
        // /// <summary> Tells if skill can be activated now </summary>
        bool Ready { get; }
        void Activate();
        void Cancel();
    }
}