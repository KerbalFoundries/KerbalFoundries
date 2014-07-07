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

    [KSPModule("SmoothSteering")]
    public class SmoothSteering : PartModule
    {
        public Transform steeringFound;
        public Transform smoothSteering;
        public float smoothSpeed = 40f;

        public override void OnStart(PartModule.StartState start)  //when started
        {
            base.OnStart(start);

            if (HighLogic.LoadedSceneIsFlight)
            {
                this.part.force_activate();
                steeringFound = transform.Search("steering");
                smoothSteering = transform.Search("smoothSteering");
            }
        }
        public override void OnFixedUpdate()
        {
            //    Vector3 tempRotation = steeringFound.localEulerAngles;
            //    print(tempRotation);
            //arrowFound.localEulerAngles = steeringFound.localEulerAngles;
            smoothSteering.transform.rotation = Quaternion.Lerp(steeringFound.transform.rotation, smoothSteering.transform.rotation, Time.deltaTime * smoothSpeed);
        }

    }//end class
} //end namespace
