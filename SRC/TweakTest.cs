using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KerbalFoundries
{
    [KSPModule("TweakTest")]
    public class TweakTest : PartModule
    {
        public WheelCollider thiswheelCollider;        //container for wheelcollider we grab from wheelmodule
        public WheelCollider mywc;
        public JointSpring userspring;
        public JointSpring thisSpring;

        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Height"), UI_FloatRange(minValue = 0, maxValue = 2.00f, stepIncrement = 0.25f)]
        public float Rideheight;        //this is what's tweaked by the line above
        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Strength"), UI_FloatRange(minValue = 0, maxValue = 3.00f, stepIncrement = 0.2f)]
        public float SpringRate;        //this is what's tweaked by the line above
        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Damping"), UI_FloatRange(minValue = 0, maxValue = 1.00f, stepIncrement = 0.025f)]
        public float DamperRate;        //this is what's tweaked by the line above
        [KSPField(isPersistant = true)]
        public bool deployed = true;

        //steeringstuff
        public Transform steeringFound;
        public Transform smoothSteering;
        public Transform Arrow;
        public float smoothSpeed = 40f;

        //begin start
        public override void OnStart(PartModule.StartState start)  //when started
        {
            // degub only: print("onstart");
            this.part.force_activate();
            base.OnStart(start);
            //thiswheelCollider = part.gameObject.GetComponentInChildren<WheelCollider>();   //find the 'wheelCollider' gameobject named by KSP convention.
            mywc = this.part.GetComponentInChildren<WheelCollider>();     //pull collider properties

            userspring = mywc.suspensionSpring;         //set up jointspring to modify spring property

            if (HighLogic.LoadedSceneIsEditor)
            {
                if (SpringRate == 0) //check if a value exists already. This is important, because if a wheel has been tweaked from the default value, we will overwrite it!
                {
                    SpringRate = userspring.spring;                                    //pass to springrate to be used in the GUI
                    DamperRate = userspring.damper;
                    Rideheight = mywc.suspensionDistance;
                }

            }

            if (HighLogic.LoadedSceneIsFlight)
            {
                steeringFound = transform.Search("steering");
                smoothSteering = transform.Search("smoothSteering");

            }//end flight




            if (SpringRate == 0) //check if a value exists already. This is important, because if a wheel has been tweaked from the default value, we will overwrite it!
            {
                SpringRate = userspring.spring;                                    //pass to springrate to be used in the GUI
                DamperRate = userspring.damper;
                Rideheight = mywc.suspensionDistance;
            }
            else
            {
                userspring.spring = SpringRate;
                userspring.damper = DamperRate;
                mywc.suspensionSpring = userspring;
                mywc.suspensionDistance = Rideheight;
            }
        }//end start

        public override void OnFixedUpdate()
        {
            smoothSteering.transform.rotation = Quaternion.Lerp(steeringFound.transform.rotation, smoothSteering.transform.rotation, Time.deltaTime * smoothSpeed);

            // Arrow.transform.LookAt(FlightGlobals.currentMainBody.bodyTransform, Vector3.forward);

        }// end OnFixedUpdate
    }//end class
}//end namespacesa
