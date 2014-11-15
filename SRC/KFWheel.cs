using System;
using System.Collections.Generic;
using System.Collections;  
using System.Linq;
using System.Text;
using UnityEngine;

namespace KerbalFoundries
{

    public class KFWheel : PartModule
    {
        [KSPField(isPersistant = false, guiActive = true, guiName = "Suspension travel")]
        public float susTravel;

        //config fields
        [KSPField]
        public string wheelName;
        [KSPField]
        public string colliderName;
        [KSPField]
        public string sustravName;
        [KSPField]
        public string steeringName;
        [KSPField]
        public bool useDirectionCorrector = false; //make sure it's set to false if not specified in the config
        [KSPField]
        public bool isSprocket = false;
        [KSPField]
        public bool hasSuspension = true;
        [KSPField]
        public float smoothSpeed = 40; 
        [KSPField]
        public float rotationCorrection = 1;
        [KSPField]
        public bool trackedWheel = true; //default to tracked type (average of all colliders in contact with floor). This is OK for wheels, and will only need to be changed for multi wheeled parts that are not tracks 

            //wheel rotation axis
        [KSPField]
        public float wheelRotationX = 1;
        [KSPField]
        public float wheelRotationY = 0;
        [KSPField]
        public float wheelRotationZ = 0;
            //suspension traverse axis
        [KSPField]
        public string susTravAxis = "Y";
        [KSPField]
        public string steeringAxis = "Y";
        [KSPField(isPersistant = true)]
        public float lastFrameTraverse;

        //persistent fields. Not to be used for config
        [KSPField(isPersistant = true)]
        public float suspensionDistance;
        [KSPField(isPersistant = true)]
        public float suspensionSpring;
        [KSPField(isPersistant = true)]
        public float suspensionDamper;
        [KSPField(isPersistant = true)]
        public bool isConfigured = false;
        [KSPField(isPersistant = true)]
        public int suspensionCorrector = 1;

        //object types
        WheelCollider _wheelCollider;
        Transform _susTrav;
        Transform _wheel;
        Transform _trackSteering;
        KFModuleWheel _KFModuleWheel;

        //gloabl variables

        Vector3 initialSteeringAngles;
        float newTranslation;
        Vector3 _wheelRotation;
        int susTravIndex = 1;
        int steeringIndex = 1;
        public int directionCorrector = 1;
        
        float degreesPerTick;
        

        //OnStart
        public override void OnStart(PartModule.StartState state)
        {
            if (!isConfigured)
            {
                foreach (WheelCollider wc in this.part.GetComponentsInChildren<WheelCollider>())
                {
                    if (wc.name.StartsWith(colliderName, StringComparison.Ordinal))
                    {
                        _wheelCollider = wc;
                        suspensionDistance = wc.suspensionDistance;
                        Debug.LogError("suspensionDistance is" + suspensionDistance);
                        isConfigured = true;
                    }
                    else
                    {
                        Debug.LogError("Wheel Collider" + _wheelCollider + " not found. Disabling module");
                    }
                }
            }
            else
            {
                Debug.LogError("Already configured - skipping");
            }

            if (HighLogic.LoadedSceneIsEditor)
            {

            }
            
            if (HighLogic.LoadedSceneIsFlight && isConfigured)
            {
                //find named onjects in part
                foreach (WheelCollider wc in this.part.GetComponentsInChildren<WheelCollider>())
                {
                    if (wc.name.StartsWith(colliderName, StringComparison.Ordinal))
                    {
                        _wheelCollider = wc;
                    }
                }
                foreach (Transform tr in this.part.GetComponentsInChildren<Transform>())
                {
                    if (tr.name.StartsWith(wheelName, StringComparison.Ordinal))
                    {
                        _wheel = tr;
                    }
                    if (tr.name.StartsWith(steeringName, StringComparison.Ordinal))
                    {
                        _trackSteering = tr;
                    }
                    if (tr.name.StartsWith(sustravName, StringComparison.Ordinal))
                    {
                        _susTrav = tr;
                    }
                    
                }
                //end find named objects

                _KFModuleWheel = this.part.GetComponentInChildren<KFModuleWheel>();

                susTravIndex = Extensions.SetAxisIndex(susTravAxis);
                steeringIndex = Extensions.SetAxisIndex(steeringAxis);

                if (_KFModuleWheel.hasSteering)
                {
                    initialSteeringAngles = _trackSteering.transform.localEulerAngles;
                    //print(initialSteeringAngles);
                }

                if (useDirectionCorrector)
                    directionCorrector = _KFModuleWheel.directionCorrector;
                else directionCorrector = 1;

                _wheelRotation = new Vector3(wheelRotationX, wheelRotationY, wheelRotationZ);

                if (lastFrameTraverse == 0) //check to see if we have a value in persistance
                {
                    Debug.LogError("Last frame = 0. Setting");
                    lastFrameTraverse = _wheelCollider.suspensionDistance * suspensionCorrector;
                    Debug.LogError(lastFrameTraverse);
                }
                //Debug.LogError("Last frame =");
                //Debug.LogError(lastFrameTraverse);
                MoveSuspension(susTravIndex, -lastFrameTraverse, _susTrav); //to get the initial stuff correct

                if (_KFModuleWheel.hasSteering)
                {
                    StartCoroutine(Steering());
                    Debug.LogError("starting steering coroutine");
                }
                if (trackedWheel)
                {
                    StartCoroutine(TrackedWheel());
                }
                else
                {
                    StartCoroutine(IndividualWheel());
                }
                if(hasSuspension)
                {
                    StartCoroutine(Suspension());
                }
                this.part.force_activate();
            }//end flight
            base.OnStart(state);
        }//end OnStart
        //OnUpdate

        IEnumerator Steering() //Coroutine for steering
        {
            while(true)
            {
            Vector3 newSteeringAngle = initialSteeringAngles;
            newSteeringAngle[steeringIndex] += _KFModuleWheel.steeringAngleSmoothed;
            _trackSteering.transform.localEulerAngles = newSteeringAngle;
            yield return null;
            }
        }
        IEnumerator TrackedWheel() //coroutine for tracked wheels (all rotate the same speed in the part) 
        {
            while (true)
            {
                _wheel.transform.Rotate(_wheelRotation, _KFModuleWheel.degreesPerTick * directionCorrector * rotationCorrection); //rotate wheel
                yield return null;
            }
        }
        IEnumerator IndividualWheel() //coroutine for individual wheels
        {
            while (true)
            {
                degreesPerTick = (_wheelCollider.rpm / 60) * Time.deltaTime * 360;
                _wheel.transform.Rotate(_wheelRotation, degreesPerTick * directionCorrector * rotationCorrection); //rotate wheel
                yield return new WaitForFixedUpdate();
            }
        }

        IEnumerator Suspension() //Coroutine for wheels with suspension.
        {
            while (true)
            {
                _wheelCollider.suspensionDistance = suspensionDistance * _KFModuleWheel.appliedRideHeight;
                //suspension movement
                WheelHit hit; //set this up to grab sollider raycast info
                float frameTraverse = 0;
                bool grounded = _wheelCollider.GetGroundHit(out hit); //set up to pass out wheelhit coordinates
                float tempLastFrameTraverse = lastFrameTraverse; //we need the value, but will over-write shortly. Store it here.
                if (grounded) //is it on the ground
                {
                    frameTraverse = -_wheelCollider.transform.InverseTransformPoint(hit.point).y + _KFModuleWheel.raycastError - _wheelCollider.radius; //calculate suspension travel using the collider raycast.
                    if (frameTraverse > (_wheelCollider.suspensionDistance + _KFModuleWheel.raycastError)) //the raycast sometimes goes further than its max value. Catch and stop the mesh moving further
                    {
                        frameTraverse = _wheelCollider.suspensionDistance;
                    }
                        /*
                    else if (frameTraverse < 0) //the raycast can be negative (!); catch this too
                    {
                        frameTraverse = 0;
                    }
                         * */
                    //print(frameTraverse);
                    frameTraverse *= suspensionCorrector;
                    lastFrameTraverse = frameTraverse;
                }
                else
                {
                    frameTraverse = lastFrameTraverse; //movement defaults back to last position when the collider is not grounded. Ungrounded collider returns suspension travel of zero!
                }
                susTravel = frameTraverse;

                newTranslation = tempLastFrameTraverse - frameTraverse; // calculate the change of movement. Using Translate on susTrav, which is cumulative, not absolute.
                MoveSuspension(susTravIndex, newTranslation, _susTrav); //move suspension in its configured direction by the amount calculated for this frame. 
                //end suspension movement
                yield return null;
            }
        }

        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
            
            //not a lot in here since I moved it all into coroutines.
            
        }//end OnFixedUpdate

        public void MoveSuspension(int index, float movement, Transform movedObject) //susTrav Axis, amount to move, named object.
        {
            Vector3 tempVector = new Vector3(0, 0, 0);
            tempVector[index] = movement;
            movedObject.transform.Translate(tempVector, Space.Self);
        }
    }//end modele
}//end class