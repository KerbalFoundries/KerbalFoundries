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
    [KSPModule("SteeringTest")]
    public class SteeringTest : PartModule
    {
        public WheelCollider thiswheelCollider;        //container for wheelcollider we grab from wheelmodule
        public WheelCollider mywc;
        public JointSpring userspring;

        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Height"), UI_FloatRange(minValue = 0, maxValue = 2.00f, stepIncrement = 0.25f)]
        public float Rideheight;        //this is what's tweaked by the line above
        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Strength"), UI_FloatRange(minValue = 0, maxValue = 3.00f, stepIncrement = 0.2f)]
        public float SpringRate;        //this is what's tweaked by the line above
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Damping"), UI_FloatRange(minValue = 0, maxValue = 1.00f, stepIncrement = 0.025f)]
        public float DamperRate;        //this is what's tweaked by the line above

        public float TargetPosition;

        //which side is wheel on:
        [KSPField(isPersistant = true)]
        public bool reverseMotorSet = false;
        [KSPField(isPersistant = true)]
        public bool reverseMotor = false;
        //steeringstuff
        public Transform steeringFound;
        public Transform smoothSteering;
        //public Transform arrow;
        [KSPField(isPersistant = false, guiActive = true, guiName = "AntiRoll", guiUnits = "N", guiFormat = "F1")]
        public float antiRollForce;
        //[KSPField(isPersistant = false, guiActive = true, guiName = " Damped Anti-Roll", guiUnits = "N", guiFormat = "F1")]
        public float DampedAntiRollForce;

        public float smoothSpeed = 40f;
        [KSPField(isPersistant = true)]
        public Vector3 thisTransform;
        [KSPField(isPersistant = true)]
        public string thisPartID;
        [KSPField(isPersistant = true, guiActive = true, guiName = "PartnerID", guiFormat = "F1")]
        public string partnerID;
        [KSPField(isPersistant = false, guiActive = true, guiName = "My Suspension Travel", guiFormat = "F1")]
        public float suspensionTravel; // 0-1
        [KSPField(isPersistant = false, guiActive = true, guiName = "Partner Suspension Travel", guiFormat = "F1")]
        public float partnerSuspensionTravel;
        //[KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Anti-roll"), UI_FloatRange(minValue = 1.00f, maxValue = 2.50f, stepIncrement = .25f)]
        //public float antiRoll = 1f;
        //[KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Anti-roll"), UI_FloatRange(minValue = 0.00f, maxValue = 10f, stepIncrement = .5f)]
        public float antiRollMultiplier = 1; // Do not set this to 0. BAD stuff happens :/
        [KSPField(isPersistant = true)]
        public bool LHS;
        [KSPField(isPersistant = false, guiActive = false, guiName = "Rear", guiFormat = "F1")]
        public float rearWheel;
        [KSPField(isPersistant = false, guiActive = false, guiName = "Front", guiFormat = "F1")]
        public float frontWheel;
        [KSPField(isPersistant = false, guiActive = false, guiName = "Front To Back", guiFormat = "F1")]
        public float frontToBack;
        [KSPField(isPersistant = false, guiActive = false, guiName = "Mid To Front", guiFormat = "F1")]
        public float midToFore;
        [KSPField(isPersistant = false, guiActive = false, guiName = "Offset", guiFormat = "F1")]
        public float offset;
        [KSPField(isPersistant = false, guiActive = false, guiName = "Pos.X", guiFormat = "F1")]
        public float myPositionx; //debug only
        [KSPField(isPersistant = false, guiActive = false, guiName = "Pos.Z", guiFormat = "F1")]
        public float myPositionz; //debug only
        [KSPField(isPersistant = false, guiActive = false, guiName = "Pos.Y", guiFormat = "F1")]
        public float myPosition;
        [KSPField(isPersistant = false, guiActive = false, guiName = "Pos.Adjusted", guiFormat = "F1")]
        public float myAdjustedPosition;
        [KSPField(isPersistant = false, guiActive = true, guiName = "Steering Ratio", guiFormat = "F1")]
        public float steeringRatio;

        //[KSPField(isPersistant = false, guiActive = true, guiName = "Direct", guiUnits = "deg", guiFormat = "F1")]
        //float tempSmoothSteeringx;
        //[KSPField(isPersistant = false, guiActive = true, guiName = "divided", guiUnits = "deg", guiFormat = "F1")]
        float steeringAngleSmoothed;
        //[KSPField(isPersistant = false, guiActive = true, guiName = "Normalised", guiUnits = "deg", guiFormat = "F1")]
        float steeringAngle;

        public Vector3 vesselWorldCom;
        [KSPField]
        public Vector3 vesselLocalCoM;

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
                /*
                foreach (ModuleWheel mw in part.FindModulesImplementing<ModuleWheel>())
                {
                    //mw.Events["toggle"].guiActiveEditor = false;
                         mw.steeringMode = ModuleWheel.SteeringModes.ManualSteer;
                    mw.Events["LockSteering"].guiActiveEditor = false;
                    mw.Events["DisableMotor"].guiActiveEditor = false;
                    mw.Events["EnableMotor"].guiActiveEditor = false;
                    mw.Events["InvertSteering"].guiActiveEditor = false;
                    mw.Events["DisableMotor"].guiActiveEditor = false;      //stop the gui items for wheels showing in editor
                     * 
                }
                 * */
            }

            if (HighLogic.LoadedSceneIsFlight)
            {
                partnerID = "NoPartner"; //set so anti-roll partner is re-found and will be diabled if none found.
                this.part.force_activate(); //force the part active or OnFixedUpate is not called
                //DampedAntiRollForce = 0.001f;
                //antiRollForce = 0.001f;


//Start of initial proportional steering routine

                myPosition = this.part.orgPos.y;
                myPositionx = this.part.orgPos.x;
                print("my X position");
                myPositionz = this.part.orgPos.z;
                foreach (SteeringTest st in this.vessel.FindPartModulesImplementing<SteeringTest>())
                {
                    //myPosition = (float) Math.Round(myPosition, 1);
                    if (((float)Math.Round(this.part.orgPos.y, 1)) == ((float)Math.Round(st.part.orgPos.y, 1)) && ((float)Math.Round(this.part.orgPos.x, 1)) == ((float)Math.Round(-st.part.orgPos.x, 1)))
                    { //sometimes symmetry does not put things in _EXACTLY_ the same place, meaning the pairing would fail. Rounding to one decimal place seems to make it much more robust.
                        print("Partner Found :)");
                        partnerID = st.part.ConstructID;
                        print(st.part.ConstructID);
                    }
                    else
                    {
                        print("This is not my partner :(");
                    }
                }
                if (partnerID == "NoPartner")
                {
                    partnerID = this.part.ConstructID; //this effectively disables anti-roll on this wheel that has not found a partner as roll
                }

                //Left or rigth handed

                thisPartID = this.part.ConstructID;

                if (thisTransform.x <= 0)
                {
                    LHS = false;
                    print("I'm right handed");
                }
                else
                {
                    LHS = true;
                    print("I'm left handed");
                }

                //find positions

                frontWheel = this.part.orgPos.y; //values for forwe and aftmost wheels
                rearWheel = this.part.orgPos.y;

                foreach (SteeringTest st in this.vessel.FindPartModulesImplementing<SteeringTest>()) //scan vessel to find fore or rearmost wheel. 
                {
                    if ((st.part.orgPos.y + 1000) >= (frontWheel + 1000)) //dodgy hack. Make sure all values are positive or we struggle to evaluate < or >
                    {
                        frontWheel = st.part.orgPos.y; //Store transform y value
                        //print(st.part.orgPos.y);
                    }

                    if ((st.part.orgPos.y + 1000) <= (rearWheel + 1000))
                    {
                        rearWheel = st.part.orgPos.y; //Store transform y value
                        //print(st.part.orgPos.y);
                    }

                }
                //grab this one to compare
                frontToBack = frontWheel - rearWheel; //distance front to back wheel
                midToFore = frontToBack / 2;
                offset = (frontWheel + rearWheel) / 2; //here is the problem

                myAdjustedPosition = myPosition - offset; //change the value based on our calculated offset 
                steeringRatio = myAdjustedPosition / midToFore; // this sets how much this wheel steers as a proportion of rear/front wheel steering angle 
                if (steeringRatio < 0) //this just changes all values to positive
                {
                    steeringRatio /= -1; //if it's negative
                }

            }//end isInFlight
        }//end start

        public override void OnFixedUpdate()
        {
            //smoothSteering.transform.rotation = Quaternion.Lerp(steeringFound.transform.rotation, smoothSteering.transform.rotation, Time.deltaTime * smoothSpeed);
            //above is original code for smoothing steering input. Depracated.


            // code below deals with proportional steering and smoothing input.

            if (steeringFound.transform.localEulerAngles.y > 180.0f) //if greater than 180, we want it to be negative to evaluate properly
            {
                steeringAngle = (steeringFound.transform.localEulerAngles.y - 360.0f) * steeringRatio; //multiply by the ratio 0-1 we generate below.
            }
            else
            {
                steeringAngle = steeringFound.transform.localEulerAngles.y * steeringRatio;
            }
            //next line does smoothing of input
            steeringAngleSmoothed = Mathf.Lerp(steeringAngle, steeringAngleSmoothed, Time.deltaTime * 40f);
            steeringVector.y = steeringAngleSmoothed; //pass the angle back to our Vector3
            smoothSteering.transform.localEulerAngles = steeringVector; //pass the whole Vector3 to the transform as we can't directly modify localEulerAngles
            // start of anti-roll
            WheelHit hit;
            bool grounded = mywc.GetGroundHit(out hit); //set up to pass out wheelhit coordinates
            if (grounded) //is it on the ground
            {
                suspensionTravel = (-mywc.transform.InverseTransformPoint(hit.point).y - mywc.radius) / mywc.suspensionDistance; //out hit does not take wheel radius into account
            }

            foreach (SteeringTest st in this.vessel.FindPartModulesImplementing<SteeringTest>())
            {
                if (st.thisPartID == partnerID) //find my paired wheel
                {
                    partnerSuspensionTravel = st.suspensionTravel;
                }
            }
            antiRollForce = ((suspensionTravel - partnerSuspensionTravel) / 2f) * (antiRollMultiplier + 0.1f); //now we have both we can calculate.
            DampedAntiRollForce = Mathf.Lerp(antiRollForce, DampedAntiRollForce, Time.deltaTime * 20f);

            foreach (WheelCollider hhh in this.part.GetComponentsInChildren<WheelCollider>())
            {
                JointSpring jjj = hhh.suspensionSpring;
                jjj.spring = SpringRate;
                hhh.suspensionSpring = jjj;
            }

            //mywc.suspensionDistance = Rideheight - DampedAntiRollForce; //add a force based on the anti roll value. Note: this does not simply press the veseel onto the floor!!

        } //end OnFixedUpdate 

//general functions
        [KSPEvent(guiActive = true, guiName = "Show CoM", active = true)]
        public void thisCoM()
        {

           vesselLocalCoM = this.part.vessel.ReferenceTransform.InverseTransformPoint(vessel.CoM);
           print(vesselLocalCoM);          
        }


    }//end class
} //end namespace
