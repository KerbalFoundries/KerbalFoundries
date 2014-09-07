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
        public int referenceDirection;
        [KSPField(isPersistant = true)]
        public bool brakesApplied;
        [KSPField(isPersistant = false, guiActive = true, guiName = "Min", guiFormat = "F6")]
        public float minPos;
        [KSPField(isPersistant = false, guiActive = true, guiName = "Max", guiFormat = "F6")]
        public float maxPos;
        [KSPField(isPersistant = false, guiActive = true, guiName = "Min to Max", guiFormat = "F6")]
        public float minToMax;
        [KSPField(isPersistant = false, guiActive = true, guiName = "Mid", guiFormat = "F6")]
        public float midPoint;
        [KSPField(isPersistant = false, guiActive = true, guiName = "Offset", guiFormat = "F6")]
        public float offset;

        [KSPField(isPersistant = false, guiActive = true, guiName = "Adjuted position", guiFormat = "F6")]
        public float myAdjustedPosition;
        [KSPField(isPersistant = false, guiActive = true, guiName = "Steering Ratio", guiFormat = "F6")]
        public float steeringRatio;

        [KSPField(isPersistant = false, guiActive = true, guiName = "Dot.X", guiFormat = "F6")]
        public float dotx; //debug only
        [KSPField(isPersistant = false, guiActive = true, guiName = "Dot.Y", guiFormat = "F6")]
        public float doty; //debug only
        [KSPField(isPersistant = false, guiActive = true, guiName = "Dot.Z", guiFormat = "F6")]
        public float dotz; //debug only

        public string right = "right";
        public string forward = "forward";
        public string up = "up";

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
                SetupRatios();
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
                referenceDirection = 0;
            }
            if (doty > dotx && doty > dotz)
            {
                print("root part mounted forward");
                myPosition = this.part.orgPos.y;
                referenceTranformVector = this.vessel.ReferenceTransform.up;
                referenceDirection = 1;
            }
            if (dotz > doty && dotz > dotx)
            {
                print("root part mounted up");
                myPosition = this.part.orgPos.z;
                referenceTranformVector = this.vessel.ReferenceTransform.forward;
                referenceDirection = 2;
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

        public void SetupRatios()
        {
            maxPos = myPosition;
            minPos = myPosition;

            foreach (ModuleWheelMaster st in this.vessel.FindPartModulesImplementing<ModuleWheelMaster>()) //scan vessel to find fore or rearmost wheel. 
            {
                float otherPosition = myPosition;
                otherPosition = st.part.orgPos[referenceDirection];
             
                if ((otherPosition + 1000) >= (maxPos + 1000)) //dodgy hack. Make sure all values are positive or we struggle to evaluate < or >
                    maxPos = otherPosition; //Store transform y value

                if ((otherPosition + 1000) <= (minPos + 1000))
                    minPos = otherPosition; //Store transform y value
            }

            minToMax = maxPos - minPos;
            midPoint = minToMax / 2;
            offset = (maxPos + minPos) / 2;
            myAdjustedPosition = myPosition - offset;

            steeringRatio = myAdjustedPosition / midPoint;

            if (steeringRatio == 0 || float.IsNaN(steeringRatio)) //check is we managed to evaluate to zero or infinity somehow.
                steeringRatio = 1;
        }

        public void DestroyBounds()
        {
            Transform bounds = transform.Search("Bounds");
            if (bounds != null)
            {
                GameObject.Destroy(bounds.gameObject);
                //boundsDestroyed = true; //remove the bounds object to let the wheel colliders take over
                print("destroying Bounds");
            }
        }
    }
}
