using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KerbalFoundries
{
    [KSPModule("ModuleWheelMaster")]
    public class ModuleWheelMaster : PartModule
    {
        public int directionCorrector;
        public bool boundsDestroyed;
        public Vector3 referenceTranformVector;
        public Transform referenceTransform;
        public float myPosition;
        [KSPField(isPersistant = false, guiActive = true, guiName = "Reference Direction")]
        public string referenceDirection;
        [KSPField(isPersistant = true)]
        public bool brakesApplied;

        public override void OnStart(PartModule.StartState start)  //when started
        {
            print("ModuleTrack called");
            base.OnStart(start);
            if (HighLogic.LoadedSceneIsEditor)
            {

            }

            if (HighLogic.LoadedSceneIsFlight)
            {
                FindDirection();
                DestroyBounds();
            }
        }

        public void FindDirection()
        {
            float dotx = Math.Abs(Vector3.Dot(this.part.transform.forward, vessel.ReferenceTransform.right)); // up is forward
            float doty = Math.Abs(Vector3.Dot(this.part.transform.forward, vessel.ReferenceTransform.up));
            float dotz = Math.Abs(Vector3.Dot(this.part.transform.forward, vessel.ReferenceTransform.forward));

            if (dotx > doty && dotx > dotz)
            {
                print("root part mounted sideways");
                myPosition = this.part.orgPos.x;
                referenceTranformVector = this.vessel.ReferenceTransform.right;
            }
            if (doty > dotx && doty > dotz)
            {
                print("root part mounted forward");
                myPosition = this.part.orgPos.y;
                referenceTranformVector = this.vessel.ReferenceTransform.up;
            }
            if (dotz > doty && dotz > dotx)
            {
                print("root part mounted up");
                myPosition = this.part.orgPos.z;
                referenceTranformVector = this.vessel.ReferenceTransform.forward;
            }

            float dot = Vector3.Dot(this.part.transform.forward, referenceTranformVector); // up is forward
            if (dot < 0) // below 0 means the engine is on the left side of the craft
            {
                directionCorrector = -1;
                print("left");
            }
            else
            {
                directionCorrector = 1;
                print("right");
            }
        }

        public void DestroyBounds()
        {
            Transform bounds = transform.Search("Bounds");
            if (bounds != null)
            {
                GameObject.Destroy(bounds.gameObject);
                //boundsDestroyed = true; //remove the bounds object to left the wheel colliders take over
                print("destroying Bounds");
            }
        }
    }
}
