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
    [KSPModule("Repulsor")]
    public class Repulsor : PartModule
    {
        public WheelCollider thiswheelCollider;        //container for wheelcollider we grab from wheelmodule
        public WheelCollider mywc;

        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Height"), UI_FloatRange(minValue = 0, maxValue = 4.00f, stepIncrement = 0.5f)]
        public float Rideheight;        //this is what's tweaked by the line above
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Strength"), UI_FloatRange(minValue = 0, maxValue = 3.00f, stepIncrement = 0.2f)]
        public float SpringRate;        //this is what's tweaked by the line above
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Damping"), UI_FloatRange(minValue = 0, maxValue = 0.2f, stepIncrement = 0.025f)]
        public float DamperRate;        //this is what's tweaked by the line above
        [KSPField(isPersistant = true)]
        public bool deployed = true;

        //begin start
        public override void OnStart(PartModule.StartState start)  //when started
        {
            // degub only: print("onstart");
            base.OnStart(start);


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
                //ADD CODE HERE TO DEAL WITH WHETHER WE ARE IN WHEEL OR REPULSOR MODE!


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

                thiswheelCollider = part.gameObject.GetComponentInChildren<WheelCollider>();   //find the 'wheelCollider' gameobject named by KSP convention.
                mywc = thiswheelCollider.GetComponent<WheelCollider>();     //pull collider properties
                JointSpring userspring = mywc.suspensionSpring;         //set up jointspring to modify spring property
                SpringRate = userspring.spring;                                    //pass to springrate to be used in the GUI
                DamperRate = userspring.damper;
                Rideheight = mywc.suspensionDistance;

            }
            else //set the values from those stored in persistance
            {
                //thiswheelCollider = part.gameObject.GetComponentInChildren<WheelCollider>();   //find the 'wheelCollider' gameobject named by KSP convention.
                //mywc = thiswheelCollider.GetComponent<WheelCollider>();     //pull collider properties
                //suspension:
                JointSpring userspring = mywc.suspensionSpring;         //set up jointspring to modify spring property
                userspring.spring = SpringRate;
                userspring.damper = DamperRate;
                mywc.suspensionSpring = userspring;

                if (deployed == true) //is the deployed flag set? set the rideheight appropriately
                {
                    thiswheelCollider.suspensionDistance = Rideheight;
                    Events["deploy"].active = false;
                    Events["retract"].active = true;                            //make sure gui starts in deployed state
                }
                else
                {
                    thiswheelCollider.suspensionDistance = 0;                  //set retracted if the deployed flag is not set
                }
            }
        }//end start


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
            foreach (Repulsor rp in this.vessel.FindPartModulesImplementing<Repulsor>())
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
            foreach (Repulsor rp in this.vessel.FindPartModulesImplementing<Repulsor>())
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
