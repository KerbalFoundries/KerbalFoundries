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
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Spring strength"), UI_FloatRange(minValue = 0, maxValue = 3.00f, stepIncrement = 0.2f)]
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

                frontWheel = this.part.orgPos.y; //values for forwe and aftmost wheels
                rearWheel = this.part.orgPos.y;

                foreach (AlphaSteering st in this.vessel.FindPartModulesImplementing<AlphaSteering>()) //scan vessel to find fore or rearmost wheel. 
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
         } //end OnFixedUpdate 

    }//end class
} //end namespace
