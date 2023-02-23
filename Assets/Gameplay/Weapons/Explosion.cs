using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace ColbyDoan
{
    using CharacterBase;
    using Physics;

    public class Explosion : MonoBehaviour
    {
        [Range(0, 1)]
        public float falloff;
        public float radius;
        public DamageInfo damage;
        public float terrainDamage;

        // bool destroySelf;
        // bool detachSelf;

        [SerializeField] SpriteRenderer spriteRenderer = null;

        void Awake()
        {
            transform.SetParent(null);
            transform.localScale = Vector3.one;
            spriteRenderer.color = Color.white;
            damage.knockback.impulseMode = true;
            Explode();
        }

        void Explode()
        {
            //print("exploding " + damage.damage);
            HarmEntities();
            DamageTerrain();
            ExplosionTween();
            // StartCoroutine(ExplosionAnimation());
        }

        void HarmEntities()
        {
            // find all hitboxes within range
            Collider2D[] results = Physics2D.OverlapCircleAll(transform.position, radius, PhysicsSettings.kinematics);
            foreach (Collider2D collider in results)
            {
                DamageInfo toDamage = damage;

                // halves damage outside falloff
                if (Vector3.Distance(transform.position, collider.transform.position) > falloff * radius) toDamage.damage *= .5f;

                // first linecast
                RaycastHit2D hit = PhysicsSettings.SolidsLinecast(transform.position, collider.transform.position);

                // subsiquent casts
                while (hit && toDamage.damage > 1)
                {
                    // reduce impact for each .5 units through solid
                    toDamage.damage *= .6f;
                    toDamage.knockback.i *= .6f;
                    hit = PhysicsSettings.SolidsLinecast(Vector3.MoveTowards(hit.point, collider.transform.position, .5f), collider.transform.position);
                }

                if (toDamage.damage < 1) continue;

                toDamage.knockback.v = collider.transform.position - transform.position;
                // Debug.Log($"Before: {damage.knockback}, After: {toDamage.knockback}");
                toDamage.ApplyTo(collider.transform);
            }
        }

        void DamageTerrain()
        {
            if (terrainDamage == 0) return;

            int rayCount = Mathf.CeilToInt(radius * 4);
            float rayDamage = terrainDamage / rayCount;
            float raySeperation = 2 * Mathf.PI / rayCount;
            float damageDecayRate = rayDamage / radius;

            for (int i = 0; i < rayCount; i++)
            {
                float angle = raySeperation * i;
                Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

                Vector3 rayStart = transform.position;
                float distanceLeft = radius;
                float damageLeft = rayDamage;
                RaycastHit2D hit;

                int count = 0;

                do
                {
                    count++;
                    // raycast for solids
                    hit = PhysicsSettings.SolidsRaycast(rayStart, direction, distanceLeft);

                    if (count > 50)
                    {
                        print("endless loop at i=" + i + " point: " + hit.point);
                        break;
                    }

                    if (hit)
                    {
                        // update damage left
                        damageLeft -= hit.distance * damageDecayRate;
                        distanceLeft -= hit.distance;

                        // check if tile hit has already been destroyed
                        if (!TileManager.Instance.GetTile(hit.point + direction * 0.1f, Mathf.CeilToInt(transform.position.z)))
                        {
                            // if so then jump forwards in raycast
                            damageLeft -= damageDecayRate * .4f;
                            distanceLeft -= .4f;
                            rayStart = hit.point + direction * .4f;
                            continue;
                        }

                        // // destroy tile if theres enough damage
                        // if (damageLeft > 1)
                        // {
                        // Debug.Log($"{damageLeft} damage done to tile at {hit.point + direction}");
                        // TileManager.instance.DestroyTile(hit.point + direction * 0.1f, Mathf.CeilToInt(transform.position.z));

                        float damageCost = TileManager.Instance.DamageTile(hit.point + direction * 0.1f, Mathf.CeilToInt(transform.position.z), damageLeft);

                        // Debug.Log($"Left: {damageLeft}, Cost: {damageCost}, Distance left: {distanceLeft}");

                        // update everything else
                        damageLeft -= damageCost + damageDecayRate * .4f;
                        distanceLeft -= .4f;
                        rayStart = hit.point + direction * .4f;
                        // }
                    }

                    //if (PhysicsSettings.CheckForSolids(hit.point,.01f))
                    //{
                    //    print("colliders dont update");
                    //    break;
                    //}

                } while (hit && damageLeft > 0); // repeat if a solid was hit and theres enough damage left
            }

            TileManager.Instance.ClearDamage();

        }

        const float intialExpansionDuration = .25f;
        const float postExpansionDuration = .5f;
        const float postExpansionExcess = 1.2f;

        void ExplosionTween()
        {
            float scaleLimit = radius / spriteRenderer.bounds.extents.x;
            var scaleTween = transform.DOScale(scaleLimit * Vector3.one, 1.1f).SetEase(Ease.OutExpo);
            scaleTween.onComplete += () => { GameObject.Destroy(gameObject); };
            scaleTween.Play();
            spriteRenderer?.DOColor(new Color(1, 1, 1, 0), 1).SetEase(Ease.OutExpo).Play();
        }

        IEnumerator ExplosionAnimation()
        {
            // set scale
            float spriteRadius = spriteRenderer.bounds.extents.x;
            float scaleLimit = radius / spriteRadius;

            // fast expansion
            float intialRate = scaleLimit / intialExpansionDuration;

            while (transform.localScale.x < scaleLimit)
            {
                yield return null;
                transform.localScale += Vector3.one * intialRate * Time.deltaTime;
            }

            // slow expansion
            transform.localScale = scaleLimit * Vector3.one;

            float postRate = (postExpansionExcess - 1) * scaleLimit / postExpansionDuration;
            while (transform.localScale.x < scaleLimit * postExpansionExcess)
            {
                transform.localScale += Vector3.one * postRate * Time.deltaTime;
                spriteRenderer.SetSpriteAlpha(Mathf.InverseLerp(scaleLimit * postExpansionExcess, scaleLimit, transform.localScale.x));
                yield return null;
            }

            Destroy(gameObject);
        }
    }
}