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
    [KSPModule("RepulsorAlpha")]
    public class RepulsorAlpha : PartModule
    {
        public WheelCollider mywc;
        public JointSpring userspring;


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

            mywc = this.part.GetComponentInChildren<WheelCollider>();     //pull collider properties
            userspring = mywc.suspensionSpring;
            



            if (HighLogic.LoadedSceneIsEditor)
            {
                foreach (ModuleWheel mw in part.FindModulesImplementing<ModuleWheel>())
                {

                    mw.Events["LockSteering"].guiActiveEditor = false;
                    mw.Events["DisableMotor"].guiActiveEditor = false;
                    mw.Events["EnableMotor"].guiActiveEditor = false;
                    mw.Events["InvertSteering"].guiActiveEditor = false;
                    mw.Events["DisableMotor"].guiActiveEditor = false;      //stop the gui items for wheels showing in editor
                    mw.Actions["InvertSteeringAction"].active = false;
                    mw.Actions["LockSteeringAction"].active = false;
                    mw.Actions["UnlockSteeringAction"].active = false;
                    mw.Actions["ToggleSteeringAction"].active = false;
                    mw.Actions["ToggleMotorAction"].active = false;
                    mw.Actions["BrakesAction"].active = false;
                }
            }
            if (HighLogic.LoadedSceneIsFlight)
            {
                this.part.force_activate(); //force the part active or OnFixedUpate is not called
                foreach (ModuleWheel mw in this.vessel.FindPartModulesImplementing<ModuleWheel>())
                {

                    foreach (WheelCollider wc in mw.GetComponentsInChildren<WheelCollider>())
                    {

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

               
                SpringRate = userspring.spring;                                    //pass to springrate to be used in the GUI
                DamperRate = userspring.damper;
                Rideheight = mywc.suspensionDistance;

            }
            else //set the values from those stored in persistance
            {

                userspring.spring = SpringRate;
                userspring.damper = DamperRate;
                mywc.suspensionSpring = userspring;

                if (deployed == true) //is the deployed flag set? set the rideheight appropriately
                {
                    mywc.suspensionDistance = Rideheight;
                }
                else
                {
                    mywc.suspensionDistance = 0;                  //set retracted if the deployed flag is not set
                }
            }
        }//end start

        public override void OnFixedUpdate()
        {
            if (deployed)
            {
                float electricCharge = part.RequestResource("ElectricCharge", .05f);
                if (electricCharge == 0)
                {
                    deployed = false;
                    print("retracting due to low electricity");
                    foreach (WheelCollider wc in this.part.GetComponentsInChildren<WheelCollider>())
                    {
                        wc.suspensionDistance = 0;
                    }
                }
            }
        }

        [KSPAction("Toggle Deployed")]
        public void AGToggleDeployed(KSPActionParam param)
        {
            if (deployed)
            {
                retract(param);
            }
            else
            {
                deploy(param);
            }
        }//End Deploy toggle

        [KSPAction("Retract")]
        public void retract(KSPActionParam param)
        {
            if (deployed == true)
            {
                deployed = false;
                print("Retracting");
                foreach (WheelCollider wc in this.part.GetComponentsInChildren<WheelCollider>())
                {
                    wc.suspensionDistance = 0;
                }
            }
        }//End Retract

        [KSPAction("Deploy")]
        public void deploy(KSPActionParam param)
        {
            if (deployed == false)
            {
                deployed = true;
                print("Deploying");
                foreach (WheelCollider wc in this.part.GetComponentsInChildren<WheelCollider>())
                {
                    wc.suspensionDistance = Rideheight;
                }
            }
        }//end Deploy

    }//end class
} //end namespace
