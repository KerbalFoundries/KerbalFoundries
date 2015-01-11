/*
 * KSP [0.23.5] Anti-Grav Repulsor plugin by Lo-Fi
 * Much inspiration and a couple of code snippets for deployment taken from BahamutoD's Critter Crawler mod. Huge respect, it's a fantastic mod :)
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KerbalFoundries
{
    public class RepulsorWheel : PartModule
    {
        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Repulsor Height %"), UI_FloatRange(minValue = 0, maxValue = 100, stepIncrement = 5)]
        public float repulsorHeight = 50;

        float replusorHeightMultiplier = 5;

        [KSPField(isPersistant = true)]
        public bool repulsorMode = false;

        //float repulsorDistance = 8;
        [KSPField]
        public float chargeConsumptionRate = 1;

        float effectPower;
        float effectPowerMax;
        bool lowEnergy = false;

        List<WheelCollider> wcList = new List<WheelCollider>();
        List<float> wfForwardList = new List<float>();
        List<float> susDistList = new List<float>();
        List<float> wfSideList = new List<float>();

        //ModuleWaterSlider mws;
        KFModuleWheel _moduletrack;

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
                    ma.Events["Toggle"].guiActiveEditor = false;
                }
            }

            if(HighLogic.LoadedSceneIsFlight)
            {
                print("Repulsor Wheel started");

                foreach (ModuleAnimateGeneric ma in this.part.FindModulesImplementing<ModuleAnimateGeneric>())
                {
                    ma.Events["Toggle"].guiActive = false;
                }

                _moduletrack = this.part.GetComponentInChildren<KFModuleWheel>();
                _moduletrack.Events["ApplySettings"].guiActive = false;
                
                //this.part.force_activate();
                //mws = this.vessel.FindPartModulesImplementing<ModuleWaterSlider>().SingleOrDefault();

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
                effectPowerMax = 1 * chargeConsumptionRate * Time.deltaTime;
                print(effectPowerMax);
            }//end isInFlight
        }//end start

        public override void OnUpdate()
        {
            base.OnUpdate();
            if (repulsorMode)
            {
                float chargeConsumption = (repulsorHeight / 2) * (1 + _moduletrack.springRate) * Time.deltaTime * chargeConsumptionRate;
                effectPower = chargeConsumption / effectPowerMax;
                print(effectPower);

                float electricCharge = part.RequestResource("ElectricCharge", chargeConsumption);
                //print(electricCharge);
                // = Extensions.GetBattery(this.part);
                if (electricCharge < (chargeConsumption * 0.9f))
                {
                    print("Retracting due to low Electric Charge");
                    lowEnergy = true;
                    repulsorHeight = 0;
                    UpdateColliders("wheel");
                    _moduletrack.status = "Low Charge";
                }
                else
                {
                    lowEnergy = false;
                    _moduletrack.status = "Nominal";
                }
            }
            else
            {
                effectPower = 0;
            }
            RepulsorSound();
            effectPower = 0;    //reset to make sure it doesn't play when it shouldn't.
            //print(effectPower);
        }

        public void RepulsorSound()
        {
            part.Effect("RepulsorEffect", effectPower);
        }

        public void UpdateColliders(string mode)
        {
            if (mode == "repulsor")
            {
                //mws.colliderHeight = 3f;
                if(lowEnergy)
                    return;
                _moduletrack.UpdateColliders("retract");
                _moduletrack.currentRideHeight = repulsorHeight * replusorHeightMultiplier;
                repulsorMode = true;
                
                for(int i = 0; i < wcList.Count(); i++)
                {
                    //wcList[i].suspensionDistance = wcList[i].suspensionDistance * 2f;
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
                //mws.colliderHeight = 10;
                _moduletrack.currentRideHeight = _moduletrack.rideHeight;
                _moduletrack.smoothedRideHeight = _moduletrack.currentRideHeight;
                _moduletrack.UpdateColliders("deploy");

                repulsorMode = false;
 
                for (int i = 0; i < wcList.Count(); i++)
                {
                    //wcList[i].suspensionDistance = susDistList[i];
                    WheelFrictionCurve wf = wcList[i].forwardFriction;
                    wf.stiffness = wfForwardList[i];
                    wcList[i].forwardFriction = wf;
                    wf = wcList[i].sidewaysFriction;
                    wf.stiffness = wfSideList[i];
                    wcList[i].sidewaysFriction = wf;
                    //wcList[i].enabled = false;
                    //wcList[i].enabled = true;
                }
            }
            
            else
            {
                print("incorrect command passed to UpdateColliders");
            }
        }

        [KSPEvent(guiActive = true, guiName = "Apply Repulsor Settings", active = true)]
        public void ApplySettings()
        {
            foreach (KFModuleWheel mt in this.vessel.FindPartModulesImplementing<KFModuleWheel>())
            {
                //if (!_moduletrack.isRetracted)   //normally the button would be disabled in the wheel is retracted.
                //    _moduletrack.ApplySettings(); 
                if (_moduletrack.groupNumber != 0 && _moduletrack.groupNumber == mt.groupNumber)
                {
                    if (repulsorMode)               //similiarly, only apply settings when inreplusor mode.
                    {
                        _moduletrack.currentRideHeight = repulsorHeight * replusorHeightMultiplier;
                        mt.currentRideHeight = repulsorHeight * replusorHeightMultiplier;
                    }
                }
            }
            foreach (RepulsorWheel rw in this.vessel.FindPartModulesImplementing<RepulsorWheel>())
            {
                    rw.repulsorHeight = repulsorHeight;
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

        [KSPAction("Wheel Mode")]
        public void toWheel(KSPActionParam param)
        {
            if (repulsorMode == true)
            {
                PlayAnimation();
                repulsorMode = false;
                UpdateColliders("wheel");
            }

        }//end Deploy All

        [KSPAction("Repulsor Mode")]
        public void toRepulsor(KSPActionParam param)
        {
            if (lowEnergy)
                return;
                if (repulsorMode == false)
                {
                    PlayAnimation();
                    repulsorMode = true;
                    UpdateColliders("repulsor");
                }
        }//end Deploy All
    }//end class
} //end namespace
