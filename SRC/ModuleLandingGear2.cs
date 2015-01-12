using System;
using System.Linq;
using UnityEngine;

namespace KerbalFoundries
{
    [KSPModule("ModuleLandingGear2")]
    public class ModuleLandingGear2 : PartModule
    {
        public bool boundsDestroyed;
        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Steering"), UI_Toggle(disabledText = "Enabled", enabledText = "Disabled")]
        public bool steeringDisabled;
        [KSPField(isPersistant = true)]
        public bool brakesApplied;
        [KSPField(isPersistant = true)]
        public float brakeTorque;
        [KSPField]
        public float brakingTorque;
        [KSPField]
        public FloatCurve steeringCurve = new FloatCurve();
        public Transform Steering;
        [KSPField]
        public string steeringName;
        public float smoothSpeed = 40f;
        public float steeringAngle;

        public Vector3 initialSteering;

        public override void OnStart(PartModule.StartState start)  //when started
        {
            print("ModuleLandingGear called");
            base.OnStart(start);

            foreach (ModuleAnimateGeneric ma in this.part.FindModulesImplementing<ModuleAnimateGeneric>())
            {
                ma.Events["Toggle"].guiActive = false;
            }

            foreach (Transform tr in this.part.GetComponentsInChildren<Transform>())
            {
                if (tr.name.Equals(steeringName, StringComparison.Ordinal))
                {
                    Steering = tr;
                }
            }
            initialSteering = Steering.transform.localEulerAngles;

            if (HighLogic.LoadedSceneIsEditor)
            {
            }

            if (HighLogic.LoadedSceneIsFlight)
            {
                this.part.force_activate();
                foreach (WheelCollider wc in this.part.GetComponentsInChildren<WheelCollider>())
                {
                    wc.enabled = true;
                }
            }
                
            Transform bounds = transform.Search("Bounds");
            if (bounds != null)
            {
                GameObject.Destroy(bounds.gameObject);
                //boundsDestroyed = true; //remove the bounds object to let the wheel colliders take over
                print("destroying Bounds");
            }
        }

        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
            foreach (WheelCollider wc in this.part.GetComponentsInChildren<WheelCollider>())
            {
                wc.brakeTorque = brakeTorque;
            }
            steeringAngle = Mathf.Lerp(steeringCurve.Evaluate((float)this.vessel.srfSpeed) * this.vessel.ctrlState.wheelSteer, steeringAngle, Time.deltaTime * smoothSpeed);
            Vector3 tempSteering = initialSteering;
            tempSteering.y -= steeringAngle;
            //print(steeringAngle);
            //print(brakeTorque);
            Steering.transform.localEulerAngles = tempSteering;
        }

        public void PlayAnimation()
        {
            ModuleAnimateGeneric myAnimation = part.FindModulesImplementing<ModuleAnimateGeneric>().SingleOrDefault();
            if (!myAnimation)
            {
                print("No animation found");
                return; 
            }
            else
            {
                print("Playing");
                myAnimation.Toggle();
            }
        }

        // I noticed that "Gear" and "brakes" were swapped, so I changed the functions around so it makes sense. - Gaalidas
        [KSPAction("Gear", KSPActionGroup.Gear)]
        public void Gear(KSPActionParam param)
        {
                PlayAnimation();
        }

        [KSPAction("Brakes", KSPActionGroup.Brakes)]
        public void brakes(KSPActionParam param)
        {
            if (param.type == KSPActionType.Activate)
            {
                brakeTorque = brakingTorque;
                brakesApplied = true;
            }
            else
            {
                brakeTorque = 0;
                brakesApplied = false;
            }
        }
    }
}
