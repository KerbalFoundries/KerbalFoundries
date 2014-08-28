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

        //steeringstuff
        public Transform steeringFound;
        public Transform smoothSteering;
  
        [KSPField(isPersistant = false, guiActive = false, guiName = "Min", guiFormat = "F6")]
        public float minPos;
        [KSPField(isPersistant = false, guiActive = false, guiName = "Max", guiFormat = "F6")]
        public float maxPos;
        [KSPField(isPersistant = false, guiActive = false, guiName = "Min to Max", guiFormat = "F6")]
        public float minToMax;
        [KSPField(isPersistant = false, guiActive = false, guiName = "Mid", guiFormat = "F6")]
        public float midPoint;
        [KSPField(isPersistant = false, guiActive = false, guiName = "Offset", guiFormat = "F6")]
        public float offset;

        [KSPField(isPersistant = false, guiActive = false, guiName = "Adjuted position", guiFormat = "F6")]
        public float myAdjustedPosition;
        [KSPField(isPersistant = true, guiActive = true, guiName = "Steering Ratio", guiFormat = "F6")]
        public float steeringRatio;
        public float smoothSpeed = 40f;

        [KSPField(isPersistant = false, guiActive = true, guiName = "Dot.X", guiFormat = "F6")]
        public float dotx; //debug only
        [KSPField(isPersistant = false, guiActive = true, guiName = "Dot.Y", guiFormat = "F6")]
        public float doty; //debug only
        [KSPField(isPersistant = false, guiActive = true, guiName = "Dot.Z", guiFormat = "F6")]
        public float dotz; //debug only

        float steeringAngleSmoothed;
        float steeringAngle;
        Vector3 steeringVector;

        public Vector3 referenceTranform;
        public float myPosition;
        [KSPField(isPersistant = false, guiActive = true, guiName = "Reference Direction")]
        public string referenceDirection;
        public string right = "right";
        public string forward = "forward";
        public string up = "up";



        //begin start
        public override void OnStart(PartModule.StartState start)  //when started
        {
            thiswheelCollider = part.gameObject.GetComponentInChildren<WheelCollider>();   //find the 'wheelCollider' gameobject named by KSP convention.
            mywc = thiswheelCollider.GetComponent<WheelCollider>();
            userspring = mywc.suspensionSpring;
            base.OnStart(start);
            steeringFound = transform.Search("steering");
            smoothSteering = transform.Search("smoothSteering");
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
                }
            }

            if (HighLogic.LoadedSceneIsFlight)
            {
 
                this.part.force_activate(); //force the part active or OnFixedUpate is not called
                //Start of initial proportional steering routine

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
                FindDirection();
                SetupRatios();
            }//end isInFlight
        }//end start

        public void FindDirection()
        {
            dotx = Math.Abs(Vector3.Dot(this.part.transform.forward, vessel.ReferenceTransform.right)); // up is forward
            doty = Math.Abs(Vector3.Dot(this.part.transform.forward, vessel.ReferenceTransform.up));
            dotz = Math.Abs(Vector3.Dot(this.part.transform.forward, vessel.ReferenceTransform.forward));

            if (dotx > doty && dotx > dotz)
            {
                print("root part mounted sideways");
                myPosition = this.part.orgPos.x;
                referenceTranform = this.vessel.ReferenceTransform.right;
                referenceDirection = right;
            }
            if (doty > dotx && doty > dotz)
            {
                print("root part mounted forward");
                myPosition = this.part.orgPos.y;
                referenceTranform = this.vessel.ReferenceTransform.up;
                referenceDirection = forward;
            }
            if (dotz > doty && dotz > dotx)
            {
                print("root part mounted up");
                myPosition = this.part.orgPos.z;
                referenceTranform = this.vessel.ReferenceTransform.forward;
                referenceDirection = up;
            }
        }

        public void SetupRatios()
        {
            maxPos = myPosition;
            minPos = myPosition;

            foreach (AlphaSteering st in this.vessel.FindPartModulesImplementing<AlphaSteering>()) //scan vessel to find fore or rearmost wheel. 
            {
                float otherPosition = myPosition;
                if (referenceDirection == right)

                    otherPosition = st.part.orgPos.x;
                if (referenceDirection == forward)

                    otherPosition = st.part.orgPos.y;

                if (referenceDirection == up)
                    otherPosition = st.part.orgPos.z;

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
            steeringRatio = Math.Abs(steeringRatio);

            if (steeringRatio == 0 || float.IsNaN(steeringRatio)) //check is we managed to evaluate to zero or infinity somehow.
                steeringRatio = 1;
        }



        public override void OnFixedUpdate()
        {
            if (this.part.Splashed)
            {
            }
            // code below deals with proportional steering and smoothing input.

            if (steeringFound.transform.localEulerAngles.y > 180.0f) //if greater than 180, we want it to be negative to evaluate properly
            {
                steeringAngle = (steeringFound.transform.localEulerAngles.y - 360.0f) *steeringRatio; //multiply by the ratio 0-1 we generate below.
            }
            else
            {
                steeringAngle = steeringFound.transform.localEulerAngles.y *steeringRatio;
            }
            //next line does smoothing of input
            steeringAngleSmoothed = Mathf.Lerp(steeringAngle, steeringAngleSmoothed, Time.deltaTime * 40f);
            steeringVector.y = steeringAngleSmoothed; //pass the angle back to our Vector3
            smoothSteering.transform.localEulerAngles = steeringVector; //pass the whole Vector3 to the transform as we can't directly modify localEulerAngles
        } //end OnFixedUpdate 
    }//end class
} //end namespace
