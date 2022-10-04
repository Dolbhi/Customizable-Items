using System.Collections.Generic;
using UnityEngine;
using System;

namespace ColbyDoan
{
    public class Carrier : MonoBehaviour
    {
        [HideInInspector] public KinematicObject self;

        public float EffectiveMass => self.mass + CarriedMass;
        public float CarriedMass { get; private set; }
        public event Action<KinematicObject> OnDrop = delegate { };

        List<KinematicObject> carriedObjects = new List<KinematicObject>();
        Dictionary<KinematicObject, CarryingInfo> carryingInfo = new Dictionary<KinematicObject, CarryingInfo>();

        void Update()
        {
            for (int i = carriedObjects.Count - 1; i >= 0; i--)
            {
                ManageObject(carryingInfo[carriedObjects[i]]);
            }
        }
        // does carrying calculations
        void ManageObject(CarryingInfo info)
        {
            // pos adjustment
            Vector3 carriedDisplacement = transform.position + info.relativeCarryPos - info.carriedObject.transform.position;
            if (carriedDisplacement.magnitude > 1)
            {
                Drop(info.carriedObject);
                return;
            }
            info.carriedObject.controller.Move(carriedDisplacement - Vector3.SmoothDamp(carriedDisplacement, Vector3.zero, ref info.currentCarriedV, 0.03f));
            // velocity adjustment
            Vector3 averagedV = (self.velocity * (EffectiveMass - info.carriedObject.mass) + info.carriedObject.velocity * info.carriedObject.objCarrier.EffectiveMass) / (EffectiveMass - info.carriedObject.mass + info.carriedObject.objCarrier.EffectiveMass);
            //Debug.Log("averageV: " + averagedV);
            self.ForceTo(averagedV, info.holdForce * Time.deltaTime);
            info.carriedObject.ForceTo(averagedV, info.holdForce * Time.deltaTime);
        }

        public void TeleportCarriedIntoPos()
        {
            foreach (KinematicObject obj in carriedObjects)
            {
                CarryingInfo info = carryingInfo[obj];
                if (!info.followTeleports) continue;
                obj.transform.position = transform.position + info.relativeCarryPos;
            }
        }

        public void Carry(KinematicObject toCarry, Vector3 carryPos, float holdForce, bool followTeleports = false)
        {
            // Carry object carrying toCarry iSend carry request up to carrier
            //if (carriedBy)
            //{
            //    Carry(carriedBy, carryPos, holdForce);
            //    return;
            //}
            carriedObjects.Add(toCarry);
            carryingInfo.Add(toCarry, new CarryingInfo(toCarry, holdForce, carryPos, followTeleports));

            CarriedMass += toCarry.mass;
            toCarry.carriedBy = this;
            toCarry.controller.Move(transform.position + carryPos - toCarry.transform.position, false);
        }
        public void Drop(KinematicObject toDrop)
        {
            Debug.Log(self.name + " dropped " + toDrop.name);

            carriedObjects.Remove(toDrop);
            carryingInfo.Remove(toDrop);

            toDrop.carriedBy = null;
            CarriedMass -= toDrop.mass;
            OnDrop.Invoke(toDrop);
            //framesOutOfRange = 0;
        }
        public void DropAll()
        {
            while (carriedObjects.Count > 0)
            {
                Drop(carriedObjects[0]);
            }
        }

        class CarryingInfo
        {
            public KinematicObject carriedObject;
            public float holdForce;
            public Vector3 relativeCarryPos;
            public bool followTeleports;

            public Vector3 currentCarriedV;

            public CarryingInfo(KinematicObject toCarry, float _holdForce, Vector3 carryPos, bool _followTeleports)
            {
                carriedObject = toCarry;
                holdForce = _holdForce;
                relativeCarryPos = carryPos;
                followTeleports = _followTeleports;
            }
        }
    }
}