using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using ColbyDoan.FixedTimeLerp;
namespace ColbyDoan
{
    using CharacterBase;
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
        public new SpriteRenderer renderer;
        public bool rotateToVelocity = true;

        // HashSet<Collider2D> pastCollisions = new HashSet<Collider2D>();
        Dictionary<Collider2D, int> colliderTimeOut = new Dictionary<Collider2D, int>();

        public Vector3 Velocity { get => velocity; set { velocity = value; } }
        public float Mass => mass;
        public bool Grounded => false;
        public float GravityMultiplier { get => gravityMultiplier; set { gravityMultiplier = value; } }

        const float DESTROY_DELAY = .5f;

        Transform _transform;

        void Awake()
        {
            _transform = transform;
        }

        void Start()
        {
            //print(System.Convert.ToString(damagables, 2));
            if (lifetime != 0)
            {
                StartCoroutine(DelayedDestruction(lifetime));
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
                _transform.right = velocity;

            Vector3 move = velocity * Time.fixedDeltaTime;

            // cache pos
            Vector3 pos = _transform.position;
            // Vector2 displacedPos = pos.GetDepthApparentPosition();

            //          E
            //    -     -
            //--->|     E
            //    +     -   E
            //(DC)-         -
            // projectile hitbox size is 2 unit tall, biased upwards (extends downwards if depthCrossing)
            float zFloor = Mathf.Floor(pos.z);
            RaycastHit2D hitboxHit = Physics2D.Raycast(pos, move, ((Vector2)move).magnitude, damagables, zFloor - .1f, zFloor + 1f);
            Debug.DrawRay(pos, move, Color.red);

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
                    _transform.position = new Vector3(solidHit.point.x, solidHit.point.y, pos.z);
                    if (!spectral)
                    {
                        OnHit.Invoke(new ProjectileHitInfo(true, true, solidHit));
                        StartCoroutine(DelayedDestruction(DESTROY_DELAY));
                        enabled = false;
                        renderer.enabled = false;
                        return;
                    }
                    else
                    {
                        OnHit.Invoke(new ProjectileHitInfo(true, false, solidHit));
                        // pastCollisions = newCollisions;
                        colliderTimeOut.Add(solidHit.collider, 2);
                        return;
                    }
                }
            }
            else if (_ProcessHitBoxCheck(hitboxHit))
            {
                // return immediately if something was hit, move is handled in function
                return;
            }

            // pastCollisions = newCollisions;
            _transform.Translate(move, Space.World);

            // step down if applicable
            Vector3 newPos = _transform.position;
            if (depthCrossing && newPos.z > 1)
            {
                // check for solids at its current position and at where it would end up (1 unit down in depth and 1 unit up)
                if (TileManager.Instance.GetTerrainHeight(newPos + Vector3.up) < newPos.z - 1 && TileManager.Instance.GetTerrainHeight(newPos) < newPos.z - 1)
                {
                    // if theres none step projectile down
                    _transform.Translate(0, 1, -1, Space.World);
                    // displacedPos = pos.GetDepthApparentPosition();

                    // recheck for hitboxes that may have passed under
                    pos += new Vector3(0, 1, -1);
                    zFloor = Mathf.Floor(pos.z);
                    hitboxHit = Physics2D.Raycast(pos, move, ((Vector2)move).magnitude, damagables, zFloor - .1f, zFloor + 1f);
                    Debug.DrawRay(pos, move, Color.red);
                    _ProcessHitBoxCheck(hitboxHit);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="hitboxHit"></param>
        /// <returns>if something was hit</returns>
        bool _ProcessHitBoxCheck(RaycastHit2D hitboxHit)
        {
            if (hitboxHit)// kinematic collisions
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
                    _transform.position = new Vector3(hitboxHit.point.x, hitboxHit.point.y, _transform.position.z);
                    if (!piercing)
                    {
                        // don't pierce and destroy
                        OnHit.Invoke(new ProjectileHitInfo(false, true, hitboxHit));
                        StartCoroutine(DelayedDestruction(DESTROY_DELAY));
                        enabled = false;
                        renderer.enabled = false;
                        return true;
                    }
                    else
                    {
                        // pierce
                        OnHit.Invoke(new ProjectileHitInfo(false, false, hitboxHit));
                        // pastCollisions = newCollisions;
                        colliderTimeOut.Add(hitboxHit.collider, 2);
                        return true;
                    }
                }
            }

            return false;
        }

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
            Instantiate(hitSoundProp, _transform.position, Quaternion.identity);
        }

        // public void SetFaction(int faction)
        // {
        //     damagables = PhysicsSettings.hitboxes & ~1 >> faction;
        // }

        IEnumerator DelayedDestruction(float duration)
        {
            yield return new WaitForSeconds(duration);
            DestroyProjectile();
        }
        void DestroyProjectile()
        {
            Destroy(gameObject);
        }
    }

    public class ProjectileHitInfo
    {
        public ProjectileHitInfo(bool _solidHit, bool _destroyed, RaycastHit2D raycastHit2D)//, Vector3 _move)
        {
            solidHit = _solidHit;
            destroyed = _destroyed;
            raycastInfo = raycastHit2D;
            // move = _move;
        }

        public bool solidHit;
        public bool destroyed;
        public RaycastHit2D raycastInfo;
        // public Vector3 move;
    }
}