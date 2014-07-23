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
    [KSPModule("RepulsorTest")]
    public class RepulsorTest : PartModule
    {

        public JointSpring userspring;

        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Height"), UI_FloatRange(minValue = 0, maxValue = 8f, stepIncrement = 0.5f)]
        public float Rideheight;        //this is what's tweaked by the line above
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Strength"), UI_FloatRange(minValue = 0, maxValue = 3.00f, stepIncrement = 0.2f)]
        public float SpringRate;        //this is what's tweaked by the line above
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Damping"), UI_FloatRange(minValue = 0, maxValue = 0.2f, stepIncrement = 0.025f)]
        public float DamperRate;        //this is what's tweaked by the line above
        //[KSPField(isPersistant = true)]
        //public bool deployed = true;

        //begin start
        public override void OnStart(PartModule.StartState start)  //when started
        {
            // degub only: print("onstart");
            base.OnStart(start);

            if (HighLogic.LoadedSceneIsEditor)
            {

            }
            if (HighLogic.LoadedSceneIsFlight)
            {
                this.part.force_activate(); //force the part active or OnFixedUpate is not called
                
                foreach (WheelCollider b in this.part.GetComponentsInChildren<WheelCollider>()) 
                {
                    print("getComponents");
                    b.enabled = true;
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


                        if (Rideheight>0) //is the deployed flag set? set the rideheight appropriately
                        {
                            b.enabled = true;
                        }
                        else if(Rideheight<.1f)
                        {
                            b.enabled = false;                 //set retracted if the deployed flag is not set
                        }
                    }
                }
            }


        }//end start

        public override void OnFixedUpdate()
        {

        }

        [KSPAction("Retract")]
        public void retract(KSPActionParam param)
        {
            if ( Rideheight>0)
            {
                //deployed = false;
                print("Retracting"); 
                foreach (WheelCollider wc in this.part.GetComponentsInChildren<WheelCollider>())
                {
                    Rideheight -= 0.001f;
                    wc.suspensionDistance = Rideheight;
                    wc.enabled = false;
                    if (Rideheight < 0.1f)
                    {
                        wc.enabled = false;
                    }
                }
            }
        }//End Retract

        [KSPAction("Extend")]
        public void deploy(KSPActionParam param)
        {
            if (Rideheight<8)
            {
                //deployed = true;
                print("Extending");
                foreach (WheelCollider wc in this.part.GetComponentsInChildren<WheelCollider>())
                {
                    Rideheight += 0.001f;
                    wc.suspensionDistance = Rideheight;
                    wc.enabled = true;

                }
            }
        }//end Deploy

    }//end class
} //end namespace
