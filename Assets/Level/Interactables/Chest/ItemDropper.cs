// using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ColbyDoan
{
    public class ItemDropper : MonoBehaviour
    {
        [SerializeField] ArtifactPools pool;
        [SerializeField] ItemPickup pickupPrefab;

        Item itemToDrop;

        void Start()
        {
            itemToDrop = pool.GetRandomItem();
        }

        public void DropArtifact()
        {
            // make pickup
            ItemPickup pickup = Instantiate<ItemPickup>(pickupPrefab, transform.position + .1f * Vector3.forward, Quaternion.identity);
            pickup.CurrentItem = itemToDrop.Copy();

            // find pickup's KO and yeet it
            KinematicObject.FindFromRoot(pickup.transform.root).velocity = Quaternion.AngleAxis(UnityEngine.Random.Range(0, 360), Vector3.forward) * Vector3.right * 2 + Vector3.forward * 3;
        }
    }
}
