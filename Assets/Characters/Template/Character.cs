using UnityEngine;

namespace ColbyDoan
{
    /// <summary>
    /// Health, physics, hitboxes, stats, movement, artifacts
    /// </summary>
    public class Character : FindableByRoot<Character>, IAutoDependancy<KinematicObject>, IFacingCharacter
    {
        public Health healthManager;
        public ArtifactManager artifacts;
        public SkillsManager skills;
        public StatusEffectsManager statusEffects;
        public CharacterStats stats;
        public Collider2D hitbox;
        public Controller2D Controller => kinematicObject.controller;
        [HideInInspector] public KinematicObject kinematicObject;
        public KinematicObject Dependancy { set => kinematicObject = value; }

        public bool Alive { get; private set; }

        /// <summary> not normalised </summary>
        public Vector2 FacingDirection { get; set; }
        public float FacingAngle => Vector2.SignedAngle(Vector2.right, FacingDirection);
        public Vector2 Velocity => kinematicObject.velocity;
        // public bool freezeFacing

        //[ReadOnly]
        [HideInInspector] public LayerMask damageMask;

        protected virtual void Awake()
        {
            FacingDirection = Vector2.right;

            // set health
            healthManager.OnDeath += Die;
            healthManager.UpdateMaxHealth(stats.maxHealth.FinalValue);
            healthManager.CurrentHealth = stats.maxHealth.FinalValue;
            stats.maxHealth.OnStatChanged += healthManager.UpdateMaxHealth;
            // set armor
            healthManager.armor = stats.armor.FinalValue;
            stats.armor.OnStatChanged += (newValue) => { healthManager.armor = newValue; };

            Alive = true;
            damageMask = PhysicsSettings.hitboxes.Exclude(hitbox.gameObject.layer);


            //print(System.Convert.ToString(PhysicsSettings.hitboxes, 2));
            ////print(hitbox.gameObject.layer);
            //print(System.Convert.ToString(damageMask, 2));

            DependancyInjector.InjectDependancies(this);
        }

        //private void Update()
        //{
        //    //print("dir: " + FacingDirection + " angle: " + FacingAngle);
        //}

        // put stats
        protected virtual void Die()
        {
            Alive = false;
            healthManager.regen = 0;
            kinematicObject.objCarrier.DropAll();
            gameObject.SetActive(false);
            Destroy(transform.root.gameObject);
        }
    }
}
/*
 * -----Character construction notes-----
 * 1. solid collision hitbox to be on the root gameobject with no offset
 * 2. sprite to be in a child gameobject with its pivot set to bottom center with a DisplacingSprite script
 * 3. for all intents and purposes the character is treated to be centered on the root gameobject's position and raycasts, solid colliders and distance calcs will be centered there
 * 4. the only exception to 3. are projectile collisions which follows the appearence of the sprite with projectile colliders on the same gameObject as the sprite
 */