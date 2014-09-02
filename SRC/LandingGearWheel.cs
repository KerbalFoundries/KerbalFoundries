using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KerbalFoundries
{
    [KSPModule("LandingGearWheel")]
    public class LandingGearWheel : PartModule
    {
        [KSPField]
        public string wheelName;
        [KSPField]
        public string colliderName;
        [KSPField]
        public string susTravName;
        [KSPField]
        public bool isSuspension;
        [KSPField]
        public float rotationX = 1;
        [KSPField]
        public float rotationY = 0;
        [KSPField]
        public float rotationZ = 0;
        [KSPField]
        public string susTravAxis = "Y";

        int susTravIndex = 1;

        WheelCollider wheelCollider;
        Transform susTrav;
        Transform wheel;
        Vector3 initialTraverse;
        float lastTempTraverse;

        public override void OnStart(PartModule.StartState state)
        {
            foreach (WheelCollider wc in this.part.GetComponentsInChildren<WheelCollider>())
            {
                if (wc.name.Equals(colliderName, StringComparison.Ordinal))
                {
                    wheelCollider = wc;
                }
            }
            foreach (Transform tr in this.part.GetComponentsInChildren<Transform>())
            {
                if (tr.name.Equals(susTravName, StringComparison.Ordinal))
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

            print("LandingGearWheel Called");
            if (HighLogic.LoadedSceneIsEditor)
            {

            }
            if (HighLogic.LoadedSceneIsFlight)
            {
                //find names onjects in part
                this.part.force_activate();

                initialTraverse = susTrav.transform.localPosition;
                lastTempTraverse = initialTraverse[susTravIndex] - wheelCollider.suspensionDistance - 0.035f; //sets it to a default value for the sprockets

                susTravIndex = Extensions.SetAxisIndex(susTravAxis);

            }
            //end find named objects
            base.OnStart(state);
        }//end OnStart

        public override void OnUpdate() 
        {
            base.OnUpdate(); 
            WheelHit hit;
            Vector3 tempTraverse = initialTraverse;
            bool grounded = wheelCollider.GetGroundHit(out hit); //set up to pass out wheelhit coordinates
            if (grounded) //is it on the ground
            {
                tempTraverse[susTravIndex] -= (-wheelCollider.transform.InverseTransformPoint(hit.point).y) - wheelCollider.radius;// / wheelCollider.suspensionDistance; //out hit does not take wheel radius into account
                lastTempTraverse = tempTraverse[susTravIndex];
            }
            else
            {
                tempTraverse[susTravIndex] = lastTempTraverse;
            } //movement defaults back to zero when not grounded
            //print(tempTraverse.y);
            susTrav.transform.localPosition = tempTraverse; //move the suspensioTraverse object
            float degreesPerTick = (wheelCollider.rpm / 60) * Time.deltaTime * 360;
          
            wheel.transform.Rotate(new Vector3(rotationX,rotationY,rotationZ), degreesPerTick / wheelCollider.radius); //rotate wheel
        }
    }
}
