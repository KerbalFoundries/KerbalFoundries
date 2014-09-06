using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KerbalFoundries
{
    [KSPModule("AlphaTrackWheel")]
    public class AlphaTrackWheel : PartModule
    {
        //start variables
        [KSPField]
        public string wheelName;
        [KSPField]
        public string colliderName;
        [KSPField]
        public string sustravName;
        [KSPField]
        public bool useDirectionCorrector = false; //make sure it's set to false if not specified in the config
        [KSPField]
        public bool isSprocket;
        [KSPField]
        public bool isIdler;
        public WheelCollider wheelCollider;
        public Transform susTrav;
        public Transform wheel;
        public AlphaModuleTrack track;
        public Vector3 initialTraverse;
        public float lastTempTraverse;
        [KSPField]
        public float rotationCorrection = 1;

        [KSPField]
        public float rotationX = 1;
        [KSPField]
        public float rotationY = 0;
        [KSPField]
        public float rotationZ = 0;
        [KSPField]
        public string susTravAxis = "Y";

        int susTravIndex = 1;

        public int directionCorrector = 1;
        //end variables

        //OnStart
        public override void OnStart(PartModule.StartState state)
        {
            print("TrackWheel Called");
            if (HighLogic.LoadedSceneIsEditor)
            {

            }
            if (HighLogic.LoadedSceneIsFlight)
            {
                //find names onjects in part
                this.part.force_activate();

                susTravIndex = Extensions.SetAxisIndex(susTravAxis);


                foreach (WheelCollider wc in this.part.GetComponentsInChildren<WheelCollider>())
                {
                    if (wc.name.Equals(colliderName, StringComparison.Ordinal))
                    {
                        wheelCollider = wc;
                    }
                }
                foreach (Transform tr in this.part.GetComponentsInChildren<Transform>())
                {
                    if (tr.name.Equals(sustravName, StringComparison.Ordinal))
                    {
                        susTrav = tr;
                    }
                }
                foreach (Transform tr in this.part.GetComponentsInChildren<Transform>())
                {
                    if (tr.name.Equals(wheelName, StringComparison.Ordinal))
                    {
                        wheel = tr;
                    }
                }
                track = this.part.GetComponentInChildren<AlphaModuleTrack>();

                initialTraverse = susTrav.transform.localPosition;
                lastTempTraverse = initialTraverse[susTravIndex] - wheelCollider.suspensionDistance; //sets it to a default value for the sprockets and wheels
                if (useDirectionCorrector)
                    directionCorrector = track.directionCorrector;
                else directionCorrector = 1;
                print(directionCorrector);
            }
            //end find named objects
            base.OnStart(state);
        }//end OnStart
        //OnUpdate
        public override void OnUpdate()
        {
            base.OnUpdate();
            wheel.transform.Rotate(new Vector3 (rotationX,rotationY,rotationZ), track.degreesPerTick / wheelCollider.radius * directionCorrector * rotationCorrection); //rotate wheel
            //suspension movement
            WheelHit hit;
            Vector3 tempTraverse = initialTraverse;
            bool grounded = wheelCollider.GetGroundHit(out hit); //set up to pass out wheelhit coordinates
            if (grounded && !isSprocket) //is it on the ground
            {
                tempTraverse[susTravIndex] -= (-wheelCollider.transform.InverseTransformPoint(hit.point).y + track.raycastError) - wheelCollider.radius;// / wheelCollider.suspensionDistance; //out hit does not take wheel radius into account
                lastTempTraverse = tempTraverse[susTravIndex];
            }
            else
            {
                tempTraverse[susTravIndex] = lastTempTraverse;
            } //movement defaults back to zero when not grounded
            susTrav.transform.localPosition = tempTraverse; //move the suspensioTraverse object

            //end suspension mvoement
        }//end OnUpdate
    }//end modele
}//end class