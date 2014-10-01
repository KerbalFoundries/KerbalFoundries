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
        [KSPField(isPersistant = false, guiActive = true, guiName = "SteeringCorrector")]
        public int steeringCorrector = 1;
        [KSPField(isPersistant = true, guiActive = false, guiName = " Steering Invert")]
        public float steeringInvert = 1;


        public bool boundsDestroyed;


        public float myPosition;

        [KSPField(isPersistant = false, guiActive = true, guiName = "Long Index")]
        public int rootIndexLong;
        [KSPField(isPersistant = false, guiActive = true, guiName = "Lat Index")]
        public int rootIndexLat;
        [KSPField(isPersistant = false, guiActive = true, guiName = "Up Index")]
        public int rootIndexUp;

        [KSPField(isPersistant = false, guiActive = true, guiName = "Control Index")]
        public int controlAxisIndex;
        [KSPField(isPersistant = false, guiActive = false, guiName = "Steering Temp")]
        public int steeringTemp;

        [KSPField(isPersistant = true)] 
        public bool brakesApplied;
        [KSPField(isPersistant = false, guiActive = false, guiName = "Min", guiFormat = "F6")]
        public float minPos;
        [KSPField(isPersistant = false, guiActive = false, guiName = "Max", guiFormat = "F6")]
        public float maxPos;
        [KSPField(isPersistant = false, guiActive = false, guiName = "Min to Max", guiFormat = "F6")]
        public float minToMax;
        [KSPField(isPersistant = false, guiActive = false, guiName = "Mid", guiFormat = "F6")]
        public float midPoint;
        [KSPField(isPersistant = false, guiActive = false, guiName = "Offset", guiFormat = "F6")]
        public float offset;

        [KSPField(isPersistant = false, guiActive = true, guiName = "Adjuted position", guiFormat = "F6")]
        public float myAdjustedPosition;
        [KSPField(isPersistant = false, guiActive = true, guiName = "Steering Ratio", guiFormat = "F6")]
        public float steeringRatio;

        [KSPField(isPersistant = false, guiActive = false, guiName = "Dot.X", guiFormat = "F6")]
        public float dotx; //debug only
        [KSPField(isPersistant = false, guiActive = false, guiName = "Dot.X Signed", guiFormat = "F6")]
        public float dotxSigned; //debug only
        [KSPField(isPersistant = false, guiActive = false, guiName = "Dot.Up", guiFormat = "F6")]
        public float dotUp; //debug only
        [KSPField(isPersistant = false, guiActive = true, guiName = "Dot.Y", guiFormat = "F6")]
        public float doty; //debug only
        [KSPField(isPersistant = false, guiActive = true, guiName = "Dot.Z", guiFormat = "F6")]
        public float dotz; //debug only
        [KSPField(isPersistant = false, guiActive = false, guiName = "OrgPos", guiFormat = "F6")]
        public Vector3 orgpos;

        public uint commandId;
        public uint lastCommandId;

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
        [KSPField]
        public float chargeConsumptionRate = 1f;

        public float brakeTorque;
        [KSPField(isPersistant = false, guiActive = true, guiName = "BrakeSteering", guiFormat = "F1")]
        public float brakeSteering;
        public float degreesPerTick;
        public float motorTorque;
        public int groundedWheels = 0; //if it's 0 at the start it send things into and NaN fit.
        public float trackRPM = 0;

        public float steeringAngle;
        public float steeringAngleSmoothed;
        
        public int wheelCount;
        public float calculatedRollingResistance;

        float effectPower;

        //tweakables
        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Group Number"), UI_FloatRange(minValue = 0, maxValue = 10f, stepIncrement = 1f)]
        public float groupNumber = 1;
        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Torque ratio"), UI_FloatRange(minValue = 0, maxValue = 2f, stepIncrement = .25f)]
        public float torque = 1;
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Spring strength"), UI_FloatRange(minValue = 0, maxValue = 6.00f, stepIncrement = 0.2f)]
        public float springRate;        //this is what's tweaked by the line above
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Spring Damping"), UI_FloatRange(minValue = 0, maxValue = 1.0f, stepIncrement = 0.025f)]
        public float damperRate;        //this is what's tweaked by the line above
        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Steering"), UI_Toggle(disabledText = "Enabled", enabledText = "Disabled")]
        public bool steeringDisabled;
        [KSPField(isPersistant = false, guiActive = true, guiName = "Status")]
        public string status = "Nominal";

        public List<WheelCollider> wcList = new List<WheelCollider>();

        public override void OnStart(PartModule.StartState start)  //when started
        {
            
            base.OnStart(start);
            print(Version.versionNumber);

            if (HighLogic.LoadedSceneIsEditor)
            {
                //Do nothing in editor
            }

            if (HighLogic.LoadedSceneIsFlight)
            {
                // wheel steering ratio setup
                rootIndexLong = GetRefAxis(this.part.transform.forward, this.vessel.rootPart.transform); //Find the root part axis which matches each wheel axis.
                rootIndexLat = GetRefAxis(this.part.transform.right, this.vessel.rootPart.transform);
                rootIndexUp = GetRefAxis(this.part.transform.up, this.vessel.rootPart.transform);

                steeringRatio = SetupRatios(rootIndexLong); //use the axis which corresponds to the forward axis of the wheel.

                GetControlAxis(); // sets up motor and steering direction direction

                DestroyBounds(); //destroys the Bounds helper object if it is still in the model.

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

        public void WheelSound()
        {
            part.Effect("WheelEffect", effectPower);
        }

        public override void OnFixedUpdate()
        {
            //User input
            float electricCharge;
            
            float forwardTorque = torqueCurve.Evaluate((float)this.vessel.srfSpeed) * torque; //this is used a lot, so may as well calculate once
            float steeringTorque;
            float brakeSteeringTorque;

            Vector3 travelVector = this.vessel.GetSrfVelocity();

            float travelDirection = Vector3.Dot(this.part.transform.forward, travelVector); //compare travel velocity with the direction the part is pointed.
            //print(travelDirection);

            if (!steeringDisabled)
            {
                steeringTorque = torqueSteeringCurve.Evaluate((float)this.vessel.srfSpeed) * torque * steeringInvert; //low speed steering mode. Differential motor torque
                brakeSteering = brakeSteeringCurve.Evaluate(travelDirection) * steeringInvert; //high speed steering. Brake on inside track because Unity seems to weight reverse motor torque less at high speed.
                steeringAngle = (steeringCurve.Evaluate((float)this.vessel.srfSpeed)) * -this.vessel.ctrlState.wheelSteer * steeringRatio * steeringCorrector * steeringInvert; //low speed steering mode. Differential motor torque
            }
            else
            {
                steeringTorque = 0;
                brakeSteering = 0;
                steeringAngle = 0;
            }


            motorTorque = (forwardTorque * directionCorrector * this.vessel.ctrlState.wheelThrottle) - (steeringTorque * this.vessel.ctrlState.wheelSteer); //forward and low speed steering torque. Direction controlled by precalulated directioncorrector
            brakeSteeringTorque = Mathf.Clamp(brakeSteering * this.vessel.ctrlState.wheelSteer, 0, 1000); //if the calculated value is negative, disregard: Only brake on inside track. no need to direction correct as we are using the velocity or the part not the vessel.
            //chargeRequest = Math.Abs(motorTorque * 0.0005f); //calculate the requested charge
            steeringAngleSmoothed = Mathf.Lerp(steeringAngleSmoothed, steeringAngle, Time.deltaTime * smoothSpeed);

            float chargeConsumption = Time.deltaTime * chargeConsumptionRate * (Math.Abs(motorTorque) /100);
            //print(chargeConsumption);
            electricCharge = part.RequestResource("ElectricCharge", chargeConsumption);

            float freeWheelRPM = 0;
            foreach (WheelCollider wc in wcList)
            {
                if (electricCharge != chargeConsumption)
                {
                    motorTorque = 0;
                    status = "Low Charge"; 
                }
                else if (Math.Abs(averageTrackRPM) >= maxRPM)
                {
                    motorTorque = 0;
                    status = "Rev Limit";
                }
                else
                {
                    status = "Nominal";
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
            degreesPerTick = (averageTrackRPM / 60) * Time.deltaTime * 360; //calculate how many degrees to rotate the wheel mesh
            groundedWheels = 0; //reset number of wheels.

        }//end OnFixedUpdate

        public override void OnUpdate()
        {
            base.OnUpdate();
            commandId = this.vessel.referenceTransformId;
            if (commandId != lastCommandId)
            {
                print("Control Axis Changed");
                GetControlAxis();
            }
            lastCommandId = commandId;

            effectPower = Math.Abs(averageTrackRPM / maxRPM);
            //print(effectPower);
            WheelSound();
        } //end OnUpdate

        public void GetControlAxis()
        {
            controlAxisIndex = GetRefAxis(this.part.transform.forward, this.vessel.ReferenceTransform); //grab current values for the part and the control module, which may ahve changed.
            directionCorrector = GetCorrector(this.part.transform.forward, this.vessel.ReferenceTransform, controlAxisIndex); // dets the motor direction correction again.
            if (controlAxisIndex == rootIndexLat)       //uses the precalulated forward (as far as this part is concerned) to determined the orientation of the control module
                steeringCorrector = GetCorrector(this.vessel.ReferenceTransform.up, this.vessel.rootPart.transform, rootIndexLat);
            if (controlAxisIndex == rootIndexLong)
                steeringCorrector = GetCorrector(this.vessel.ReferenceTransform.up, this.vessel.rootPart.transform, rootIndexLong);
            if (controlAxisIndex == rootIndexUp)
                steeringCorrector = GetCorrector(this.vessel.ReferenceTransform.up, this.vessel.rootPart.transform, rootIndexUp);
            steeringTemp = steeringCorrector; //This is for debugging
        }


        public int GetRefAxis(Vector3 refDirection, Transform refTransform) //takes a vector 3 derived from the axis of the parts transform (typically), and the transform of the part to compare to (usually the root part)
        {                                                                   // uses scalar products to determine which axis is closest to the axis specified in refDirection, return an index value 0 = X, 1 = Y, 2 = Z
            //orgpos = this.part.orgPos; //debugguing
            dotx = Math.Abs(Vector3.Dot(refDirection, refTransform.right)); // up is forward
            print(dotx); //debugging
            doty = Math.Abs(Vector3.Dot(refDirection, refTransform.up));
            print(doty); //debugging
            dotz = Math.Abs(Vector3.Dot(refDirection, refTransform.forward));
            print(dotz); //debugging

            int orientationIndex = 0;

            if (dotx > doty && dotx > dotz)
            {
                dotxSigned = Vector3.Dot(refDirection, refTransform.right);

                print("root part mounted right");
                orientationIndex = 0;
            }
            if (doty > dotx && doty > dotz)
            {
                print("root part mounted forward");
                orientationIndex = 1;
            }
            if (dotz > doty && dotz > dotx)
            {
                print("root part mounted up");
                orientationIndex = 2;
            }
            /*
            if (referenceDirection == 0)
            {
                referenceTranformVector.x = Math.Abs(referenceTranformVector.x);
            }
             * */
            return orientationIndex;
        }

        public int GetCorrector(Vector3 transformVector, Transform referenceVector, int directionIndex) // takes a vector (usually from a parts axis) and a transform, plus an index giving which axis to   
        {                                                                                               // use for the scalar product of the two. Returns a value of -1 or 1, depending on whether the product is positive or negative.
            int corrector = 1;
            float dot = 0;

            if (directionIndex == 0)
            {
                dot = Vector3.Dot(transformVector, referenceVector.right); // up is forward
                
            }
            if (directionIndex == 1)
            {
                dot = Vector3.Dot(transformVector, referenceVector.up); // up is forward
            }
            if (directionIndex == 2)
            {
                dot = Vector3.Dot(transformVector, referenceVector.forward); // up is forward
            }

            print(dot);

            if (dot < 0) // below 0 means the engine is on the left side of the craft
            {
                corrector = -1;
                print("left");
            }
            else
            {
                corrector = 1;
                print("right");
            }
            return corrector;
        }

        public float SetupRatios(int refIndex)      // Determines how much this wheel should be steering according to its position in the craft. Returns a value -1 to 1.
        {
            myPosition = this.part.orgPos[refIndex];
            maxPos = this.part.orgPos[refIndex];
            minPos = this.part.orgPos[refIndex];
            float ratio = 1;
            foreach (ModuleTrack st in this.vessel.FindPartModulesImplementing<ModuleTrack>()) //scan vessel to find fore or rearmost wheel. 
            {
                if (st.groupNumber == groupNumber && groupNumber != 0)
                {
                    float otherPosition = myPosition;
                    otherPosition = st.part.orgPos[refIndex];

                    if ((otherPosition + 1000) >= (maxPos + 1000)) //dodgy hack. Make sure all values are positive or we struggle to evaluate < or >
                        maxPos = otherPosition; //Store transform y value

                    if ((otherPosition + 1000) <= (minPos + 1000))
                        minPos = otherPosition; //Store transform y value
                }
            }

            minToMax = maxPos - minPos;
            midPoint = minToMax / 2;
            offset = (maxPos + minPos) / 2;
            myAdjustedPosition = myPosition - offset;

            ratio = myAdjustedPosition / midPoint;

            if (ratio == 0 || float.IsNaN(ratio)) //check is we managed to evaluate to zero or infinity somehow. Happens with less than three wheels, or all wheels mounted at the same position.
                ratio = 1;
            print("ratio"); //Debugging
            print(ratio);
            return ratio;
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
        [KSPAction("Invert Steering")]
        public void InvertSteeringAG(KSPActionParam param)
        {
            InvertSteering();
        }//end toggle steering
        //end action groups
        
        [KSPEvent(guiActive = true, guiName = "Invert Steering ", active = true)]
        public void InvertSteering()
        {
            steeringInvert *= -1;
        }

        [KSPEvent(guiActive = true, guiName = "Apply Settings", active = true)]
        public void ApplySettings()
        {
            foreach (ModuleTrack mt in this.vessel.FindPartModulesImplementing<ModuleTrack>())
            {
                if (groupNumber != 0 && groupNumber == mt.groupNumber)
                {
                    mt.steeringInvert = steeringInvert;
                    mt.torque = torque;
                }
                mt.steeringRatio = mt.SetupRatios(mt.rootIndexLong);
            }

            
        }

    }//end class
}//end namespaces