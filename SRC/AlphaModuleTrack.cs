using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KerbalFoundries
{
    [KSPModule("AlphaModuleTrack")]
    public class AlphaModuleTrack : PartModule
    {
        //variable setup 
        public int directionCorrector;
        public float motorTorque;
        public float numberOfWheels = 1; //if it's 0 at the start it send things into and NaN fit.
        public float trackRPM = 0;
        public float averageTrackRPM;
        [KSPField]
        public float trackLength = 100;
        [KSPField]
        public FloatCurve torqueCurve = new FloatCurve();
        [KSPField]
        public FloatCurve steeringCurve = new FloatCurve();
        [KSPField]
        public FloatCurve brakeSteeringCurve = new FloatCurve();
        [KSPField]
        public float brakingTorque;
        public float brakeTorque;
        public float brakeSteering;

        public GameObject trackSurface;

        //public Transform bounds;
        public bool boundsDestroyed;
        [KSPField(isPersistant = true)]
        public bool brakesApplied;

        [KSPField]
        public float raycastError;

        public float degreesPerTick;
        //tweakables
        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Torque ratio"), UI_FloatRange(minValue = 0, maxValue = 2f, stepIncrement = .25f)]
        public float torque = 1;
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Spring strength"), UI_FloatRange(minValue = 0, maxValue = 6.00f, stepIncrement = 0.2f)]
        public float springRate;        //this is what's tweaked by the line above
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Spring Damping"), UI_FloatRange(minValue = 0, maxValue = 1.0f, stepIncrement = 0.025f)]
        public float damperRate;        //this is what's tweaked by the line above
        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Steering"), UI_Toggle(disabledText = "Enabled", enabledText = "Disabled")]
        public bool steeringDisabled;

        public Vector3 referenceTranformVector;
        public Transform referenceTransform;
        public float myPosition;
        [KSPField(isPersistant = false, guiActive = true, guiName = "Reference Direction")]
        public string referenceDirection;

        [KSPField]
        public float buoyancy;
        [KSPField]
        public string buoyancyName;
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
                FindDirection();
                if(torque >2) //check if the torque value is using the old numbering system
                {
                    torque /= 100;
                }
                this.part.force_activate(); //force the part active or OnFixedUpate is not called
                foreach (WheelCollider wc in this.part.GetComponentsInChildren<WheelCollider>()) //set colliders to values chosen in editor and activate
                {
                    JointSpring userSpring = wc.suspensionSpring;
                    userSpring.spring = springRate;
                    userSpring.damper = damperRate;
                    wc.suspensionSpring = userSpring;
                    wc.enabled = true;
                }

                float dot = Vector3.Dot(this.part.transform.forward, referenceTranformVector); // up is forward
                if (dot < 0) // below 0 means the engine is on the left side of the craft
                {
                    directionCorrector = -1;
                    print("left");
                }
                else
                {
                    directionCorrector = 1;
                    print("right");
                }

                foreach (SkinnedMeshRenderer Track in this.part.GetComponentsInChildren<SkinnedMeshRenderer>()) //this is the track
                {
                    trackSurface = Track.gameObject;
                }

                if (brakesApplied)
                {
                    brakeTorque = brakingTorque; //were the brakes left applied
                }
                Transform bounds = transform.Search("Bounds");
                if (bounds != null)
                {
                    GameObject.Destroy(bounds.gameObject);
                    //boundsDestroyed = true; //remove the bounds object to left the wheel colliders take over
                    print("destroying Bounds");
                }

                SetupBouyancy();

            }//end scene is flight
        }//end OnStart

        public void FindDirection()
        {
            float dotx = Math.Abs(Vector3.Dot(this.part.transform.forward, vessel.ReferenceTransform.right)); // up is forward
            float doty = Math.Abs(Vector3.Dot(this.part.transform.forward, vessel.ReferenceTransform.up));
            float dotz = Math.Abs(Vector3.Dot(this.part.transform.forward, vessel.ReferenceTransform.forward));

            if (dotx > doty && dotx > dotz)
            {
                print("root part mounted sideways");
                myPosition = this.part.orgPos.x;
                referenceTranformVector = this.vessel.ReferenceTransform.right;
            }
            if (doty > dotx && doty > dotz)
            {
                print("root part mounted forward");
                myPosition = this.part.orgPos.y;
                referenceTranformVector = this.vessel.ReferenceTransform.up;
            }
            if (dotz > doty && dotz > dotx)
            {
                print("root part mounted up");
                myPosition = this.part.orgPos.z;
                referenceTranformVector = this.vessel.ReferenceTransform.forward;
            }
        }

        public void SetupBouyancy()
        {
            this.part.buoyancy = buoyancy;
            this.part.CenterOfBuoyancy = transform.Search(buoyancyName).up;
        }

        public override void OnFixedUpdate()
        {
            //User input
            float electricCharge;
            float chargeRequest;
            float forwardTorque = torqueCurve.Evaluate((float)this.vessel.srfSpeed) * torque; //this is used a lot, so may as well calculate once
            float steeringTorque;
            float brakeSteeringTorque;

            Vector3 travelVector = this.vessel.GetSrfVelocity();

            float travelDirection = Vector3.Dot(this.part.transform.forward, travelVector); //compare travel velocity with the direction the part is pointed.
            //print(travelDirection);

            if (!steeringDisabled)
            {
                steeringTorque = steeringCurve.Evaluate((float)this.vessel.srfSpeed) * torque; //low speed steering mode. Differential motor torque
                brakeSteering = brakeSteeringCurve.Evaluate(travelDirection); //high speed steering. Brake on inside track because Unity seems to weight reverse motor torque less at high speed.
                //print(brakeSteering);
            }
            else
            {
                steeringTorque = 0;
                brakeSteering = 0;
            }


            motorTorque = (forwardTorque * directionCorrector * this.vessel.ctrlState.wheelThrottle) - (steeringTorque * this.vessel.ctrlState.wheelSteer); //forward and low speed steering torque. Direction controlled by precalulated directioncorrector
            brakeSteeringTorque = Mathf.Clamp(brakeSteering * this.vessel.ctrlState.wheelSteer, 0, 1000); //if the calculated value is negative, disregard: Only brake on inside track. no need to direction correct as we are using the velocity or the part not the vessel.
            chargeRequest = Math.Abs(motorTorque * 0.0005f); //calculate the requested charge

            electricCharge = part.RequestResource("ElectricCharge", chargeRequest); //ask the vessel for requested charge

            float freeWheelRPM = 0;
            foreach (WheelCollider wc in this.part.GetComponentsInChildren<WheelCollider>())
            {
                if (electricCharge == 0)
                {
                    motorTorque = 0;
                }
                wc.motorTorque = motorTorque;
                wc.brakeTorque = brakeTorque + brakeSteeringTorque;

                if (wc.isGrounded) //only count wheels in contact with the floor. Others will be freewheeling and will wreck the calculation.
                {
                    numberOfWheels++;
                    trackRPM += wc.rpm;
                }
                else if (wc.suspensionDistance != 0) //the sprocket colliders could be doing anything. Don't count them.
                {
                    freeWheelRPM += wc.rpm;
                }
            }
            if (numberOfWheels > 1)
            {
                averageTrackRPM = trackRPM / numberOfWheels;
            }
            else
            {
                averageTrackRPM = freeWheelRPM / 4;
            }
        }//end OnFixedUpdate

        public override void OnUpdate()
        {
            base.OnUpdate();
            degreesPerTick = (averageTrackRPM / 60) * Time.deltaTime * 360; //calculate how many degrees to rotate the wheel
            trackRPM = 0;
            float distanceTravelled = (float)((averageTrackRPM * 2 * Math.PI) / 60) * Time.deltaTime; //calculate how far the track will need to move
            Material trackMaterial = trackSurface.renderer.material;    //set things up for changing the texture offset on the track
            Vector2 textureOffset = trackMaterial.mainTextureOffset;
            textureOffset = textureOffset + new Vector2(-distanceTravelled / trackLength, 0); //tracklength is used to fine tune the speed of movement.
            trackMaterial.SetTextureOffset("_MainTex", textureOffset);
            trackMaterial.SetTextureOffset("_BumpMap", textureOffset);
            numberOfWheels = 1; //reset number of wheels. Setting to zero gives NaN!
        } //end OnUpdate

//Action groups
        [KSPAction("Brakes", KSPActionGroup.Brakes)]
        public void brakes(KSPActionParam param)
        {
            if (param.type == KSPActionType.Activate)
            {
                brakeTorque = brakingTorque * ((torque / 2) + .5f);
                brakesApplied = true;
            }
            else
            {
                brakeTorque = 0;
                brakesApplied = false;
            }
        }
        [KSPAction("Increase Torque")]
        public void increase(KSPActionParam param)
        {
            if (torque < 2)
            {
                torque += 0.25f;
            }

        }//End increase

        [KSPAction("Decrease Toqrque")]
        public void decrease(KSPActionParam param)
        {
            if (torque > 0)
            {
                torque -= 0.25f;
            }
        }//end decrease
        [KSPAction("Toggle Steering")]
        public void toggleSteering(KSPActionParam param)
        {
            steeringDisabled = !steeringDisabled;
        }//end toggle steering
        //end action groups
    }//end class
}//end namespace