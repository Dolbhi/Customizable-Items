using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using ColbyDoan.FixedTimeLerp;
namespace ColbyDoan
{
    using Physics;
    [RequireComponent(typeof(InterpolatedTransform))]
    public class Projectile : MonoBehaviour, IPhysicsObject
    {
        [HideInInspector] public DamageInfo damage;
        public Vector3 velocity;
        public float gravityMultiplier;
        public float lifetime = 3;
        public float mass = .1f;
        public bool piercing = false;
        public bool spectral = false;
        public bool depthCrossing = true;

        public LayerMask damagables;
        public UnityEvent<ProjectileHitInfo> OnHit;

        public AudioSource hitSoundProp;
        public bool rotateToVelocity = true;

        // HashSet<Collider2D> pastCollisions = new HashSet<Collider2D>();
        Dictionary<Collider2D, int> colliderTimeOut = new Dictionary<Collider2D, int>();

        public Vector3 Velocity { get => velocity; set { velocity = value; } }
        public float Mass => mass;
        public bool Grounded => false;
        public float GravityMultiplier { get => gravityMultiplier; set { gravityMultiplier = value; } }

        void Start()
        {
            //print(System.Convert.ToString(damagables, 2));
            if (lifetime != 0)
            {
                StartCoroutine(DelayedDestruction());
            }
            OnHit.AddListener((_) => { PlayHitSound(); });
        }

        List<Collider2D> _keys = new List<Collider2D>();
        void FixedUpdate()
        {
            // HashSet<Collider2D> newCollisions = new HashSet<Collider2D>();
            _keys.AddRange(colliderTimeOut.Keys);
            foreach (var key in _keys)
            {
                colliderTimeOut[key]--;
                if (colliderTimeOut[key] < 0) colliderTimeOut.Remove(key);
            }
            _keys.Clear();

            velocity.z += gravityMultiplier * PhysicsSettings.gravity * Time.fixedDeltaTime;
            if (rotateToVelocity)
                transform.right = velocity;

            Vector3 move = velocity * Time.fixedDeltaTime;

            // cache pos
            Vector3 pos = transform.position;
            Vector2 displacedPos = pos.GetDepthApparentPosition();

            // step down if applicable
            if (depthCrossing && pos.z > 1)
            {
                // check for solids 1 unit down in depth and 1 unit up(where it would end up)
                if (TileManager.Instance.GetTerrainHeight(pos + Vector3.up) < pos.z - 1)
                {
                    // if theres none step projectile down
                    transform.Translate(0, 1, -1, Space.World);
                    pos = transform.position;
                    displacedPos = pos.GetDepthApparentPosition();
                }
            }

            //          E
            //    -     -
            //--->|     E
            //    +     -   E
            //(DC)-         -
            // projectile hitbox size is 2 unit tall, biased upwards (extends downwards if depthCrossing)
            RaycastHit2D hitboxHit = Physics2D.Raycast(displacedPos, move, ((Vector2)move).magnitude, damagables, pos.z - (depthCrossing ? 2.5f : 1.5f), pos.z + .3f);
            Debug.DrawRay(displacedPos, move, Color.red);

            // for solid collisions the projectile has a height of 0
            float dist = hitboxHit ? hitboxHit.distance : ((Vector2)move).magnitude;
            RaycastHit2D solidHit = Physics2D.Raycast(pos, move, dist, PhysicsSettings.solids, pos.z, pos.z + 1);
            Debug.DrawRay(pos, dist * move.normalized, Color.green);

            // solid collisions
            if (solidHit)
            {
                // check if collision is new
                // newCollisions.Add(solidHit.collider);
                if (colliderTimeOut.ContainsKey(solidHit.collider))
                {
                    colliderTimeOut[solidHit.collider]++;
                }
                else //if (!pastCollisions.Contains(solidHit.collider))
                {
                    // do damage
                    damage.knockback = new ForceInfo(mass * velocity);
                    damage.ApplyTo(solidHit.transform.root);

                    // manually set pos and move on
                    transform.position.Set(solidHit.point.x, solidHit.point.y, pos.z);
                    if (!spectral)
                    {
                        OnHit.Invoke(new ProjectileHitInfo(true, true, solidHit, move));
                        DestroyProjectile();
                        return;
                    }
                    else
                    {
                        OnHit.Invoke(new ProjectileHitInfo(true, false, solidHit, move));
                        // pastCollisions = newCollisions;
                        colliderTimeOut.Add(solidHit.collider, 2);
                        return;
                    }
                }
            }
            else if (hitboxHit)// kinematic collisions
            {
                // check if collision is new
                // newCollisions.Add(hitboxHit.collider);
                if (colliderTimeOut.ContainsKey(hitboxHit.collider))
                {
                    colliderTimeOut[hitboxHit.collider]++;
                }
                else //if (!pastCollisions.Contains(hitboxHit.collider))
                {
                    // do damage
                    damage.knockback = new ForceInfo(mass * velocity);
                    damage.ApplyTo(hitboxHit.transform.root);

                    // manually set pos and move on
                    transform.position.Set(hitboxHit.point.x, hitboxHit.point.y, pos.z);
                    if (!piercing)
                    {
                        OnHit.Invoke(new ProjectileHitInfo(false, true, hitboxHit, move));
                        DestroyProjectile();
                        return;
                    }
                    else
                    {
                        OnHit.Invoke(new ProjectileHitInfo(false, false, hitboxHit, move));
                        // pastCollisions = newCollisions;
                        colliderTimeOut.Add(hitboxHit.collider, 2);
                        return;
                    }
                }
            }

            // pastCollisions = newCollisions;
            transform.Translate(move, Space.World);
        }
        // private void CheckForCollisions(in float move, Vector3 pos, Vector2 depthApparentPos)
        // {

        // }

        /// <summary>
        /// Knock is based on momentum and is set on hit
        /// </summary>
        public Projectile FireCopy(Vector3 position, Vector2 velocity, LayerMask damageMask, DamageInfo damage, bool aimedDown = false)
        {
            Projectile fired = Instantiate(this, position, transform.rotation);
            fired.gameObject.SetActive(true);
            fired.transform.right = velocity;
            fired.damage = damage;
            fired.velocity = velocity;
            fired.damagables = damageMask;
            fired.depthCrossing = aimedDown;

            fired.GetComponent<InterpolatedTransform>().ForgetPreviousTransforms();

            return fired;
        }

        void PlayHitSound()
        {
            Instantiate(hitSoundProp, transform.position, Quaternion.identity);
        }

        public void SetFaction(int faction)
        {
            damagables = PhysicsSettings.hitboxes & ~1 >> faction;
        }

        IEnumerator DelayedDestruction()
        {
            yield return new WaitForSeconds(lifetime);
            DestroyProjectile();
        }
        public void DestroyProjectile()
        {
            Destroy(gameObject);
        }
    }

    public class ProjectileHitInfo
    {
        public ProjectileHitInfo(bool _solidHit, bool _destroyed, RaycastHit2D raycastHit2D, Vector3 _move)
        {
            solidHit = _solidHit;
            destroyed = _destroyed;
            raycastInfo = raycastHit2D;
            move = _move;
        }

        public bool solidHit;
        public bool destroyed;
        public RaycastHit2D raycastInfo;
        public Vector3 move;
    }
}