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
        [KSPField(isPersistant = false, guiActive = true, guiName = "DirectionCorrector")]
        public int directionCorrector;
        public bool boundsDestroyed;
        public Vector3 referenceTranformVector;
        public Transform referenceTransform;
        public float myPosition;
        [KSPField(isPersistant = false, guiActive = true, guiName = "Reference Direction")]
        public int referenceDirection;
        [KSPField(isPersistant = true)]
        public bool brakesApplied;
        [KSPField(isPersistant = false, guiActive = true, guiName = "Min", guiFormat = "F6")]
        public float minPos;
        [KSPField(isPersistant = false, guiActive = true, guiName = "Max", guiFormat = "F6")]
        public float maxPos;
        [KSPField(isPersistant = false, guiActive = true, guiName = "Min to Max", guiFormat = "F6")]
        public float minToMax;
        [KSPField(isPersistant = false, guiActive = true, guiName = "Mid", guiFormat = "F6")]
        public float midPoint;
        [KSPField(isPersistant = false, guiActive = true, guiName = "Offset", guiFormat = "F6")]
        public float offset;

        [KSPField(isPersistant = false, guiActive = true, guiName = "Adjuted position", guiFormat = "F6")]
        public float myAdjustedPosition;
        [KSPField(isPersistant = false, guiActive = true, guiName = "Steering Ratio", guiFormat = "F6")]
        public float steeringRatio;

        [KSPField(isPersistant = true, guiActive = true, guiName = "Dot.X", guiFormat = "F6")]
        public float dotx; //debug only
        [KSPField(isPersistant = true, guiActive = true, guiName = "Dot.X Signed", guiFormat = "F6")]
        public float dotxSigned; //debug only
        [KSPField(isPersistant = true, guiActive = true, guiName = "Dot.Y", guiFormat = "F6")]
        public float doty; //debug only
        [KSPField(isPersistant = true, guiActive = true, guiName = "Dot.Z", guiFormat = "F6")]
        public float dotz; //debug only
        [KSPField(isPersistant = false, guiActive = true, guiName = "OrgPos", guiFormat = "F6")]
        public Vector3 orgpos;

        public string right = "right";
        public string forward = "forward";
        public string up = "up";

        [KSPField]
        public FloatCurve torqueCurve = new FloatCurve();
        [KSPField]
        public FloatCurve steeringCurve = new FloatCurve();
        [KSPField]
        public FloatCurve torqueSteeringCurve = new FloatCurve();
        [KSPField]
        public FloatCurve brakeSteeringCurve = new FloatCurve();
        [KSPField]
        public bool hasSteering = false;
        [KSPField]
        public float brakingTorque;
        [KSPField]
        public float rollingResistanceMultiplier;
        [KSPField]
        public float rollingResistance;
        [KSPField]
        public float smoothSpeed = 10;
        [KSPField]
        public float raycastError;
        [KSPField(isPersistant = false, guiActive = false, guiName = "RPM", guiFormat = "F1")]
        public float averageTrackRPM;
        [KSPField]
        public float maxRPM = 350;

        public float brakeTorque;
        [KSPField(isPersistant = false, guiActive = true, guiName = "BrakeSteering", guiFormat = "F1")]
        public float brakeSteering;
        public float degreesPerTick;
        public float motorTorque;
        public float groundedWheels = 0; //if it's 0 at the start it send things into and NaN fit.
        public float trackRPM = 0;

        public float steeringAngle;
        public float steeringAngleSmoothed;
        
        public float wheelCount;
        public float calculatedRollingResistance;

        //tweakables
        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Torque ratio"), UI_FloatRange(minValue = 0, maxValue = 2f, stepIncrement = .25f)]
        public float torque = 1;
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Spring strength"), UI_FloatRange(minValue = 0, maxValue = 6.00f, stepIncrement = 0.2f)]
        public float springRate;        //this is what's tweaked by the line above
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Spring Damping"), UI_FloatRange(minValue = 0, maxValue = 1.0f, stepIncrement = 0.025f)]
        public float damperRate;        //this is what's tweaked by the line above
        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Steering"), UI_Toggle(disabledText = "Enabled", enabledText = "Disabled")]
        public bool steeringDisabled;

        public List<WheelCollider> wcList = new List<WheelCollider>();

        public override void OnStart(PartModule.StartState start)  //when started
        {
            print("ModuleTrack called");
            base.OnStart(start);
            if (HighLogic.LoadedSceneIsEditor)
            {
                //Do nothing in editor
            }

            if (HighLogic.LoadedSceneIsFlight)
            {
                FindDirection();
                SetupRatios();
                DestroyBounds();

                if (torque > 2) //check if the torque value is using the old numbering system
                {
                    torque /= 100;
                }
                wheelCount = 0;
                this.part.force_activate(); //force the part active or OnFixedUpate is not called
                foreach (WheelCollider wc in this.part.GetComponentsInChildren<WheelCollider>()) //set colliders to values chosen in editor and activate
                {
                    wheelCount++;
                    JointSpring userSpring = wc.suspensionSpring;
                    userSpring.spring = springRate;
                    userSpring.damper = damperRate;
                    wc.suspensionSpring = userSpring;
                    wc.enabled = true;
                    wcList.Add(wc);
                }

                if (brakesApplied)
                {
                    brakeTorque = brakingTorque; //were the brakes left applied
                }

            }//end scene is flight
        }//end OnStart

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
                steeringTorque = torqueSteeringCurve.Evaluate((float)this.vessel.srfSpeed) * torque; //low speed steering mode. Differential motor torque
                brakeSteering = brakeSteeringCurve.Evaluate(travelDirection); //high speed steering. Brake on inside track because Unity seems to weight reverse motor torque less at high speed.
                steeringAngle = (steeringCurve.Evaluate((float)this.vessel.srfSpeed)) * -this.vessel.ctrlState.wheelSteer * steeringRatio; //low speed steering mode. Differential motor torque
            }
            else
            {
                steeringTorque = 0;
                brakeSteering = 0;
                steeringAngle = 0;
            }


            motorTorque = (forwardTorque * directionCorrector * this.vessel.ctrlState.wheelThrottle) - (steeringTorque * this.vessel.ctrlState.wheelSteer); //forward and low speed steering torque. Direction controlled by precalulated directioncorrector
            brakeSteeringTorque = Mathf.Clamp(brakeSteering * this.vessel.ctrlState.wheelSteer, 0, 1000); //if the calculated value is negative, disregard: Only brake on inside track. no need to direction correct as we are using the velocity or the part not the vessel.
            chargeRequest = Math.Abs(motorTorque * 0.0005f); //calculate the requested charge
            steeringAngleSmoothed = Mathf.Lerp(steeringAngleSmoothed, steeringAngle, Time.deltaTime * smoothSpeed);

            electricCharge = part.RequestResource("ElectricCharge", chargeRequest); //ask the vessel for requested charge

            float freeWheelRPM = 0;
            foreach (WheelCollider wc in wcList)
            {
                if (electricCharge == 0 || Math.Abs (averageTrackRPM) >= maxRPM)
                {
                    motorTorque = 0;
                }
                wc.motorTorque = motorTorque;
                wc.brakeTorque = brakeTorque + brakeSteeringTorque + rollingResistance;

                if (wc.isGrounded) //only count wheels in contact with the floor. Others will be freewheeling and will wreck the calculation. 
                {
                    groundedWheels++;
                    trackRPM += wc.rpm;
                }
                else if (wc.suspensionDistance != 0) //the sprocket colliders could be doing anything. Don't count them.
                {
                    freeWheelRPM += wc.rpm;
                }
                
                wc.steerAngle = steeringAngleSmoothed;
                //print(wc.steerAngle);
            }

            if (groundedWheels >= 1)
            {
                averageTrackRPM = trackRPM / groundedWheels; 
            }
            else
            {
                averageTrackRPM = freeWheelRPM / wheelCount;
            }
            trackRPM = 0;
            degreesPerTick = (averageTrackRPM / 60) * Time.deltaTime * 360; //calculate how many degrees to rotate the wheel
            groundedWheels = 0; //reset number of wheels. Setting to zero gives NaN!

        }//end OnFixedUpdate

        public override void OnUpdate()
        {
            base.OnUpdate(); 

        } //end OnUpdate

        public void FindDirection()
        {
            orgpos = this.part.orgPos;
            dotx = Math.Abs(Vector3.Dot(this.part.transform.forward, this.vessel.rootPart.transform.right)); // up is forward
            doty = Math.Abs(Vector3.Dot(this.part.transform.forward, this.vessel.rootPart.transform.up));
            dotz = Math.Abs(Vector3.Dot(this.part.transform.forward, this.vessel.rootPart.transform.forward));

            if (dotx > doty && dotx > dotz)
            {
                dotxSigned = Vector3.Dot(this.part.transform.forward, this.vessel.rootPart.transform.right);

                print("root part mounted right");
                //myPosition = this.part.orgPos.x;
                referenceTranformVector = this.vessel.ReferenceTransform.right;
                referenceDirection = 0;
            }
            if (doty > dotx && doty > dotz)
            {
                print("root part mounted forward");
                //myPosition = this.part.orgPos.y;
                referenceTranformVector = this.vessel.ReferenceTransform.forward;
                referenceDirection = 1;
            }
            if (dotz > doty && dotz > dotx)
            {
                print("root part mounted up");
                //myPosition = this.part.orgPos.z;
                referenceTranformVector = this.vessel.ReferenceTransform.up;
                referenceDirection = 2;
            }
            if (referenceDirection == 0)
            {
                referenceTranformVector.x = Math.Abs(referenceTranformVector.x);
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
        }

        public void SetupRatios()
        {
            myPosition = this.part.orgPos[referenceDirection];
            maxPos = this.part.orgPos[referenceDirection];
            minPos = this.part.orgPos[referenceDirection];

            foreach (ModuleTrack st in this.vessel.FindPartModulesImplementing<ModuleTrack>()) //scan vessel to find fore or rearmost wheel. 
            {
                float otherPosition = myPosition;
                otherPosition = st.part.orgPos[referenceDirection];

                if ((otherPosition + 1000) >= (maxPos + 1000)) //dodgy hack. Make sure all values are positive or we struggle to evaluate < or >
                    maxPos = otherPosition; //Store transform y value

                if ((otherPosition + 1000) <= (minPos + 1000))
                    minPos = otherPosition; //Store transform y value
            }

            minToMax = maxPos - minPos;
            midPoint = minToMax / 2;
            offset = (maxPos + minPos) / 2;
            myAdjustedPosition = myPosition - offset;

            steeringRatio = myAdjustedPosition / midPoint;

            if (steeringRatio == 0 || float.IsNaN(steeringRatio)) //check is we managed to evaluate to zero or infinity somehow.
                steeringRatio = 1;
        }

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

        [KSPAction("Decrease Torque")]
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
}//end namespaces