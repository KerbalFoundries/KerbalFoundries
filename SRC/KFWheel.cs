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

        //object types
        WheelCollider _wheelCollider;
        Transform _susTrav;
        Transform _wheel;
        Transform _trackSteering;
        KFModuleWheel _track;

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

                _track = this.part.GetComponentInChildren<KFModuleWheel>();

                susTravIndex = Extensions.SetAxisIndex(susTravAxis);
                steeringIndex = Extensions.SetAxisIndex(steeringAxis);

                if (_track.hasSteering)
                {
                    initialSteeringAngles = _trackSteering.transform.localEulerAngles;
                    //print(initialSteeringAngles);
                }

                if (useDirectionCorrector)
                    directionCorrector = _track.directionCorrector;
                else directionCorrector = 1;

                _wheelRotation = new Vector3(wheelRotationX, wheelRotationY, wheelRotationZ);

                if (lastFrameTraverse == 0) //check to see if we have a value in persistance
                {
                    Debug.LogError("Last frame = 0. Setting");
                    lastFrameTraverse = _wheelCollider.suspensionDistance;
                    Debug.LogError(lastFrameTraverse);
                }
                //Debug.LogError("Last frame =");
                //Debug.LogError(lastFrameTraverse);
                MoveSuspension(susTravIndex, -lastFrameTraverse, _susTrav); //to get the initial stuff correct

                if (_track.hasSteering)
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
            newSteeringAngle[steeringIndex] += _track.steeringAngleSmoothed;
            _trackSteering.transform.localEulerAngles = newSteeringAngle;
            yield return null;
            }
        }
        IEnumerator TrackedWheel() //coroutine for tracked wheels (all rotate the same speed in the part) 
        {
            while (true)
            {
                _wheel.transform.Rotate(_wheelRotation, _track.degreesPerTick * directionCorrector * rotationCorrection); //rotate wheel
                yield return null;
            }
        }
        IEnumerator IndividualWheel() //coroutine for individual wheels
        {
            while (true)
            {
                degreesPerTick = (_wheelCollider.rpm / 60) * Time.deltaTime * 360;
                _wheel.transform.Rotate(_wheelRotation, degreesPerTick * directionCorrector * rotationCorrection); //rotate wheel
                yield return null;
            }
        }

        IEnumerator Suspension()
        {
            while (true)
            {
                _wheelCollider.suspensionDistance = suspensionDistance * _track.appliedRideHeight;
                //suspension movement
                WheelHit hit;
                float frameTraverse = 0;
                bool grounded = _wheelCollider.GetGroundHit(out hit); //set up to pass out wheelhit coordinates
                float tempLastFrameTraverse = lastFrameTraverse;
                if (grounded && !isSprocket) //is it on the ground
                {
                    frameTraverse = -_wheelCollider.transform.InverseTransformPoint(hit.point).y + _track.raycastError - _wheelCollider.radius;
                    lastFrameTraverse = frameTraverse;
                }
                else
                {
                    frameTraverse = lastFrameTraverse; //movement defaults back to zero when not grounded
                }

                newTranslation = tempLastFrameTraverse - frameTraverse;
                MoveSuspension(susTravIndex, newTranslation, _susTrav);
                //end suspension movement
                yield return null;
            }
        }

        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
            

            
        }//end OnFixedUpdate

        public void MoveSuspension(int index, float movement, Transform movedObject)
        {
            Vector3 tempVector = new Vector3(0, 0, 0);
            tempVector[index] = movement;
            movedObject.transform.Translate(tempVector, Space.Self);
        }
    }//end modele
}//end class