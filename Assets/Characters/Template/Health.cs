using System.Collections;
using UnityEngine;
using System;

using ColbyDoan.Attributes;

namespace ColbyDoan
{
    public class Health : FindableByRoot<Health>
    {
        [SerializeField][ReadOnly] float maxHealth;
        [SerializeField] float health;
        public float armor;
        public float regen;
        public bool isAlly = false;
        public bool invincible = false;

        public float MaxHealth => maxHealth;
        public float CurrentHealth { get => health; set { health = value; } }
        public float FractionFull => health / maxHealth;

        public event Action<float> OnHeal;
        public event Action<HurtInfo> OnHurt;
        public event Action OnDeath;

        protected void Awake()
        {
            StartCoroutine("RegenLoop");
        }

        public void UpdateMaxHealth(float newMax)
        {
            health = FractionFull * newMax;
            maxHealth = newMax;
        }

        private void OnValidate()
        {
            UpdateMaxHealth(GetComponent<CharacterStats>().maxHealth.FinalValue);
            health = Mathf.Clamp(health, 0, maxHealth);
        }

        //public void Damage(DamageInfo damage)
        //{
        //    if (health <= 0) return;
        //    int displayDamage = Mathf.RoundToInt(damage.damage);
        //    if (displayDamage != 0) PopupSpawner.OnSpawnPopup.Invoke(transform.position, displayDamage.ToString(), Color.red);
        //    if (invincible) return;
        //    health -= damage.damage;

        //    OnHurt?.Invoke(damage);

        //    if (health <= 0)
        //    {
        //        health = 0;
        //        OnDeath?.Invoke();
        //    }

        //    // apply knockback if applicable (done in damage info now)
        //    //kinematicObject?.EZForce(damage.knockback);
        //}

        public float Damage(float attack, bool isCrit = false, bool showPopup = true)
        {
            return Damage(attack, Vector3.zero, isCrit, showPopup);
        }

        /// <summary> Direct hp deduction </summary>
        public float Damage(float attack, Vector3 direction, bool isCrit = false, bool showPopup = true)
        {
            if (health <= 0) return 0;
            if (invincible)
            {
                if (showPopup)
                {
                    PopupSpawner.popupColor = Color.gray;
                    PopupSpawner.OnSpawnPopup?.Invoke(transform.position, "Invincible");
                }
                return 0;
            }

            // calc and deal damage
            float damage = attack * (1 - armor / (100 + Mathf.Abs(armor)));
            health -= damage;
            OnHurt?.Invoke(new HurtInfo(attack, direction, isCrit));
            if (health <= 0)
            {
                health = 0;
                OnDeath?.Invoke();
            }

            // damage numbers
            if (showPopup)
            {
                int displayDamage = Mathf.RoundToInt(damage);
                PopupSpawner.popupColor = isCrit ? Color.red : new Color(1, 1, 0);
                if (displayDamage != 0) PopupSpawner.OnSpawnPopup?.Invoke(transform.position, displayDamage.ToString());
            }

            return damage;
        }

        public void Heal(float heal, bool showPopup = true)
        {
            if (heal <= 0) return;
            if (showPopup)
            {
                PopupSpawner.popupColor = Color.green;
                PopupSpawner.OnSpawnPopup?.Invoke(transform.position, Mathf.Ceil(heal).ToString());
            }
            health += heal;
            health = Mathf.Min(maxHealth, health);
            OnHeal?.Invoke(heal);
        }

        WaitForSeconds wait1Sec = new WaitForSeconds(1);
        IEnumerator RegenLoop()
        {
            while (health > 0)
            {
                yield return wait1Sec;
                if (maxHealth != health)
                    Heal(regen, false);
            }
        }
    }

    /// <summary>
    /// For now only used in Health itself for passing hurt data around
    /// </summary>
    [System.Serializable]
    public struct HurtInfo
    {
        public float damage;
        public Vector3 direction;
        public bool crit;

        public HurtInfo(float damage, Vector3 direction, bool crit = false)
        {
            this.damage = damage;
            this.direction = direction;
            this.crit = crit;
        }
        public HurtInfo(float damage, bool crit = false) : this(damage, Vector3.zero, crit) { }
    }

    [System.Serializable]
    public struct DamageInfo
    {
        public float damage;
        public float critChance;
        public Character source;
        public ForceInfo knockback;
        public bool invokeOnHit;
        public string metaTriggerID;

        public void ApplyTo(Transform target)
        {
            if (target == null) return;
            target = target.root;
            //Debug.Log(target.name);

            float damageDone;

            Health hp;
            if (Health.instanceFromTransform.TryGetValue(target, out hp))
            {
                // apply hit
                bool isCrit = UnityEngine.Random.Range(0, 1f) < critChance;
                if (isCrit)
                {
                    // crit
                    damage *= 2;
                    damageDone = hp.Damage(damage, -knockback.v, true);
                }
                else
                {
                    damageDone = hp.Damage(damage, -knockback.v);
                }


                // trigger on hit effects
                if (invokeOnHit)
                {
                    source?.artifacts.InvokeOnHit(damageDone, isCrit, hp);
                }
                if (metaTriggerID != "")
                {
                    source?.artifacts.InvokeMeta(new TriggerContext(hp.transform.root), metaTriggerID);
                }

                // if (source)
                // {
                //     Debug.Log($"{source.transform.root.name} deals {damageDone} damage to {target.name} with a knockback of {knockback}");
                // }
            }

            KinematicObject ko;
            if (KinematicObject.instanceFromTransform.TryGetValue(target, out ko))
                ko.EZForce(knockback);
        }

        /// <summary> Damage based on character stats, damage does not update if player stats change </summary>
        /// <param name="_source"> The character dealing damage </param>
        /// <param name="_damageMultiplier"> Multiple of character damage attack will do </param>
        /// <param name="_bonusCritChance"> Crit chance added on top of character crit chance </param>
        /// <param name="_knockback"> Knockback info </param>
        /// <param name="_invokeOnHit"> Whether to invoke artifacts on hit </param>
        /// <param name="_metaTriggerID"> Custom artifact meta trigger to invoke </param>
        public DamageInfo(Character _source, float _damageMultiplier = 1, float _bonusCritChance = 0, ForceInfo _knockback = default, bool _invokeOnHit = false, string _metaTriggerID = "")
            : this(_damageMultiplier * _source.stats.attack.FinalValue, _bonusCritChance + _source.stats.critChance.FinalValue, _knockback, _source, _invokeOnHit, _metaTriggerID) { }
        /// <summary> Damage based on other values </summary>
        /// <param name="_damage"> Damage to deal </param>
        /// <param name="_critChance"> Crit chance </param>
        /// <param name="_knockback"> Knockback info </param>
        /// <param name="_source"> The character dealing damage </param>
        /// <param name="_invokeOnHit"> Whether to invoke artifacts on hit </param>
        /// <param name="_metaTriggerID"> Custom artifact meta trigger to invoke </param>
        public DamageInfo(float _damage, float _critChance, ForceInfo _knockback = default, Character _source = null, bool _invokeOnHit = false, string _metaTriggerID = "")
        {
            damage = _damage;
            critChance = _critChance;
            source = _source;
            knockback = _knockback;
            invokeOnHit = _invokeOnHit;
            metaTriggerID = _metaTriggerID;
        }

        public override string ToString()
        {
            string output = $"Damage: {damage}, CritRate: {critChance}";
            if (source) output += $", Source: {source.transform.root.name}";
            if (knockback.i != 0) output += $", Knockback: {knockback}";
            if (metaTriggerID != "") output += $", MetaTriggerID: {metaTriggerID}";
            if (invokeOnHit) output += ", Invokes on hit";

            return output;
        }
    }
}