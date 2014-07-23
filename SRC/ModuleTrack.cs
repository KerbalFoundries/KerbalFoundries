using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KerbalFoundries
{
    [KSPModule("ModuleTrack")]
    public class ModuleTrack : PartModule
    {
//variable setup 
        public int directionCorrector;
        public float motorTorque;
        public float numberOfWheels = 1; //if it's 0 at the start it send things into and NaN fit.
        public float trackRPM = 0;
        [KSPField]
        public float trackLength = 100;
        [KSPField]
        public FloatCurve torqueCurve = new FloatCurve();
        [KSPField]
        public FloatCurve steeringCurve = new FloatCurve();
        [KSPField]
        public float brakingTorque;
        public float brakeTorque;

        public GameObject trackSurface;

        [KSPField]
        public float raycastError;

        public float degreesPerTick;
//tweakables
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Torque %"), UI_FloatRange(minValue = 20, maxValue = 100f, stepIncrement = 25f)]
        public float torque = 100;
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Strength"), UI_FloatRange(minValue = 0, maxValue = 3.00f, stepIncrement = 0.2f)]
        public float springRate;        //this is what's tweaked by the line above
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Damping"), UI_FloatRange(minValue = 0, maxValue = 0.2f, stepIncrement = 0.025f)]
        public float damperRate;        //this is what's tweaked by the line above
//end twekables
        
//end variable setup

        public override void OnStart(PartModule.StartState start)  //when started
        {
            print("ModuleTrack called");
            base.OnStart(start);
            if (HighLogic.LoadedSceneIsEditor)
            {

            }

            if (HighLogic.LoadedSceneIsFlight)
            {
                torque /= 100;
                this.part.force_activate(); //force the part active or OnFixedUpate is not called
                foreach (WheelCollider wc in this.part.GetComponentsInChildren<WheelCollider>())
                {
                    JointSpring userSpring = wc.suspensionSpring;
                    userSpring.spring = springRate;
                    userSpring.damper = damperRate;
                    wc.suspensionSpring = userSpring;
                    wc.enabled = true;
                }

                if (this.part.orgPos.x < 0)
                {
                    directionCorrector = 1;
                }
                else
                {
                    directionCorrector = -1;
                }

                foreach(SkinnedMeshRenderer potentialTrackOrWheel in this.part.GetComponentsInChildren<SkinnedMeshRenderer>())
                {
                    trackSurface = potentialTrackOrWheel.gameObject;
                }
            }
       }//end OnStart



        public override void OnUpdate()
        {
//User input
            if (this.vessel.ctrlState.wheelThrottle > 0)
            {
                motorTorque = torqueCurve.Evaluate((float)this.vessel.srfSpeed) * directionCorrector * torque; //calculate foce = how fast are we going, which direction is the part facing, scale for torque setting
            }
            if (this.vessel.ctrlState.wheelThrottle < 0)
            {
                motorTorque = -torqueCurve.Evaluate((float)this.vessel.srfSpeed) * directionCorrector * torque;
            }
            if (this.vessel.ctrlState.wheelSteer > 0)
            {
                motorTorque -= steeringCurve.Evaluate((float)this.vessel.srfSpeed) * torque;
            }

            if (this.vessel.ctrlState.wheelSteer < 0)
            {
                motorTorque += steeringCurve.Evaluate((float)this.vessel.srfSpeed) * torque;
            }
//end user input
//Apply to WheelColliders
            foreach (WheelCollider wc in this.part.GetComponentsInChildren<WheelCollider>())
            {
                wc.motorTorque = motorTorque;
                wc.brakeTorque = brakeTorque;
                if(wc.isGrounded) //only use colliders that are grounded for average RPM calculation. Those off the floor spinng freely mess it up!
                {
                    numberOfWheels++;
                    trackRPM += wc.rpm;
                }
            }
            trackRPM /= numberOfWheels;

            degreesPerTick = (trackRPM / 60) * Time.deltaTime * 360; //calculate how many degrees to rotate the wheel

            float distanceTravelled = (float)((trackRPM * 2 * Math.PI) / 60) * Time.deltaTime; //calculate how far the track will need to move
            Material trackMaterial = trackSurface.renderer.material;    //set things up for changing the texture offset on the track
            Vector2 textureOffset = trackMaterial.mainTextureOffset;
            textureOffset = textureOffset + new Vector2(-distanceTravelled/trackLength, 0); //tracklength is use to fine tune the speed of movement.
            trackMaterial.SetTextureOffset("_MainTex", textureOffset);
            trackMaterial.SetTextureOffset("_BumpMap", textureOffset);
            motorTorque = 0; //reset motortorque
            numberOfWheels = 1; //reset number of wheels. Setting to zero gives NaN!
            trackRPM = 0;
        } //end OnUpdate
//Action groups
        [KSPAction("Brakes", KSPActionGroup.Brakes)]
        public void brakes(KSPActionParam param)
        {
            if (param.type == KSPActionType.Activate)
            {
                brakeTorque = brakingTorque;
            }
            else
            {
                brakeTorque = 0;
            }
        }
//end action groups
    }//end class
}//end namespace
