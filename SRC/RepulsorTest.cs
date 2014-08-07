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
        [KSPField(isPersistant = true)]
        public bool boundsDestroyed = false;

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
            }
            if (HighLogic.LoadedSceneIsFlight)
            {
                this.part.force_activate(); //force the part active or OnFixedUpate is not called
                
                foreach (WheelCollider b in this.part.GetComponentsInChildren<WheelCollider>()) 
                {
                    userspring = b.suspensionSpring;
                    userspring.spring = SpringRate;
                    userspring.damper = DamperRate;
                    b.suspensionSpring = userspring;
                    b.suspensionDistance = Rideheight;

                    if (Rideheight>0) //is the deployed flag set? set the rideheight appropriately
                    {
                        b.enabled = true;
                    }
                    else if(Rideheight<.5f)
                    {
                        b.enabled = false;                 //set retracted if the deployed flag is not set
                    }
                    
                }
            }

            if (boundsDestroyed == false) //has teh bounds object already been destroyed?
            {
                Transform bounds = transform.Search("Bounds");
                GameObject.Destroy(bounds.gameObject);
                boundsDestroyed = true; //remove the bounds object to left the wheel colliders take over
                print("destroying Bounds");
            }


        }//end start

        public override void OnUpdate()
        {
            foreach (WheelCollider wc in this.part.GetComponentsInChildren<WheelCollider>())
            {
                wc.suspensionDistance = Rideheight;
                if (Rideheight < 0.5f)
                {
                    wc.enabled = false;
                }
                else
                {
                    wc.enabled = true;
                }
            }
        }

        [KSPAction("Retract")]
        public void retract(KSPActionParam param)
        {
            if ( Rideheight>0)
            {
                Rideheight -= 0.5f;
                print("Retracting");  
            }
        }//End Retract

        [KSPAction("Extend")]
        public void extend(KSPActionParam param)
        {
            if (Rideheight<8)
            {
                Rideheight += 0.5f;
                print("Extending");
            }
        }//end Deploy
    }//end class
} //end namespace
