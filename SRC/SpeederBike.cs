/*
 * KSP [0.23.5] Anti-Grav SpeederBike plugin by Lo-Fi
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
    [KSPModule("SpeederBike")]
    public class SpeederBike : PartModule
    {
        public Transform rightWheel;
        public Transform leftWheel;
        public Transform frontWheel;
        public WheelCollider thiswheelCollider;        //container for wheelcollider we grab from wheelmodule
        public WheelCollider mywc;

        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Height"), UI_FloatRange(minValue = 0, maxValue = 5.00f, stepIncrement = 0.5f)]
        public float Rideheight;        //this is what's tweaked by the line above
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Strength"), UI_FloatRange(minValue = 0, maxValue = 3.00f, stepIncrement = 0.2f)]
        public float SpringRate;        //this is what's tweaked by the line above
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Damping"), UI_FloatRange(minValue = 0, maxValue = 0.2f, stepIncrement = 0.025f)]
        public float DamperRate;        //this is what's tweaked by the line above
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Start"), UI_Toggle(disabledText = "Retracted", enabledText = "Deployed")]
        public bool deployed = false;
        [KSPField]
        public float moveForce = 25f;

        //begin start
        public override void OnStart(PartModule.StartState start)  //when started
        {
            // degub only: print("onstart");
            base.OnStart(start);


            rightWheel = transform.Search("rightTag");
            leftWheel = transform.Search("leftTag");
            frontWheel = transform.Search("body");

            thiswheelCollider = part.gameObject.GetComponentInChildren<WheelCollider>();   //find the 'wheelCollider' gameobject named by KSP convention.
            mywc = thiswheelCollider.GetComponent<WheelCollider>();     //pull collider properties
            JointSpring userspring = mywc.suspensionSpring;         //set up jointspring to modify spring property

            if (HighLogic.LoadedSceneIsEditor)
            {
                foreach (ModuleWheel mw in part.FindModulesImplementing<ModuleWheel>())
                {
                    //     mw.steeringMode = ModuleWheel.SteeringModes.ManualSteer;
                    mw.Events["LockSteering"].guiActiveEditor = false;
                    mw.Events["DisableMotor"].guiActiveEditor = false;
                    mw.Events["EnableMotor"].guiActiveEditor = false;
                    mw.Events["InvertSteering"].guiActiveEditor = false;
                    mw.Events["DisableMotor"].guiActiveEditor = false;      //stop the gui items for wheels showing in editor
                }
            }
            else
            {
                //ADD CODE HERE TO DEAL WITH WHETHER WE ARE IN WHEEL OR SpeederBike MODE!
                this.part.force_activate();

                foreach (ModuleWheel mw in this.vessel.FindPartModulesImplementing<ModuleWheel>())
                {

                    foreach (WheelCollider wc in mw.GetComponentsInChildren<WheelCollider>())
                    {
                        //        mw.steeringMode = ModuleWheel.SteeringModes.ManualSteer;
                        mw.Events["LockSteering"].guiActive = false;
                        mw.Events["DisableMotor"].guiActive = false;
                        mw.Events["EnableMotor"].guiActive = false;
                        mw.Events["InvertSteering"].guiActive = false;
                        mw.Events["DisableMotor"].guiActive = false;        //stop the gui items for wheels showing in flight
                    }

                }

            }

            if (SpringRate == 0) //check if a value exists already. This is important, because if a wheel has been tweaked from the default value, we will overwrite it!
            {
                print("springrate is 0");
                foreach (WheelCollider wc in this.part.GetComponentsInChildren<WheelCollider>())
                {
                    SpringRate = userspring.spring;                                    //pass to springrate to be used in the GUI
                    DamperRate = userspring.damper;
                    Rideheight = mywc.suspensionDistance;
                }
            }
            else //set the values from those stored in persistance
            {
                //thiswheelCollider = part.gameObject.GetComponentInChildren<WheelCollider>();   //find the 'wheelCollider' gameobject named by KSP convention.
                //mywc = thiswheelCollider.GetComponent<WheelCollider>();     //pull collider properties
                //suspension:
                print("springrate is not 0");
                foreach (WheelCollider wc in this.part.GetComponentsInChildren<WheelCollider>())
                {

                    userspring.spring = SpringRate;
                    userspring.damper = DamperRate;
                    wc.suspensionSpring = userspring;

                    if (deployed == true) //is the deployed flag set? set the rideheight appropriately
                    {
                        print("depyloyed is true");
                        wc.suspensionDistance = Rideheight;
                        Events["deploy"].active = false;
                        Events["retract"].active = true;                            //make sure gui starts in deployed state
                    }
                    else
                    {
                        print("depyloyed is false");
                        Events["deploy"].active = true;
                        Events["retract"].active = false;
                        wc.suspensionDistance = 0;                  //set retracted if the deployed flag is not set
                    }
                }
            }
        }//end start

        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
            bool left = Input.GetKey(KeyCode.A);
            bool right = Input.GetKey(KeyCode.D);
            bool accel = Input.GetKey(KeyCode.W);
            bool stop = Input.GetKey(KeyCode.S);
            
            if (this.vessel.isActiveVessel)
            {
                if (accel)
                {

                    rigidbody.AddForceAtPosition(this.part.transform.forward * moveForce, frontWheel.position);
                }
                if (left)
                {

                    rigidbody.AddForceAtPosition(-this.part.transform.forward * moveForce, leftWheel.position);
                    rigidbody.AddForceAtPosition(-this.part.transform.up * (moveForce/2), leftWheel.position);
                }
                if (right)
                {
                    rigidbody.AddForceAtPosition(-this.part.transform.forward * moveForce, rightWheel.position);
                    rigidbody.AddForceAtPosition(-this.part.transform.up * (moveForce / 2), rightWheel.position);
                }
                if (stop)
                {
;
                    rigidbody.AddForceAtPosition(-this.part.transform.forward * moveForce, frontWheel.position);
                }
            }

        }


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

        [KSPEvent(guiActive = true, guiName = "Retract All", active = false)]
        public void retract()
        {
            foreach (SpeederBike rp in this.vessel.FindPartModulesImplementing<SpeederBike>())
            {
                rp.Events["deploy"].active = true;
                rp.Events["retract"].active = false;
                rp.deployed = false;
                print("retracting");
                foreach (WheelCollider wc in rp.GetComponentsInChildren<WheelCollider>())
                {
                    wc.suspensionDistance = 0;
                }

            }
        }//End Retract All

        [KSPEvent(guiActive = true, guiName = "Deploy All", active = true)]
        public void deploy()
        {
            foreach (SpeederBike rp in this.vessel.FindPartModulesImplementing<SpeederBike>())
            {

                rp.Events["deploy"].active = false;
                rp.Events["retract"].active = true;
                rp.deployed = true;
                print("Deploying");
                foreach (WheelCollider wc in rp.GetComponentsInChildren<WheelCollider>())
                {
                    wc.suspensionDistance = rp.Rideheight;
                }
            }

        }//end Deploy All





    }//end class
} //end namespace
