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
    [KSPModule("RepulsorWheel")]
    public class RepulsorWheel : PartModule
    {
        public WheelCollider thiswheelCollider;        //container for wheelcollider we grab from wheelmodule
        public WheelCollider mywc;
        public JointSpring userspring;

        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Height"), UI_FloatRange(minValue = 0, maxValue = 2.00f, stepIncrement = 0.25f)]
        public float Rideheight;        //this is what's tweaked by the line above
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Strength"), UI_FloatRange(minValue = 0, maxValue = 3.00f, stepIncrement = 0.2f)]
        public float SpringRate;        //this is what's tweaked by the line above
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Damping"), UI_FloatRange(minValue = 0, maxValue = 1.00f, stepIncrement = 0.025f)]
        public float DamperRate;        //this is what's tweaked by the line above
        [KSPField(isPersistant = true)]
        public bool deployed = true;
        //forward friction values
        [KSPField(isPersistant = true)]
        public float forasymSlip;
        [KSPField(isPersistant = true)]
        public float forasymValue;
        [KSPField(isPersistant = true)]
        public float forextrmSlip;
        [KSPField(isPersistant = true)]
        public float forextremValue;
        [KSPField(isPersistant = true)]
        public float forstiff;
        //sideways stiffnes values
        [KSPField(isPersistant = true)]
        public float sideasymSlip;
        [KSPField(isPersistant = true)]
        public float sideasymValue;
        [KSPField(isPersistant = true)]
        public float sideextrmSlip;
        [KSPField(isPersistant = true)]
        public float sideextremValue;
        [KSPField(isPersistant = true)]
        public float sidestiff;
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
        [KSPField(isPersistant = false)]
        public float antiRoll = 2f;
        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Anti-roll"), UI_FloatRange(minValue = 1.00f, maxValue = 10.00f, stepIncrement = 1f)]
        public float antiRollMultiplier = 10; // Do not set this to 0. BAD stuff happens :/
        [KSPField(isPersistant = true)]
        public bool LHS;
        [KSPField(isPersistant = false, guiActive = true, guiName = "Rear", guiFormat = "F1")]
        public float rearWheel;
        [KSPField(isPersistant = false, guiActive = true, guiName = "Front", guiFormat = "F1")]
        public float frontWheel;
        [KSPField(isPersistant = false, guiActive = true, guiName = "Front To Back", guiFormat = "F1")]
        public float frontToBack;
        [KSPField(isPersistant = false, guiActive = true, guiName = "Mid To Front", guiFormat = "F1")]
        public float midToFore;
        [KSPField(isPersistant = false, guiActive = true, guiName = "Offset", guiFormat = "F1")]
        public float offset;
        [KSPField(isPersistant = false, guiActive = true, guiName = "Pos.X", guiFormat = "F1")]
        public float myPositionx; //debug only
        [KSPField(isPersistant = false, guiActive = true, guiName = "Pos.Z", guiFormat = "F1")]
        public float myPositionz; //debug only
        [KSPField(isPersistant = false, guiActive = true, guiName = "Pos.Y", guiFormat = "F1")]
        public float myPosition;
        [KSPField(isPersistant = false, guiActive = true, guiName = "Pos.Adjusted", guiFormat = "F1")]
        public float myAdjustedPosition;
        [KSPField(isPersistant = false, guiActive = true, guiName = "Steering Ratio", guiFormat = "F1")]
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
                    print(SpringRate);
                    DamperRate = mywc.suspensionSpring.damper;
                    print(DamperRate);
                    Rideheight = mywc.suspensionDistance;
                    print(Rideheight);
                    WheelFrictionCurve forwardfric = mywc.forwardFriction;
                    forasymValue = mywc.forwardFriction.asymptoteValue;
                    forextremValue = mywc.forwardFriction.extremumValue;
                    forstiff = forwardfric.stiffness;
                    WheelFrictionCurve sidefric = mywc.sidewaysFriction;
                    sideasymValue = mywc.sidewaysFriction.asymptoteValue;
                    sideextremValue = mywc.sidewaysFriction.extremumValue;
                    sidestiff = sidefric.stiffness;
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

            if(HighLogic.LoadedSceneIsFlight)
            {
                partnerID = "NoPartner"; //set so anti-roll partner is re-found and will be diabled if none found.
                this.part.force_activate(); //force the part active or OnFixedUpate is not called
                foreach (ModuleAnimateGeneric ma in this.part.FindModulesImplementing<ModuleAnimateGeneric>())
                {
                    ma.Events["Toggle"].guiActive = false;
                }

                //this.part.Fields["toggle"].guiActive = false;

                //Start of initial proportional steering routine

                //print("this parts position is");
                //print(this.part.orgPos);
                myPosition = this.part.orgPos.y;
                myPositionx = this.part.orgPos.x;
                print("my X position");
                myPositionz = this.part.orgPos.z;
                foreach (RepulsorWheel st in this.vessel.FindPartModulesImplementing<RepulsorWheel>())
                {
                    //myPosition = (float) Math.Round(myPosition, 1);
                    if (((float) Math.Round(this.part.orgPos.y , 1)) == ((float) Math.Round(st.part.orgPos.y , 1)) && ((float) Math.Round(this.part.orgPos.x , 1)) == ((float) Math.Round(-st.part.orgPos.x , 1)))
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

                foreach (RepulsorWheel st in this.vessel.FindPartModulesImplementing<RepulsorWheel>()) //scan vessel to find fore or rearmost wheel. 
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
                // print("steering ratio equals");
                // print(steeringratio);
                /*
                foreach (ModuleWheel mw in this.vessel.FindPartModulesImplementing<ModuleWheel>())
                {
                    
                    foreach (WheelCollider wc in mw.GetComponentsInChildren<WheelCollider>())
                    {
                        //mw.Events["toggle"].guiActive = false;
                                mw.steeringMode = ModuleWheel.SteeringModes.ManualSteer;
                        mw.Events["LockSteering"].guiActive = false;
                        mw.Events["DisableMotor"].guiActive = false;
                        mw.Events["EnableMotor"].guiActive = false;
                        mw.Events["InvertSteering"].guiActive = false;
                        mw.Events["DisableMotor"].guiActive = false;        //stop the gui items for wheels showing in flight
                          
                    }
                    
                    
                }
                */

                if (deployed == true) //is the deployed flag set? set the rideheight appropriately
                {

                    userspring = mywc.suspensionSpring;         //set up jointspring to modify spring property
                    userspring.spring = SpringRate;
                    userspring.damper = DamperRate;
                    mywc.suspensionSpring = userspring;
                    //forward friction:
                    WheelFrictionCurve forwardfric = mywc.forwardFriction;
                    forwardfric.asymptoteValue = forasymValue;
                    forwardfric.extremumValue = forextremValue;
                    forwardfric.stiffness = forstiff;
                    //sideways friction
                    WheelFrictionCurve sidefric = mywc.sidewaysFriction;
                    sidefric.asymptoteValue = sideasymValue;
                    sidefric.extremumValue = sideextremValue;
                    sidefric.stiffness = sidestiff;

                    thiswheelCollider.suspensionDistance = Rideheight;
                    Events["deploy"].active = false;
                    Events["retract"].active = true;                            //make sure gui starts in deployed state
                }

                else if (deployed == false)
                {

                    userspring = mywc.suspensionSpring;         //set up jointspring to modify spring property
                    userspring.spring = SpringRate;
                    userspring.damper = DamperRate;
                    mywc.suspensionSpring = userspring;
                    //forward friction:
                    WheelFrictionCurve forwardfric = mywc.forwardFriction;
                    forwardfric.asymptoteValue = 0.001f;
                    forwardfric.extremumValue = 0.001f;
                    forwardfric.stiffness = 0f;
                    mywc.forwardFriction = forwardfric;

                    //sideways friction
                    WheelFrictionCurve sidefric = mywc.sidewaysFriction;
                    sidefric.asymptoteValue = 0.001f;
                    sidefric.extremumValue = 0.001f;
                    sidefric.stiffness = 0f;                 //set retracted if the deployed flag is not set
                    mywc.sidewaysFriction = sidefric;

                }
            }//end isInFlight
        }//end start

        public override void OnFixedUpdate()
        {
            //smoothSteering.transform.rotation = Quaternion.Lerp(steeringFound.transform.rotation, smoothSteering.transform.rotation, Time.deltaTime * smoothSpeed);
            //above is original code for smoothing steering input. Depracated.
     
            
// code below deals with proportional steering and smoothing input.

            if (steeringFound.transform.localEulerAngles.y >180.0f) //if greater than 180, we want it to be negative to evaluate properly
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
            
            foreach (RepulsorWheel st in this.vessel.FindPartModulesImplementing<RepulsorWheel>())
            {
                if (st.thisPartID == partnerID) //find my paired wheel
                {
                    partnerSuspensionTravel = st.suspensionTravel;
                    /*
                    foreach (WheelCollider wc in st.GetComponentsInChildren<WheelCollider>()) //find the wheelcollider
                    {   //MAY BE A REALLY STUPID BUG RIGHT HERE.... I may be over-writing hit
                        WheelHit partnerHit;
                        bool partnergrounded = wc.GetGroundHit(out partnerHit); //check if partner is grounded too
                        if (partnergrounded)
                        {
                             partnerSuspensionTravel = (-wc.transform.InverseTransformPoint(partnerHit.point).y - wc.radius) / wc.suspensionDistance; //grab partners suspension travel
                        }
                    }
                    */
                }
            }
             
            antiRollForce = (float) Math.Round(((suspensionTravel - partnerSuspensionTravel) * antiRollMultiplier), 2); //now we have both we can calculate.
            //rigidbody.AddForceAtPosition(this.part.transform.up * -antiRollForce, mywc.transform.position); //add a force based on the anti roll value. Note: this does not simply press the veseel onto the floor!!

        } //end OnFixedUpdate 
    


        [KSPAction("Toggle Deployed")]
        public void AGToggleDeployed(KSPActionParam param)
        {
            if (deployed)
            {
                retract();
            }
            else
            {
                deploy();
            }
        }//End Deploy toggle
       
        public void PlayAnimation()
        {
            // note: assumes one ModuleAnimateGeneric (or derived version) for this part
            // if this isn't the case, needs fixing
            ModuleAnimateGeneric myAnimation = part.FindModulesImplementing<ModuleAnimateGeneric>().SingleOrDefault();
            if (!myAnimation)
            {
                // this shouldn't happen under normal circumstances
                //      Log.Error("Repulsor animation error: Did not find ModuleAnimateGeneric on {0}", part.ConstructID);
                return; //the Log.Error line fails syntax check with 'The name 'Log' does not appear in the current context.
            }
            else
            {
/*
                try
                {
                    myAnimation.GetType().InvokeMember("Toggle", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.IgnoreReturn | System.Reflection.BindingFlags.InvokeMethod, null, myAnimation, null);
                }   
                catch (Exception e)
                {*/
                //Events["toggle"].active = false;
                    myAnimation.Toggle();
            //    }
            }

        }

        [KSPEvent(guiActive = true, guiName = "Repulsor", active = true)]
        public void retract()
        {
            print(thisTransform);
            // note: this loop will find "us" too. Intended
            foreach (RepulsorWheel rp in this.vessel.FindPartModulesImplementing<RepulsorWheel>())
            {
                if (rp.deployed == true) //I couldn't get the line above to work correctly. It only activated the symetry partner of the part I activated from
                {
                    print(string.Format("{1} Repulsor attached to {0}", rp.part.ConstructID, rp.Events["retract"].active ? "Retracting" : "Deploying"));
                    rp.PlayAnimation();

                    // was this intended? Once deployed and 
                    // retracted, Repulsor will never be deployable again
                    rp.Events["deploy"].active = true;
                    rp.Events["retract"].active = false;
                    rp.deployed = false;

                    foreach (WheelCollider wc in rp.GetComponentsInChildren<WheelCollider>())
                    {
                        wc.suspensionDistance = rp.Rideheight * 1.75f;
                        WheelFrictionCurve sidefric = wc.sidewaysFriction;
                        sidefric.asymptoteValue = 0.001f;
                        sidefric.extremumValue = 0.001f;
                        sidefric.stiffness = 0f;
                        wc.sidewaysFriction = sidefric;

                        WheelFrictionCurve forwardfric = wc.forwardFriction;
                        forwardfric.asymptoteValue = 0.001f;
                        forwardfric.extremumValue = 0.001f;
                        forwardfric.stiffness = 0f;
                        wc.forwardFriction = forwardfric;

                    }
                }
            }

        }//end Deploy All

        [KSPEvent(guiActive = true, guiName = "Wheel", active = true)]
        public void deploy()
        {
            print(thisTransform);
            // note: this loop will find "us" too. Intended
            foreach (RepulsorWheel rp in this.vessel.FindPartModulesImplementing<RepulsorWheel>())
            {
                if (rp.deployed == false)
                {
                    print(string.Format("{1} Repulsor attached to {0}", rp.part.ConstructID, rp.Events["deploy"].active ? "Deploying" : "Retracting"));
                    rp.PlayAnimation();


                    rp.Events["deploy"].active = false;
                    rp.Events["retract"].active = true;
                    rp.deployed = true;

                    foreach (WheelCollider wc in rp.GetComponentsInChildren<WheelCollider>())
                    {
                        //   wc.suspensionDistance = Rideheight;
                        wc.suspensionDistance = rp.Rideheight;
                        WheelFrictionCurve forwardfric = wc.forwardFriction;
                        forwardfric.asymptoteValue = forasymValue;
                        forwardfric.extremumValue = forextremValue;
                        forwardfric.stiffness = forstiff;
                        wc.forwardFriction = forwardfric;
                        //debug only: print(wc.forwardFriction.asymptoteValue);
                        //debug only: print(wc.forwardFriction.extremumValue);

                        //sideways friction
                        WheelFrictionCurve sidefric = wc.sidewaysFriction;
                        sidefric.asymptoteValue = sideasymValue;
                        sidefric.extremumValue = sideextremValue;
                        sidefric.stiffness = sidestiff;
                        wc.sidewaysFriction = sidefric;
                        //debug only: print(wc.sidewaysFriction.asymptoteValue);
                        //debug only: print(wc.sidewaysFriction.extremumValue);
                    }

                }
            }

        }//end Deploy All
    }//end class
} //end namespace
