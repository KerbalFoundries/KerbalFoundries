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
        public bool isSprocket;
        [KSPField]
        public bool isIdler;
        public WheelCollider wheelCollider;
        public Transform susTrav;
        public Transform wheel;
        public AlphaModuleTrack track;
        public Vector3 initialTraverse;
        public float lastTempTraverse;
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
                lastTempTraverse = initialTraverse.y; //sets it to a default value for the sprockets
            }
            //end find named objects
            base.OnStart(state);
        }//end OnStart
        //OnUpdate
        public override void OnUpdate()
        {
            base.OnUpdate();
            wheel.transform.Rotate(Vector3.right, track.degreesPerTick / wheelCollider.radius); //rotate wheel
            //suspension movement
            WheelHit hit;
            Vector3 tempTraverse = initialTraverse;
            bool grounded = wheelCollider.GetGroundHit(out hit); //set up to pass out wheelhit coordinates
            if (grounded && !isSprocket) //is it on the ground
            {
                tempTraverse.y -= (-wheelCollider.transform.InverseTransformPoint(hit.point).y + track.raycastError) - wheelCollider.radius;// / wheelCollider.suspensionDistance; //out hit does not take wheel radius into account
                lastTempTraverse = tempTraverse.y;
            }
            else
            {
                tempTraverse.y = lastTempTraverse;
            } //movement defaults back to zero when not grounded
            susTrav.transform.localPosition = tempTraverse; //move the suspensioTraverse object

            //end suspension mvoement
        }//end OnUpdate
    }//end modele
}//end class