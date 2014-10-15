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
        [KSPField(isPersistant = false, guiActive = true, guiName = "Repulsor Settings")]
        public string settings = "";
        [KSPField(isPersistant = false, guiActive = true, guiName = "Status")]
        public string status = "Nominal";
        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Group Number"), UI_FloatRange(minValue = 0, maxValue = 10f, stepIncrement = 1f)]
        public float groupNumber = 1;
        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Height"), UI_FloatRange(minValue = 0, maxValue = 100f, stepIncrement = 5f)]
        public float Rideheight = 25;        //this is what's tweaked by the line above
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Strength"), UI_FloatRange(minValue = 0, maxValue = 3.00f, stepIncrement = 0.2f)]
        public float SpringRate;        //this is what's tweaked by the line above
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Damping"), UI_FloatRange(minValue = 0, maxValue = 0.2f, stepIncrement = 0.025f)]
        public float DamperRate;        //this is what's tweaked by the line above
        [KSPField]
        public bool deployed;
        [KSPField]
        public bool lowEnergy;

        float effectPower;

        float effectPowerMax;

        float appliedRideHeight;
        float smoothedRideHeight;
        float currentRideHeight;

        float maxRepulsorHeight = 8;

        public float repulsorCount = 0;
        [KSPField]
        public float chargeConsumptionRate = 1f;
        //begin start
        public List<WheelCollider> wcList = new List<WheelCollider>();
        //public List<float> susDistList = new List<float>();

        public override void OnStart(PartModule.StartState start)  //when started
        {
            // degub only: print("onstart");
            base.OnStart(start);
            print(Version.versionNumber);

            if (HighLogic.LoadedSceneIsGame)
            {

            }

            if (HighLogic.LoadedSceneIsEditor)
            {

            }

            if (HighLogic.LoadedSceneIsFlight)
            {

                foreach (WheelCollider b in this.part.GetComponentsInChildren<WheelCollider>())
                {
                    repulsorCount += 1;
                    userspring = b.suspensionSpring;
                    userspring.spring = SpringRate;
                    userspring.damper = DamperRate;
                    b.suspensionSpring = userspring;
                    b.suspensionDistance = Rideheight;
                    wcList.Add(b);
                }
                
                
                this.part.force_activate(); //force the part active or OnFixedUpate is not called

                currentRideHeight = Rideheight;

                UpdateCollider();

            }
            DestroyBounds();


            effectPowerMax = 1 * repulsorCount * chargeConsumptionRate * Time.deltaTime;
            //print("max effect power");
            //print(effectPowerMax);


        }//end start 
        public void DestroyBounds()
        {
            Transform bounds = transform.Search("Bounds");
            if (bounds != null)
            {
                GameObject.Destroy(bounds.gameObject);
                //boundsDestroyed = true; //remove the bounds object to let the wheel colliders take over
                print("destroying Bounds");
            }
        }

        public void RepulsorSound()
        {
            part.Effect("RepulsorEffect", effectPower);
        }

        public override void OnFixedUpdate()
        {



            if (deployed)
            {
                float chargeConsumption = (appliedRideHeight / 2) * (1 + SpringRate) * repulsorCount * Time.deltaTime * chargeConsumptionRate;
                effectPower = chargeConsumption / effectPowerMax;

                float electricCharge = part.RequestResource("ElectricCharge", chargeConsumption);
                //print(electricCharge);
                // = Extensions.GetBattery(this.part);
                if (electricCharge < (chargeConsumption * 0.9f))
                {
                    print("Retracting due to low Electric Charge");
                    lowEnergy = true;
                    Rideheight = 0;
                    UpdateCollider();
                    status = "Low Charge";
                }
                else
                {
                    lowEnergy = false;
                    status = "Nominal";
                }
            }
            else
            {
                effectPower = 0;
            }
            RepulsorSound();
            effectPower = 0;    //reset to make sure it doesn't play when it shouldn't.
            //print(effectPower);



            for (int i = 0; i < wcList.Count(); i++)
            {
                wcList[i].suspensionDistance = maxRepulsorHeight * appliedRideHeight;
            }

            smoothedRideHeight = Mathf.Lerp(smoothedRideHeight, currentRideHeight, Time.deltaTime * 2);
            appliedRideHeight = smoothedRideHeight / 100;
        }

        public void UpdateCollider()
        {
            currentRideHeight = Rideheight;
            for (int i = 0; i < wcList.Count(); i++)
            {

                if (Rideheight < 5f)
                {
                    wcList[i].enabled = false;
                    deployed = false;
                }
                else
                {
                    wcList[i].enabled = true;
                    deployed = true;
                }
            }
        }

        [KSPAction("Retract")]
        public void Retract(KSPActionParam param)
        {
            if (Rideheight > 0)
            {
                Rideheight -= 5f;
                print("Retracting");
                UpdateCollider();
            }

        }//End Retract

        [KSPAction("Extend")]
        public void Extend(KSPActionParam param)
        {
            if (Rideheight < 100)
            {
                Rideheight += 5f;
                print("Extending");
                UpdateCollider();
            }
        }//end Deploy

        [KSPEvent(guiActive = true, guiName = "Apply Settings", active = true)]
        public void ApplySettings()
        {
            foreach (AlphaRepulsor mt in this.vessel.FindPartModulesImplementing<AlphaRepulsor>())
            {
                if (groupNumber != 0 && groupNumber == mt.groupNumber)
                {
                    mt.Rideheight = Rideheight;
                    currentRideHeight = Rideheight;
                    mt.currentRideHeight = Rideheight;
                    mt.UpdateCollider();
                }
            }

            UpdateCollider();
        }
    }//end class
} //end namespace
