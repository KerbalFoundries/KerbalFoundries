using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KerbalFoundries
{
    [KSPModule("WheelTweaks")]
    public class WheelTweaks : PartModule
    {
        public WheelCollider thiswheelCollider;        //container for wheelcollider we grab from wheelmodule
        public WheelCollider mywc;
        public JointSpring userspring;
        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Height"), UI_FloatRange(minValue = 0, maxValue = 2.00f, stepIncrement = 0.25f)]
        public float Rideheight;        //this is what's tweaked by the line above
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Strength"), UI_FloatRange(minValue = 0, maxValue = 3.00f, stepIncrement = 0.2f)]
        public float SpringRate;        //this is what's tweaked by the line above
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Damping"), UI_FloatRange(minValue = 0, maxValue = 1.00f, stepIncrement = 0.025f)]
        public float DamperRate;        //this is what's tweaked by the line above
        [KSPField(isPersistant = true)]
        public bool deployed = true;

        [KSPField(isPersistant = true)]
        public bool reverseMotorSet = false;
        [KSPField(isPersistant = true)]
        public bool reverseMotor = false;
        //steeringstuff
        public Transform steeringFound;
        public Transform smoothSteering;
        public float smoothSpeed = 40f;
//begin start
        public override void OnStart(PartModule.StartState start)  //when started
        {
            thiswheelCollider = part.gameObject.GetComponentInChildren<WheelCollider>();   //find the 'wheelCollider' gameobject named by KSP convention.
            mywc = thiswheelCollider.GetComponent<WheelCollider>();
            userspring = mywc.suspensionSpring;
            // degub only: print("onstart"); 
            base.OnStart(start);


            if (HighLogic.LoadedSceneIsEditor)
            {
                if (SpringRate == 0) //check if a value exists already. This is important, because if a wheel has been tweaked from the default value, we will overwrite it!
                {
                    //set up jointspring to modify spring property
                    SpringRate = mywc.suspensionSpring.spring;                                    //pass to springrate to be used in the GUI
                    DamperRate = mywc.suspensionSpring.damper;
                    Rideheight = mywc.suspensionDistance;
                }
            }


            if (HighLogic.LoadedSceneIsFlight)
            {

                if (SpringRate == 0) //check if a value exists already. This is important, because if a wheel has been tweaked from the default value, we will overwrite it!
                {
                     //set up jointspring to modify spring property
                    SpringRate = mywc.suspensionSpring.spring;                                    //pass to springrate to be used in the GUI
                    DamperRate = mywc.suspensionSpring.damper;
                    Rideheight = mywc.suspensionDistance;
                }
                else
                {
                    userspring.spring = SpringRate;
                    userspring.damper = DamperRate;
                    mywc.suspensionSpring = userspring;
                    mywc.suspensionDistance = Rideheight;
                }
            }//end flight
        }//end start
    }
}
