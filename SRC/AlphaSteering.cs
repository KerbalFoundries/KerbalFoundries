/*
 * KSP [0.23.5] Anti-Grav Repulsor plugin by Lo-Fi
 * Much inspiration and a couple of code snippets for deployment taken from BahamutoD's Critter Crawler mod. Huge respect, it's a fantastic mod :)
 * 
 */


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KerbalFoundries
{
    [KSPModule("AlphaSteering")]
    public class AlphaSteering : PartModule
    {
        public WheelCollider thiswheelCollider;        //container for wheelcollider we grab from wheelmodule
        public WheelCollider mywc;
        public JointSpring userspring;

        [KSPField]
        public float Rideheight;        //this is what's tweaked by the line above
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Spring strength"), UI_FloatRange(minValue = 0, maxValue = 6.00f, stepIncrement = 0.2f)]
        public float SpringRate;        //this is what's tweaked by the line above
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Damping"), UI_FloatRange(minValue = 0, maxValue = 1.00f, stepIncrement = 0.025f)]
        public float DamperRate;        //this is what's tweaked by the line above

        public float TargetPosition;
        //steeringstuff
        public Transform steeringFound;
        public Transform smoothSteering;

        public float smoothSpeed = 40f;
        [KSPField(isPersistant = true)]
        public Vector3 thisTransform;

        public float rearWheel;
        public float frontWheel;
        public float frontToBack;
        public float midToFore;
        public float offset;
        public float myPositionx; //debug only
        public float myPositionz; //debug only
        public float myPosition;
        public float myAdjustedPosition;
        public float steeringRatio;

        //[KSPField(isPersistant = false, guiActive = true, guiName = "XPos", guiFormat = "F6")]
        public float maxXPos;
        //[KSPField(isPersistant = false, guiActive = true, guiName = "YPos", guiFormat = "F6")]
        public float maxYPos;
        //[KSPField(isPersistant = false, guiActive = true, guiName = "ZPos", guiFormat = "F6")]
        public float maxZPos;

        //[KSPField(isPersistant = false, guiActive = true, guiName = "XPos", guiFormat = "F6")]
        public float minXPos;
        //[KSPField(isPersistant = false, guiActive = true, guiName = "YPos", guiFormat = "F6")]
        public float minYPos;
        //[KSPField(isPersistant = false, guiActive = true, guiName = "ZPos", guiFormat = "F6")]
        public float minZPos;

        public float maxXMinX; //max positional values front to back
        public float maxYMinY;
        public float maxZMinZ;

        public float halfX;     //mid point between above values
        public float halfY;
        public float halfZ;

        public float offsetX;   //calculated positional offset
        public float offsetY;
        public float offsetZ;

        public float adjustedPosX;  //normalised position within vessel
        public float adjustedPosY;
        public float adjustedPosZ;

        [KSPField(isPersistant = false, guiActive = false, guiName = "Pos X", guiFormat = "F2")]
        public float posX;
        [KSPField(isPersistant = false, guiActive = false, guiName = "Pos Y", guiFormat = "F2")]
        public float posY;
        [KSPField(isPersistant = false, guiActive = false, guiName = "Pos Z", guiFormat = "F2")]
        public float posZ;
        [KSPField(isPersistant = false, guiActive = false, guiName = "X ratio", guiFormat = "F6")]
        public float steeringRatioX = .001f;
        [KSPField(isPersistant = false, guiActive = false, guiName = "Y ratio", guiFormat = "F6")]
        public float steeringRatioY = .001f;
        [KSPField(isPersistant = false, guiActive = false, guiName = "Z ratio", guiFormat = "F6")]
        public float steeringRatioZ = .001f;

        [KSPField(isPersistant = false, guiActive = false, guiName = "Dot.X", guiFormat = "F6")]
        public float dotx; //debug only
        [KSPField(isPersistant = false, guiActive = false, guiName = "Dot.Y", guiFormat = "F6")]
        public float doty; //debug only
        [KSPField(isPersistant = false, guiActive = false, guiName = "Dot.Z", guiFormat = "F6")]
        public float dotz; //debug only

        //[KSPField(isPersistant = false, guiActive = true, guiName = "Direct", guiUnits = "deg", guiFormat = "F1")]
        //float tempSmoothSteeringx;
        //[KSPField(isPersistant = false, guiActive = true, guiName = "divided", guiUnits = "deg", guiFormat = "F1")]
        float steeringAngleSmoothed;
        //[KSPField(isPersistant = false, guiActive = true, guiName = "Normalised", guiUnits = "deg", guiFormat = "F1")]
        float steeringAngle;


        Vector3 steeringVector;

        //begin start
        public override void OnStart(PartModule.StartState start)  //when started
        {
            thiswheelCollider = part.gameObject.GetComponentInChildren<WheelCollider>();   //find the 'wheelCollider' gameobject named by KSP convention.
            mywc = thiswheelCollider.GetComponent<WheelCollider>();
            userspring = mywc.suspensionSpring;

            //currentSpring = mywc.suspensionSpring.spring;
            // degub only: print("onstart");
            base.OnStart(start);
            steeringFound = transform.Search("steering");
            //print(steeringFound);
            smoothSteering = transform.Search("smoothSteering");

            //arrow = transform.Search("Arrow"); DEBUG ONLY

            //grab initial Euler angles for our steering object
            Vector3 passback = new Vector3(smoothSteering.transform.localEulerAngles.x, smoothSteering.transform.localEulerAngles.y, smoothSteering.transform.localEulerAngles.z);

            print("start called");

            if (HighLogic.LoadedSceneIsEditor)
            {
                print("Starting SpringRate is:");
                print(SpringRate);
                if (SpringRate == 0) //check if a value exists already. This is important, because if a wheel has been tweaked from the default value, we will overwrite it!
                {
                    print("part creation");
                    //thiswheelCollider = part.gameObject.GetComponentInChildren<WheelCollider>();   //find the 'wheelCollider' gameobject named by KSP convention.
                    //mywc = thiswheelCollider.GetComponent<WheelCollider>();     //pull collider properties
                    userspring = mywc.suspensionSpring;         //set up jointspring to modify spring property
                    SpringRate = mywc.suspensionSpring.spring;              //pass to springrate to be used in the GUI
                    DamperRate = mywc.suspensionSpring.damper;
                    Rideheight = mywc.suspensionDistance;
                    TargetPosition = mywc.suspensionSpring.targetPosition;
                }
            }

            if (HighLogic.LoadedSceneIsFlight)
            {
 
                this.part.force_activate(); //force the part active or OnFixedUpate is not called
                //Start of initial proportional steering routine

                myPosition = this.part.orgPos.y;
                myPositionx = this.part.orgPos.x;

                myPositionz = this.part.orgPos.z;

                posX = this.part.orgPos.x;
                posY = this.part.orgPos.y;
                posZ = this.part.orgPos.z;
                
                if (SpringRate == 0) //check if a value exists already. This is important, because if a wheel has been tweaked from the default value, we will overwrite it!
                {
                    SpringRate = userspring.spring;                                    //pass to springrate to be used in the GUI
                    DamperRate = userspring.damper;
                    Rideheight = mywc.suspensionDistance;
                }
                else //set the values from those stored in persistance
                {
                    userspring.spring = SpringRate;
                    userspring.damper = DamperRate;
                    mywc.suspensionSpring = userspring;

                }

                
                //find positions

                maxXPos = this.part.orgPos.x;
                maxYPos = this.part.orgPos.y;
                maxZPos = this.part.orgPos.z;
                minXPos = this.part.orgPos.x;
                minYPos = this.part.orgPos.y;
                minZPos = this.part.orgPos.z;

                foreach (AlphaSteering st in this.vessel.FindPartModulesImplementing<AlphaSteering>()) //scan vessel to find fore or rearmost wheel. 
                {
                    if ((st.part.orgPos.x + 1000) >= (maxXPos + 1000)) //dodgy hack. Make sure all values are positive or we struggle to evaluate < or >
                    {
                        maxXPos = st.part.orgPos.x; //Store transform y value
                    }

                    if ((st.part.orgPos.x + 1000) <= (minXPos + 1000))
                    {
                        minXPos = st.part.orgPos.x; //Store transform y value
                    }

                    if ((st.part.orgPos.y + 1000) >= (maxYPos + 1000)) //dodgy hack. Make sure all values are positive or we struggle to evaluate < or >
                    {
                        maxYPos = st.part.orgPos.y; //Store transform y value
                    }

                    if ((st.part.orgPos.y + 1000) <= (minYPos + 1000))
                    {
                        minYPos = st.part.orgPos.y; //Store transform y value
                    }

                    if ((st.part.orgPos.z + 1000) >= (maxZPos + 1000)) //dodgy hack. Make sure all values are positive or we struggle to evaluate < or >
                    {
                        maxZPos = st.part.orgPos.z; //Store transform y value
                    }

                    if ((st.part.orgPos.z + 1000) <= (minZPos + 1000))
                    {
                        minZPos = st.part.orgPos.z; //Store transform y value
                    }
                }

                maxXMinX = maxXPos - minXPos;
                halfX = maxXMinX / 2;
                offsetX = (maxXPos + minXPos) / 2;
                adjustedPosX = posX - offsetX;

                maxYMinY = maxYPos - minYPos;
                halfY = maxYMinY / 2;
                offsetY = (maxYPos + minYPos) / 2;
                adjustedPosY = posY - offsetY;

                maxZMinZ = maxZPos - minZPos;
                halfZ = maxZMinZ / 2;
                offsetZ = (maxZPos + minZPos) / 2;
                adjustedPosZ = posZ - offsetZ;

                steeringRatioX = adjustedPosX / halfX;
                steeringRatioY = adjustedPosY / halfY;
                steeringRatioZ = adjustedPosZ / halfZ;

                steeringRatioX = Math.Abs(steeringRatioX);
                steeringRatioY = Math.Abs(steeringRatioY);
                steeringRatioZ = Math.Abs(steeringRatioZ);
            }//end isInFlight
        }//end start

        public override void OnFixedUpdate()
        {
            dotx = Math.Abs(Vector3.Dot(this.part.transform.right, vessel.ReferenceTransform.up)); // up is forward
            doty = Math.Abs(Vector3.Dot(this.part.transform.forward, vessel.ReferenceTransform.up));
            dotz = Math.Abs(Vector3.Dot(this.part.transform.up, vessel.ReferenceTransform.up));
            // code below deals with proportional steering and smoothing input.

            if (steeringFound.transform.localEulerAngles.y > 180.0f) //if greater than 180, we want it to be negative to evaluate properly
            {
                steeringAngle = (steeringFound.transform.localEulerAngles.y - 360.0f) * ((steeringRatioX * dotx) + (steeringRatioY * doty) + (steeringRatioZ * dotz)); //multiply by the ratio 0-1 we generate below.
            }
            else
            {
                steeringAngle = steeringFound.transform.localEulerAngles.y * ((steeringRatioX * dotx) + (steeringRatioY * doty) + (steeringRatioZ * dotz));
            }
            //next line does smoothing of input
            steeringAngleSmoothed = Mathf.Lerp(steeringAngle, steeringAngleSmoothed, Time.deltaTime * 40f);
            steeringVector.y = steeringAngleSmoothed; //pass the angle back to our Vector3
            smoothSteering.transform.localEulerAngles = steeringVector; //pass the whole Vector3 to the transform as we can't directly modify localEulerAngles
        } //end OnFixedUpdate 
    }//end class
} //end namespace
