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

        List<WheelCollider> wcList = new List<WheelCollider>();
        List<float> wfForwardList = new List<float>();
        List<float> susDistList = new List<float>();
        List<float> wfSideList = new List<float>();

        ModuleWaterSlider mws;
        

        //begin start
        public override void OnStart(PartModule.StartState start)  //when started
        {

            base.OnStart(start);
            
            if (HighLogic.LoadedSceneIsEditor)
            {
                foreach (ModuleAnimateGeneric ma in this.part.FindModulesImplementing<ModuleAnimateGeneric>())
                {
                    ma.Actions["ToggleAction"].active = false;
                    ma.Events["Toggle"].guiActive = false;
                }
            }

            if(HighLogic.LoadedSceneIsFlight)
            {
                print("Repulsor Wheel started");

                foreach (ModuleAnimateGeneric ma in this.part.FindModulesImplementing<ModuleAnimateGeneric>())
                {
                    ma.Events["Toggle"].guiActive = false;
                }
                //this.part.force_activate();
                mws = this.vessel.FindPartModulesImplementing<ModuleWaterSlider>().SingleOrDefault();

                //FindRepulsors(wcList, rcList);
                //MT = this.part.GetComponentInChildren<ModuleTrack>();
                //AR = this.part.GetComponentInChildren<AlphaRepulsor>();
                //wcList = MT.wcList; 
                //MT.wheelCount = wcList.Count();
                //AR.wcList = rcList;
                //AR.repulsorCount = rcList.Count();

                foreach(WheelCollider wc in this.part.GetComponentsInChildren<WheelCollider>())
                {
                    wcList.Add(wc);
                }

                for (int i = 0; i < wcList.Count(); i++)
                {
                    wfForwardList.Add(wcList[i].forwardFriction.stiffness);
                    wfSideList.Add(wcList[i].sidewaysFriction.stiffness);
                    susDistList.Add(wcList[i].suspensionDistance);
                }

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

        public void UpdateColliders(string mode)
        {
            if (mode == "repulsor")
            {
                mws.colliderHeight = 3f;
                for(int i = 0; i < wcList.Count(); i++)
                {
                    wcList[i].suspensionDistance = wcList[i].suspensionDistance * 2f;
                    WheelFrictionCurve wf = wcList[i].forwardFriction;
                    wf.stiffness = 0;
                    wcList[i].forwardFriction = wf;
                    wf = wcList[i].sidewaysFriction;
                    wf.stiffness = 0;
                    wcList[i].sidewaysFriction = wf;
                }
            }
            else if (mode == "wheel")
            {
                mws.colliderHeight = 10;
                for (int i = 0; i < wcList.Count(); i++)
                {
                    wcList[i].suspensionDistance = susDistList[i];
                    WheelFrictionCurve wf = wcList[i].forwardFriction;
                    wf.stiffness = wfForwardList[i];
                    wcList[i].forwardFriction = wf;
                    wf = wcList[i].sidewaysFriction;
                    wf.stiffness = wfSideList[i];
                    wcList[i].sidewaysFriction = wf;
                }
            }

            else
            {
                //do nothing
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
