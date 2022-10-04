using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ColbyDoan
{
    public class StatusEffectsManager : MonoBehaviour, IAutoDependancy<Character>
    {
        public Character Dependancy { set { character = value; } }
        [HideInInspector] public Character character;

        public List<IStatusEffect> activeEffects = new List<IStatusEffect>();

        Dictionary<string, IStatusEffect> statusEffects = new Dictionary<string, IStatusEffect>();
        public T GetStatus<T>(string statusName) where T : class, IStatusEffect, new()
        {
            if (statusEffects.ContainsKey(statusName))
            {
                return statusEffects[statusName] as T;
            }
            // create and set up statusEffect
            T newStatus = new T();
            newStatus.SetUp(this);
            statusEffects.Add(newStatus.Name, newStatus);
            return newStatus;
        }
    }

    #region Status effect framework
    public interface IStatusEffect
    {
        string Name { get; }
        int StackCount { get; }
        bool IsDebuff { get; }
        void SetUp(StatusEffectsManager target);
        void CancelStatus();
    }
    public class Timer : IHeapItem<Timer>
    {
        public float DurationLeft => endTime - Time.time;
        public float endTime = 0;

        public int HeapIndex { get; set; }

        public int CompareTo(Timer other)
        {
            return -endTime.CompareTo(other.endTime);
        }

        public Timer()
        {
            endTime = 0;
            HeapIndex = 0;
        }
        public Timer(float duration)
        {
            endTime = Time.time + duration;
            HeapIndex = 0;
        }
    }

    public abstract class RefreshingStatusEffect : IStatusEffect
    {
        public abstract string Name { get; }
        public abstract bool IsDebuff { get; }
        public virtual int StackCount => active ? 1 : 0;

        protected bool active = false;
        protected StatusEffectsManager manager;
        public Timer timer = new Timer();

        Coroutine timerCoroutine;

        public virtual void SetUp(StatusEffectsManager target)
        {
            manager = target;
        }
        public void ApplyStatus(float duration)
        {
            // refresh timer
            timer = timer.DurationLeft > duration ? timer : new Timer(duration);
            if (!active)
            {
                timerCoroutine = manager.StartCoroutine(TimerCoroutine());
            }
        }
        public void CancelStatus()
        {
            if (active)
            {
                manager.StopCoroutine(timerCoroutine);
                active = false;
                StopEffect();
            }
        }

        IEnumerator TimerCoroutine()
        {
            StartEffect();
            active = true;
            while (timer.DurationLeft > 0)
            {
                yield return new WaitForSeconds(timer.DurationLeft);
            }
            active = false;
            StopEffect();
        }
        protected virtual void StartEffect()
        {
            manager.activeEffects.Add(this);
        }
        protected virtual void StopEffect()
        {
            timer.endTime = 0;
            manager.activeEffects.Remove(this);
        }
    }
    public abstract class RefreshingStatusEffect<TExtraData> : RefreshingStatusEffect
    {
        // public abstract string Name { get; }
        // public abstract bool IsDebuff { get; }
        // public virtual int Stacks => active ? 1 : 0;

        // protected bool active = false;
        // protected StatusEffectsManager manager;
        // public Timer timer = new Timer(0);
        protected TExtraData data;

        // Coroutine timerCoroutine;

        // public virtual void SetUp(StatusEffectsManager target)
        // {
        //     this.manager = target;
        // }
        public void ApplyStatus(float duration, TExtraData extraData)
        {
            // refresh timer
            // timer = timer.DurationLeft > duration ? timer : new Timer(duration);
            UpdateData(extraData);
            ApplyStatus(duration);
            // if (!active)
            // {
            //     timerCoroutine = manager.StartCoroutine(TimerCoroutine());
            // }
        }
        protected virtual void UpdateData(TExtraData newData)
        {
            data = newData;
        }
        // public void CancelStatus()
        // {
        //     if (active)
        //     {
        //         manager.StopCoroutine(timerCoroutine);
        //         active = false;
        //         StopEffect();
        //     }
        // }

        // protected virtual void StartEffect()
        // {
        //     manager.activeEffects.Add(this);
        // }
        // protected virtual void StopEffect()
        // {
        //     timer.endTime = 0;
        //     manager.activeEffects.Remove(this);
        // }

        // IEnumerator TimerCoroutine()
        // {
        //     StartEffect();
        //     active = true;
        //     while (timer.DurationLeft > 0)
        //     {
        //         yield return new WaitForSeconds(timer.DurationLeft);
        //     }
        //     active = false;
        //     StopEffect();
        // }
    }
    public abstract class StackingStatusEffect : IStatusEffect
    {
        public abstract string Name { get; }
        public abstract bool IsDebuff { get; }
        public virtual int StackCount => stackTimers.Count;

        protected StatusEffectsManager manager;
        public List<Timer> stackTimers = new List<Timer>();

        public virtual void SetUp(StatusEffectsManager target)
        {
            manager = target;
        }
        public Timer ApplyStatus(float duration)
        {
            // add new stack
            Timer timer = new Timer(duration);
            manager.StartCoroutine(TimerCoroutine(timer));
            return timer;
        }
        public void CancelStatus()
        {
            foreach (Timer timer in stackTimers)
            {
                StopEffect(timer);
            }
        }

        protected virtual void StartEffect(Timer timer)
        {
            if (stackTimers.Count == 0)
                manager.activeEffects.Add(this);
            stackTimers.Add(timer);
        }
        protected virtual void StopEffect(Timer timer)
        {
            stackTimers.Remove(timer);
            if (stackTimers.Count == 0)
                manager.activeEffects.Remove(this);
        }

        IEnumerator TimerCoroutine(Timer timer)
        {
            StartEffect(timer);
            yield return new WaitForSeconds(timer.DurationLeft);
            StopEffect(timer);
        }
    }
    public abstract class StackingStatusEffect<TExtraData> : StackingStatusEffect
    {
        protected Dictionary<Timer, TExtraData> stackData = new Dictionary<Timer, TExtraData>();

        public void ApplyStatus(float duration, TExtraData data)
        {
            stackData.Add(ApplyStatus(duration), data);
        }
        protected override void StopEffect(Timer timer)
        {
            base.StopEffect(timer);
            stackData.Remove(timer);
        }
    }
    // public abstract class StackingStatusEffect<TExtraData> : IStatusEffect
    // {
    //     public abstract string Name { get; }
    //     public abstract bool IsDebuff { get; }
    //     public virtual int Stacks => stackTimers.Count;

    //     protected StatusEffectsManager manager;
    //     public List<(Timer, TExtraData)> stackTimers = new List<(Timer, TExtraData)>();
    //     protected Dictionary<Timer, TExtraData> stackData = new Dictionary<Timer, TExtraData>();

    //     public virtual void SetUp(StatusEffectsManager target)
    //     {
    //         manager = target;
    //     }
    //     public void ApplyStatus(float duration, TExtraData data)
    //     {
    //         // add new stack
    //         Timer timer = new Timer(duration);
    //         manager.StartCoroutine(TimerCoroutine((timer, data)));
    //     }
    //     public void CancelStatus()
    //     {
    //         foreach ((Timer, TExtraData) timer in stackTimers)
    //         {
    //             StopEffect(timer);
    //         }
    //     }

    //     protected virtual void StartEffect((Timer, TExtraData) data)
    //     {
    //         if (stackTimers.Count == 0)
    //             manager.activeEffects.Add(this);
    //         stackTimers.Add(data);
    //     }
    //     protected virtual void StopEffect((Timer, TExtraData) data)
    //     {
    //         stackTimers.Remove(data);
    //         if (stackTimers.Count == 0)
    //             manager.activeEffects.Remove(this);
    //     }

    //     IEnumerator TimerCoroutine((Timer, TExtraData) data)
    //     {
    //         StartEffect(data);
    //         yield return new WaitForSeconds(data.Item1.DurationLeft);
    //         StopEffect(data);
    //     }
    // }
    #endregion
}