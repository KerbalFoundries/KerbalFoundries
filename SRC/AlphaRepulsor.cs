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

    public class AlphaRepulsor : PartModule
    {

        public JointSpring userspring;

        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Height"), UI_FloatRange(minValue = 0, maxValue = 8f, stepIncrement = 0.5f)]
        public float Rideheight;        //this is what's tweaked by the line above
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Strength"), UI_FloatRange(minValue = 0, maxValue = 3.00f, stepIncrement = 0.2f)]
        public float SpringRate;        //this is what's tweaked by the line above
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Damping"), UI_FloatRange(minValue = 0, maxValue = 0.2f, stepIncrement = 0.025f)]
        public float DamperRate;        //this is what's tweaked by the line above
        [KSPField]
        public bool deployed;
        [KSPField]
        public bool lowEnergy;

        public float repulsorCount = 0;

        public float chargeConsumptionRate = 0.01f;
        //begin start
        public override void OnStart(PartModule.StartState start)  //when started
        {
            // degub only: print("onstart");
            base.OnStart(start);

            if (HighLogic.LoadedSceneIsEditor)
            {
                foreach (WheelCollider b in this.part.GetComponentsInChildren<WheelCollider>())
                {
                    print("Editor");
                    userspring = b.suspensionSpring;

                    if (SpringRate == 0) //check if a value exists already. This is important, because if a wheel has been tweaked from the default value, we will overwrite it!
                    {
                        SpringRate = userspring.spring;                                    //pass to springrate to be used in the GUI
                        DamperRate = userspring.damper;
                        Rideheight = b.suspensionDistance;
                    }
                    else //set the values from those stored in persistance
                    {
                        userspring.spring = SpringRate;
                        userspring.damper = DamperRate;
                        b.suspensionSpring = userspring;
                        b.suspensionDistance = Rideheight;
                    }
                }
                print(PartResourceLibrary.Instance.resourceDefinitions);
            }
            if (HighLogic.LoadedSceneIsFlight)
            {
                if (FlightGlobals.ActiveVessel.rootPart.GetComponentInChildren<ModuleWaterCollider>() == null)
                {
                    print("Adding WaterCollider");
                    FlightGlobals.ActiveVessel.rootPart.AddModule("ModuleWaterCollider");
                }
                else
                    print("No idea what happened");

                this.part.force_activate(); //force the part active or OnFixedUpate is not called

                foreach (WheelCollider b in this.part.GetComponentsInChildren<WheelCollider>())
                {
                    repulsorCount += 1;
                    userspring = b.suspensionSpring;
                    userspring.spring = SpringRate;
                    userspring.damper = DamperRate;
                    b.suspensionSpring = userspring;
                    b.suspensionDistance = Rideheight;

                    if (Rideheight > 0) //is the deployed flag set? set the rideheight appropriately
                    {
                        b.enabled = true;
                    }
                    else if (Rideheight < .5f)
                    {
                        b.enabled = false;                 //set retracted if the deployed flag is not set 
                    }

                }
            }

            Transform bounds = transform.Search("Bounds");
            if (bounds != null)
            {
                GameObject.Destroy(bounds.gameObject);
                //boundsDestroyed = true; //remove the bounds object to left the wheel colliders take over
                print("destroying Bounds");
            }


        }//end start 

        public override void OnFixedUpdate()
        {
            foreach (WheelCollider wc in this.part.GetComponentsInChildren<WheelCollider>())
            {
                wc.suspensionDistance = Rideheight;
                if (Rideheight < 0.5f)
                {
                    wc.enabled = false;
                    deployed = false;
                }
                else
                {
                    wc.enabled = true;
                    deployed = true;
                }
            }

            if (deployed)
            {
                part.RequestResource("ElectricCharge", (chargeConsumptionRate * (Rideheight / 8) * (1 + SpringRate) * repulsorCount));
                float electricCharge = Extensions.GetBattery(this.part);
                if (electricCharge < 0.1f)
                {
                    print("Retracting due to low electricity");
                    lowEnergy = true;
                    Rideheight = 0;
                }
                else
                {
                    lowEnergy = false;
                }
            }
        }

        [KSPAction("Retract")]
        public void retract(KSPActionParam param)
        {
            if (Rideheight > 0)
            {
                Rideheight -= 0.5f;
                print("Retracting");
            }

        }//End Retract

        [KSPAction("Extend")]
        public void extend(KSPActionParam param)
        {
            if (Rideheight < 8)
            {
                Rideheight += 0.5f;
                print("Extending");
            }
        }//end Deploy
    }//end class
} //end namespace
