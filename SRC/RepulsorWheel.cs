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
    public class RepulsorWheel : PartModule
    {

        [KSPField(isPersistant = true)]
        public bool repulsorMode = false;
        //forward friction values
        [KSPField]
        public string wheelCollider;

        public List<WheelCollider> wcList = new List<WheelCollider>();
        public List<WheelCollider> rcList = new List<WheelCollider>();

        //begin start
        public override void OnStart(PartModule.StartState start)  //when started
        {

            base.OnStart(start);
            
            if (HighLogic.LoadedSceneIsEditor)
            {
                foreach (ModuleAnimateGeneric ma in this.part.FindModulesImplementing<ModuleAnimateGeneric>())
                {
                    ma.Actions["ToggleAction"].active = false;
                }
            }

            if(HighLogic.LoadedSceneIsFlight)
            {
                print("Repulsor Wheel started");
                //this.part.force_activate();

                FindRepulsors(wcList, rcList);
                var MT = this.part.GetComponentInChildren<ModuleTrack>();
                MT.wcList = wcList; //override the list gernetated in ModuleTrack
                MT.wheelCount = 1;

                if (repulsorMode == true) //is the deployed flag set? set the rideheight appropriately
                {
                    UpdateColliders("repulsor");
                }
                if (repulsorMode == false)
                {
                    UpdateColliders("wheel"); 
                }
            }//end isInFlight
        }//end start

        public void FindRepulsors(List<WheelCollider> wheelList, List<WheelCollider> repulsorList)
        {
            foreach (WheelCollider wc in this.part.GetComponentsInChildren<WheelCollider>()) //set colliders to values chosen in editor and activate
            {
                print("Finding wheel colliders"); 
                print(wc.name);
                if (!wc.name.Equals(wheelCollider, StringComparison.Ordinal))
                {
                    repulsorList.Add(wc); 
                }
                if (wc.name.Equals(wheelCollider, StringComparison.Ordinal))
                {
                    wheelList.Add(wc);
                }
            }
        }


        public void UpdateColliders(string mode)
        {
            if (mode == "repulsor")
            {
                foreach (WheelCollider wc in rcList)
                {
                    wc.enabled = true;
                }
                foreach (WheelCollider wc in wcList)
                {
                    wc.enabled = false;
                }
            }
            else if (mode == "wheel")
            {
                foreach (WheelCollider wc in rcList)
                {
                    wc.enabled = false;
                }
                foreach (WheelCollider wc in wcList)
                {
                    wc.enabled = true;
                }
            }
            else //default to wheel
            {
                foreach (WheelCollider wc in rcList)
                {
                    wc.enabled = false;
                }
                foreach (WheelCollider wc in wcList)
                {
                    wc.enabled = true;
                }
            }

        }
    


        [KSPAction("Toggle Modes")]
        public void AGToggleDeployed(KSPActionParam param)
        {
            if (repulsorMode)
            {
                toWheel(param);
            }
            else
            {
                toRepulsor(param);
            }
        }//End Deploy toggle
       
        public void PlayAnimation()
        {
            // note: assumes one ModuleAnimateGeneric (or derived version) for this part
            // if this isn't the case, needs fixing
            ModuleAnimateGeneric myAnimation = part.FindModulesImplementing<ModuleAnimateGeneric>().SingleOrDefault();
            if (!myAnimation)
            {
                return; //the Log.Error line fails syntax check with 'The name 'Log' does not appear in the current context.
            }
            else
            {
                    myAnimation.Toggle();

            }

        }

        [KSPAction("Repulsor Mode")]
        public void toWheel(KSPActionParam param)
        {
            //print(thisTransform);
            // note: this loop will find "us" too. Intended
            if (repulsorMode == true)
            {

                PlayAnimation();
                repulsorMode = false;
                UpdateColliders("wheel");
            }


        }//end Deploy All

        [KSPAction("Wheel Mode")]
        public void toRepulsor(KSPActionParam param)
        {
                if (repulsorMode == false)
                {
                    PlayAnimation();
                    repulsorMode = true;
                    UpdateColliders("repulsor");
                }
        }//end Deploy All
    }//end class
} //end namespace
